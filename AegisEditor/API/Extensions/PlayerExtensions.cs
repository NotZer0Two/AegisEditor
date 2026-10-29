using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdminToys;
using AegisEditor.API.Enum;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;

namespace AegisEditor.API.Extensions;

/// <summary>
/// Extensions for <see cref="Player"/>.
/// </summary>
public static class PlayerExtensions
{
    private const int ChangeParentHash = -342419096;
    
    private static readonly Dictionary<string, ulong> SyncVarDirtyBitsValue = new();
    private static readonly Dictionary<Type, MethodInfo> WriterExtensionsValue = new();

    private static readonly ReadOnlyDictionary<Type, MethodInfo> ReadOnlyWriterExtensionsValue =
        new(WriterExtensionsValue);

    private static readonly ReadOnlyDictionary<string, ulong>
        ReadOnlySyncVarDirtyBitsValue = new(SyncVarDirtyBitsValue);

    private static readonly Dictionary<short, OpCode> OpCodeTable = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Select(f => (OpCode)f.GetValue(null))
        .ToDictionary(o => o.Value);

    /// <summary>
    /// Gets a all DirtyBit <see cref="ulong"/> from <see cref="StringExtensions"/>(format:classname.methodname).
    /// </summary>
    public static ReadOnlyDictionary<string, ulong> SyncVarDirtyBits
    {
        get
        {
            if (SyncVarDirtyBitsValue.Count == 0)
            {
                foreach (PropertyInfo property in typeof(ServerConsole).Assembly.GetTypes()
                             .SelectMany(x => x.GetProperties())
                             .Where(m => m.Name.StartsWith("Network")))
                {
                    MethodBody methodBody = property.GetSetMethod()?.GetMethodBody();

                    if (methodBody is null)
                        continue;

                    string key = $"{property.ReflectedType.Name}.{property.Name}";

                    if (SyncVarDirtyBitsValue.ContainsKey(key))
                        continue;

                    if (TryReadDirtyBit(methodBody.GetILAsByteArray(), out ulong bit))
                        SyncVarDirtyBitsValue.Add(key, bit);
                }
            }

            return ReadOnlySyncVarDirtyBitsValue;
        }
    }

    /// <summary>
    /// Gets <see cref="MethodInfo"/> corresponding to <see cref="Type"/>.
    /// </summary>
    public static ReadOnlyDictionary<Type, MethodInfo> WriterExtensions
    {
        get
        {
            if (WriterExtensionsValue.Count == 0)
            {
                foreach (MethodInfo method in typeof(NetworkWriterExtensions).GetMethods().Where(x =>
                             !x.IsGenericMethod && x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null &&
                             (x.GetParameters()?.Length == 2)))
                    WriterExtensionsValue.Add(
                        method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType,
                        method);

                Type generatedCode = Assembly.GetAssembly(typeof(RoleTypeId)).GetType("Mirror.GeneratedNetworkCode");
                foreach (MethodInfo method in generatedCode.GetMethods().Where(x =>
                             !x.IsGenericMethod && (x.GetParameters()?.Length == 2) && (x.ReturnType == typeof(void))))
                    WriterExtensionsValue.Add(
                        method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType,
                        method);

                foreach (Type serializer in typeof(ServerConsole).Assembly.GetTypes()
                             .Where(x => x.Name.EndsWith("Serializer")))
                {
                    foreach (MethodInfo method in serializer.GetMethods()
                                 .Where(x => (x.ReturnType == typeof(void)) && x.Name.StartsWith("Write")))
                        WriterExtensionsValue.Add(
                            method.GetParameters().First(x => x.ParameterType != typeof(NetworkWriter)).ParameterType,
                            method);
                }
            }

            return ReadOnlyWriterExtensionsValue;
        }
    }

    private static readonly List<(string propertyName, object value)> Buffer = new();

    /// <summary>
    /// Sends a fake SyncVar update for a single property.
    /// </summary>
    public static void SendFakeSyncVar<T>(this Player target, uint netId, Type targetType, string propertyName, T value)
    {
        Buffer.Clear();
        Buffer.Add((propertyName, value));
        target.SendFakeSyncVar(netId, targetType, Buffer);
        Buffer.Clear();
    }

