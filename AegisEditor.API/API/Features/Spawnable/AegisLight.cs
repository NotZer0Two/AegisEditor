using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Light spawnable.
/// </summary>
public class AegisLight : AegisSpawnable, INetworked, IPreCheckUseless
{
    private SpawnableLightType _lastLightType;
    private float _lastRange;
    private Color _lastColor;
    private float _lastIntensity;
    private LightShadowType _lastShadowType;
    private float _lastShadowStrength;
    private LightShapeType _lastShape;
    private float _lastSpotAngle;
    private float _lastInnerSpotAngle;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Light;

    /// <summary>
    /// The type of light.
    /// </summary>
    public SpawnableLightType LightType;

    /// <summary>
    /// Range of the light.
    /// </summary>
    [Min(0f)]
    public float Range = 10f;

    /// <summary>
    /// Color of the light.
    /// </summary>
    public Color Color = Color.white;

    /// <summary>
    /// Intensity of the light.
    /// </summary>
    [Min(0f)]
    public float Intensity = 1f;

    /// <summary>
    /// Shadow type of the light.
    /// </summary>
    public LightShadowType ShadowType = LightShadowType.None;

    /// <summary>
    /// Shadow strength of the light.
    /// </summary>
    [Range(0f, 1f)]
    public float ShadowStrength = 1f;

    /// <summary>
    /// Shape of the light.
    /// </summary>
    public LightShapeType Shape;

    /// <summary>
    /// Outer angle of spot lights.
    /// </summary>
    [Range(1f, 179f)]
    public float SpotAngle = 30f;

    /// <summary>
    /// Inner angle of spot lights.
    /// </summary>
    [Range(0f, 179f)]
    public float InnerSpotAngle = 20f;

    /// <inheritdoc />
    [HideInInspector]
    public uint NetID { get; set; }

    /// <summary>
    /// The Unity light.
    /// </summary>
    [HideInInspector]
    public Light LightObject;

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        LightObject = this.GetComponentOrCreate<Light>();
        LightObject.hideFlags = HideFlags.HideInInspector;
        ApplyToLight();
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;

        ApplyToLight();
    }

    /// <inheritdoc />
    public override void OnStart()
    {
        _lastLightType = LightType;
        _lastRange = Range;
        _lastColor = Color;
        _lastIntensity = Intensity;
        _lastShadowType = ShadowType;
        _lastShadowStrength = ShadowStrength;
        _lastShape = Shape;
        _lastSpotAngle = SpotAngle;
        _lastInnerSpotAngle = InnerSpotAngle;

        base.OnStart();
    }

    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        changed |= Track(ref _lastLightType, LightType, PropertyUpdate.LightType, value);
        changed |= Track(ref _lastRange, Range, PropertyUpdate.Range, value);
        changed |= Track(ref _lastColor, Color, PropertyUpdate.LightColor, value);
        changed |= Track(ref _lastIntensity, Intensity, PropertyUpdate.Intensity, value);
        changed |= Track(ref _lastShadowType, ShadowType, PropertyUpdate.ShadowType, value);
        changed |= Track(ref _lastShadowStrength, ShadowStrength, PropertyUpdate.ShadowStrength, value);
        changed |= Track(ref _lastShape, Shape, PropertyUpdate.LightShape, value);
        changed |= Track(ref _lastSpotAngle, SpotAngle, PropertyUpdate.SpotAngle, value);
        changed |= Track(ref _lastInnerSpotAngle, InnerSpotAngle, PropertyUpdate.InnerSpotAngle, value);

        return changed;
    }

    /// <inheritdoc />
    public bool ShouldSpawn() => Range == 0 || Intensity <= 0;

    private static bool Track<T>(ref T last, T current, PropertyUpdate property, List<(PropertyUpdate propertyName, object value)> list)
    {
        if (EqualityComparer<T>.Default.Equals(last, current))
            return false;

        last = current;
        list.Add((property, current));
        return true;
    }

    private void ApplyToLight()
    {
        if (LightObject == null)
            return;

        LightObject.type = LightType.ToLightType();
        LightObject.range = Range;
        LightObject.color = Color;
        LightObject.intensity = Intensity;
        LightObject.shadows = ShadowType.ToShadowType();
        LightObject.shadowStrength = ShadowStrength;
        LightObject.shape = Shape.ToShapeType();
        LightObject.spotAngle = SpotAngle;
        LightObject.innerSpotAngle = Mathf.Min(InnerSpotAngle, SpotAngle);
    }
}