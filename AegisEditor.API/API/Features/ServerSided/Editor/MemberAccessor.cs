using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ServerSided.Editor;

/// <summary>
/// Reads and writes a public field or property of a spawnable from text.
/// Components accept: n sets, ~ keeps, +n or ~n adds, *n multiplies, /n divides.
/// </summary>
public sealed class MemberAccessor
{
    private static readonly Dictionary<Type, Dictionary<string, MemberAccessor>> Cache = new();
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private readonly FieldInfo _field;
    private readonly PropertyInfo _property;

    private MemberAccessor(string name, Type valueType, FieldInfo field, PropertyInfo property)
    {
        Name = name;
        ValueType = valueType;
        _field = field;
        _property = property;
    }

    public string Name { get; }

    public Type ValueType { get; }

    public bool IsTransform =>
        Name is "Position" or "Rotation" or "Scale" or "LocalPosition" or "LocalRotation" or "LocalScale";

    /// <summary>
    /// Gets the editable members of a spawnable type, case insensitive.
    /// </summary>
    public static IReadOnlyDictionary<string, MemberAccessor> For(Type type)
    {
        if (Cache.TryGetValue(type, out Dictionary<string, MemberAccessor> cached))
            return cached;

        Dictionary<string, MemberAccessor> members = new(StringComparer.OrdinalIgnoreCase);

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!field.IsInitOnly && !field.IsLiteral && Include(field.DeclaringType, field.FieldType))
                members[field.Name] = new MemberAccessor(field.Name, field.FieldType, field, null);
        }

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length == 0 && property.CanRead && property.GetSetMethod() != null
                && Include(property.DeclaringType, property.PropertyType))
                members[property.Name] = new MemberAccessor(property.Name, property.PropertyType, null, property);
        }

        return Cache[type] = members;
    }

    public object Get(object target) => _field != null ? _field.GetValue(target) : _property.GetValue(target);

    public void Set(object target, object value)
    {
        if (_field != null)
            _field.SetValue(target, value);
        else
            _property.SetValue(target, value);
    }

    /// <summary>
    /// Parses the text, sets the member and returns the absolute value that was stored.
    /// </summary>
    public bool TryApply(object target, string text, out string stored, out string error)
    {
        stored = null;
        error = null;

        if (!TryParse(ValueType, Get(target), text, out object value))
        {
            error = $"Invalid value \"{text}\" for {Name} ({ValueType.Name}).";
            return false;
        }

        Set(target, value);
        stored = Format(Get(target));
        return true;
    }

    /// <summary>
    /// Parses a value of the type, using the current value for relative components.
    /// </summary>
    public static bool TryParse(Type type, object current, string text, out object value)
    {
        value = null;

        if (type == typeof(string))
        {
            value = text;
            return true;
        }

        text = text.Trim();

        if (type == typeof(bool))
        {
            if (text == "~")
                value = current;
            else if (text == "!")
                value = !(bool)current;
            else if (bool.TryParse(text, out bool flag))
                value = flag;
            else
                return false;

            return true;
        }

        if (type.IsEnum)
            return System.Enum.TryParse(type, text, true, out value);

        if (type == typeof(Color) && !text.Contains(',') && ColorUtility.TryParseHtmlString(text, out Color named))
        {
            value = named;
            return true;
        }

        float[] now = Components(type, current);
        if (now == null)
            return false;

        string[] parts = text.Split(',');
        if (type == typeof(Color) && parts.Length == 3)
            parts = [parts[0], parts[1], parts[2], "~"];

        if (parts.Length != now.Length)
            return false;

        float[] next = new float[now.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            if (!TryComponent(parts[i], now[i], out next[i]))
                return false;
        }

        try
        {
            value = Build(type, next);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Formats a value so <see cref="TryParse"/> reads it back unchanged.
    /// </summary>
    public static string Format(object value) => value switch
    {
        float f => f.ToString("R", Culture),
        double d => d.ToString("R", Culture),
        Vector3 v => $"{Format(v.x)},{Format(v.y)},{Format(v.z)}",
        Vector2 v => $"{Format(v.x)},{Format(v.y)}",
        Quaternion q => Format(q.eulerAngles),
        Color c => $"{Format(c.r)},{Format(c.g)},{Format(c.b)},{Format(c.a)}",
        System.Enum e => e.ToString(),
        IFormattable formattable => formattable.ToString(null, Culture),
        _ => value?.ToString() ?? string.Empty,
    };

    private static bool Include(Type declaring, Type valueType) =>
        typeof(AegisSpawnable).IsAssignableFrom(declaring) && IsSupported(valueType);

    private static bool IsSupported(Type type) =>
        type == typeof(string) || type == typeof(bool) || type == typeof(Vector2) || type == typeof(Vector3)
        || type == typeof(Quaternion) || type == typeof(Color) || type.IsEnum
        || (type.IsPrimitive && type != typeof(char) && type != typeof(IntPtr) && type != typeof(UIntPtr));

    private static float[] Components(Type type, object current)
    {
        if (type == typeof(Vector3))
        {
            Vector3 v = (Vector3)current;
            return [v.x, v.y, v.z];
        }

        if (type == typeof(Vector2))
        {
            Vector2 v = (Vector2)current;
            return [v.x, v.y];
        }

        if (type == typeof(Quaternion))
        {
            Vector3 v = ((Quaternion)current).eulerAngles;
            return [v.x, v.y, v.z];
        }

        if (type == typeof(Color))
        {
            Color c = (Color)current;
            return [c.r, c.g, c.b, c.a];
        }

        return type.IsPrimitive ? [Convert.ToSingle(current, Culture)] : null;
    }

    private static object Build(Type type, float[] v)
    {
        if (type == typeof(Vector3))
            return new Vector3(v[0], v[1], v[2]);

        if (type == typeof(Vector2))
            return new Vector2(v[0], v[1]);

        if (type == typeof(Quaternion))
            return Quaternion.Euler(v[0], v[1], v[2]);

        if (type == typeof(Color))
            return new Color(v[0], v[1], v[2], v[3]);

        double number = v[0];
        if (type != typeof(float) && type != typeof(double))
            number = Math.Round(number);

        return Convert.ChangeType(number, type, Culture);
    }

    private static bool TryComponent(string token, float current, out float result)
    {
        token = token.Trim();
        result = current;

        if (token.Length == 0)
            return false;

        if (token == "~")
            return true;

        if (token[0] is not ('~' or '+' or '*' or '/'))
            return TryNumber(token, out result);

        if (!TryNumber(token.Substring(1), out float number))
            return false;

        switch (token[0])
        {
            case '*':
                result = current * number;
                return true;
            case '/':
                if (number == 0f)
                    return false;

                result = current / number;
                return true;
            default:
                result = current + number;
                return true;
        }
    }

    private static bool TryNumber(string text, out float number) =>
        float.TryParse(text, NumberStyles.Float, Culture, out number);
}