    /// <summary>
    /// Sends a fake SyncVar update for the changed <see cref="PropertyUpdate"/> values.
    /// </summary>
    public static void SendFakeSyncVar(this Player target, uint netId, Type targetType,
        List<(PropertyUpdate property, object value)> values)
    {
        Buffer.Clear();

        foreach ((PropertyUpdate property, object value) in values)
        {
            // Lets add it here too so in case some genius has the idea to push it to the client lol
            if(property == PropertyUpdate.Parent)
                continue;
            
            Buffer.Add((property.ToSyncVarName(), value));
        }

        target.SendFakeSyncVar(netId, targetType, Buffer);
        Buffer.Clear();
    }

    /// <summary>
    /// Sends a fake SyncVar update for the named properties.
    /// </summary>
    public static void SendFakeSyncVar(this Player target, uint netId, Type targetType,
        List<(string propertyName, object value)> values)
    {
        bool isAdminToy = typeof(AdminToyBase).IsAssignableFrom(targetType);
        List<(ulong bit, object value)> entries = new();
        ulong mask = 0;

        foreach ((string propertyName, object value) in values)
        {
            if (!TryGetMask(targetType, propertyName, out ulong bit) || bit == 0)
                continue;

            entries.Add((bit, value));
            mask |= bit;
        }

        if (mask == 0)
            return;

        entries.Sort((a, b) => a.bit.CompareTo(b.bit));

        bool hasDerived = isAdminToy && mask > 16;

        using var writer = NetworkWriterPool.Get();

        Compression.CompressVarUInt(writer, 1UL);

        int headerPos = writer.Position;
        writer.WriteByte(0);
        int payloadStart = writer.Position;

        writer.WriteULong(0);
        writer.WriteULong(mask);

        if (!WriteValues(writer, entries, isAdminToy, false))
            return;

        if (isAdminToy)
        {
            writer.WriteULong(hasDerived ? mask : 0);
            if (!WriteValues(writer, entries, isAdminToy, true))
                return;
        }

        int payloadEnd = writer.Position;
        writer.Position = headerPos;
        writer.WriteByte((byte)((payloadEnd - payloadStart) & 0xFF));
        writer.Position = payloadEnd;

        target.Connection.Send(new EntityStateMessage
        {
            netId = netId,
            payload = writer.ToArraySegment()
        });
    }

    private static bool TryGetMask(Type targetType, string propertyName, out ulong mask)
    {
        string name = propertyName.StartsWith("Network") ? propertyName : "Network" + propertyName;
        return SyncVarDirtyBits.TryGetValue($"{targetType.Name}.{name}", out mask);
    }

    private static bool WriteValues(NetworkWriter writer, List<(ulong bit, object value)> entries, bool isAdminToy,
        bool derived)
    {
        foreach ((ulong bit, object value) in entries)
        {
            if ((isAdminToy && bit > 16) != derived)
                continue;

            if (!WriterExtensions.TryGetValue(value.GetType(), out MethodInfo writerMethod))
                return false;

            writerMethod.Invoke(null, new object[] { writer, value });
        }

        return true;
    }

    private static bool TryReadDirtyBit(byte[] il, out ulong bit)
    {
        bit = 0;
        int i = 0;

        while (i < il.Length)
        {
            short code = il[i++];

            if (code == 0xFE && i < il.Length)
                code = unchecked((short)(0xFE00 | il[i++]));

            if (!OpCodeTable.TryGetValue(code, out OpCode op))
                return false;

            if (op == OpCodes.Ldc_I8)
            {
                if (i + 8 > il.Length)
                    return false;

                bit = BitConverter.ToUInt64(il, i);
                return true;
            }

            switch (op.OperandType)
            {
                case OperandType.InlineNone:
                    break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar:
                    i += 1;
                    break;
                case OperandType.InlineVar:
                    i += 2;
                    break;
                case OperandType.InlineI8:
                case OperandType.InlineR:
                    i += 8;
                    break;
                case OperandType.InlineSwitch:
                    i += 4 + (BitConverter.ToInt32(il, i) * 4);
                    break;
                default:
                    i += 4;
                    break;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Tells the client to parent an admin toy to another networked object, same message as the game's RpcChangeParent.
    /// </summary>
    public static void SendChangeParent(this Player target, uint netId, uint parentId)
    {
        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        writer.WriteUInt(parentId);

        target.Connection.Send(new RpcMessage
        {
            netId = netId,
            componentIndex = 0,
            functionHash = unchecked((ushort)ChangeParentHash),
            payload = writer.ToArraySegment(),
        });
    }
}