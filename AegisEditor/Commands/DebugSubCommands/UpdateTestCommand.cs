using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AdminToys;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using CommandSystem;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Sends fake sync var changes for a network id to the sender.
/// </summary>
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class UpdateTestCommand : ICommand
{
    /// <inheritdoc />
    public string Command => "update";

    /// <inheritdoc />
    public string[] Aliases { get; } = [];

    /// <inheritdoc />
    public string Description => "Sends fake sync var changes for a network id, for you only.";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 2 || !uint.TryParse(arguments.At(0), out uint netId))
        {
            response = "Usage: aegisupdate <netId> <property=value> [property=value]...\n"
                       + "Properties: " + string.Join(", ", Enum.GetNames(typeof(PropertyUpdate))) + "\n"
                       + "Vector: x,y,z | Rotation: euler x,y,z | Color: red, #ff0000, r,g,b[,a] | Enums: by name";
            return false;
        }

        Player player = Player.Get(sender);
        if (player == null)
        {
            response = "Run this as a player.";
            return false;
        }

        Type type = null;
        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity)
            && identity.TryGetComponent(out AdminToyBase toy))
            type = toy.GetType();

        List<(PropertyUpdate property, object value)> changes = new();

        for (int i = 1; i < arguments.Count; i++)
        {
            string argument = arguments.At(i);
            int separator = argument.IndexOf('=');

            if (separator <= 0 || separator == argument.Length - 1)
            {
                response = $"Invalid argument \"{argument}\", use property=value.";
                return false;
            }

            string name = argument.Substring(0, separator);
            string text = argument.Substring(separator + 1);

            if (!Enum.TryParse(name, true, out PropertyUpdate property))
            {
                response = $"Unknown property \"{name}\".";
                return false;
            }

            TryResolve(property, out Type toyType, out Type valueType);

            if (!TryParse(valueType, text, out object value))
            {
                response = $"Invalid value \"{text}\" for {property}.";
                return false;
            }

            type ??= toyType;
            changes.Add((property, value));
        }

        type ??= typeof(AdminToyBase);
        player.SendFakeSyncVar(netId, type, changes);

        response = $"Sent {changes.Count} change(s) to {netId} as {type.Name}.";
        return true;
    }

    private static bool TryResolve(PropertyUpdate property, out Type toy, out Type valueType)
    {
        toy = null;
        valueType = null;

        try
        {
            string name = property.ToSyncVarName();

            foreach (SpawnableType spawnable in Enum.GetValues(typeof(SpawnableType)))
            {
                Type candidate = spawnable.ToType();
                PropertyInfo info = candidate.GetProperty(name);

                if (info == null)
                    continue;

                toy = info.DeclaringType == typeof(AdminToyBase) ? null : candidate;
                valueType = property == PropertyUpdate.Rotation ? typeof(Quaternion) : info.PropertyType;
                return true;
            }
        }
        catch (ArgumentOutOfRangeException)
        {
        }

        return false;
    }

    private static bool TryParse(Type valueType, string text, out object value)
    {
        value = null;

        if (valueType == null)
            return TryAutoParse(text, out value);

        if (valueType.IsEnum)
            return Enum.TryParse(valueType, text, true, out value);

        if (valueType == typeof(bool))
        {
            if (!bool.TryParse(text, out bool flag))
                return false;

            value = flag;
            return true;
        }

        if (valueType == typeof(float))
        {
            if (!TryFloat(text, out float number))
                return false;

            value = number;
            return true;
        }

        if (valueType == typeof(Color))
        {
            if (!TryColor(text, out Color color))
                return false;

            value = color;
            return true;
        }

        if (!TryVector(text, out Vector3 vector))
            return false;

        value = valueType == typeof(Quaternion) ? Quaternion.Euler(vector) : vector;
        return true;
    }

    private static bool TryAutoParse(string text, out object value)
    {
        if (bool.TryParse(text, out bool flag))
        {
            value = flag;
            return true;
        }

        if (TryFloat(text, out float number))
        {
            value = number;
            return true;
        }

        if (TryVector(text, out Vector3 vector))
        {
            value = vector;
            return true;
        }

        if (TryColor(text, out Color color))
        {
            value = color;
            return true;
        }

        value = text;
        return true;
    }

    private static bool TryFloat(string text, out float value) =>
        float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static bool TryVector(string text, out Vector3 vector)
    {
        vector = default;

        string[] parts = text.Split(',');
        if (parts.Length != 3
            || !TryFloat(parts[0], out float x)
            || !TryFloat(parts[1], out float y)
            || !TryFloat(parts[2], out float z))
            return false;

        vector = new Vector3(x, y, z);
        return true;
    }

    private static bool TryColor(string text, out Color color)
    {
        if (ColorUtility.TryParseHtmlString(text, out color))
            return true;

        string[] parts = text.Split(',');
        if (parts.Length is < 3 or > 4)
            return false;

        float[] values = [0f, 0f, 0f, 1f];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!TryFloat(parts[i], out values[i]))
                return false;
        }

        color = new Color(values[0], values[1], values[2], values[3]);
        return true;
    }
}