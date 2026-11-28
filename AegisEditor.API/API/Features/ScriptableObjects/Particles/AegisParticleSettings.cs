using AegisEditor.API.Enum;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.Particles;

/// <summary>
/// Shape particles are emitted from.
/// </summary>
public enum ParticleShape : byte
{
    Point,
    Sphere,
    Cone,
    Box
}

/// <summary>
/// Reusable particle configuration.
/// </summary>
[CreateAssetMenu(menuName = "AegisEditor/Particle Settings", fileName = "ParticleSettings")]
public class AegisParticleSettings : ScriptableObject
{
    private const int Samples = 16;

    public SpawnablePrimitive primitiveType = SpawnablePrimitive.Sphere;
    [Min(1)] public int maxParticles = 32;
    [Min(0)] public float emissionRate = 10f;
    [Min(0)] public int burst;
    public bool loop = true;
    [Min(0.01f)] public float duration = 5f;
    [Range(1f, 30f)] public float tickRate = 10f;
    public Vector2 lifetime = new(1f, 2f);
    public Vector2 startSpeed = new(1f, 2f);
    public Vector2 startSize = new(0.2f, 0.3f);
    public Vector3 gravity = new(0f, -9.81f, 0f);
    [Min(0)] public float drag;
    public ParticleShape shape = ParticleShape.Point;
    [Min(0)] public float shapeRadius = 0.5f;
    [Range(0f, 90f)] public float coneAngle = 25f;
    public Vector3 boxSize = Vector3.one;
    public AnimationCurve sizeOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [GradientUsage(true)] public Gradient colorOverLifetime = new();

    [System.NonSerialized] private float[] _sizeTable;
    [System.NonSerialized] private Color[] _colorTable;
    
    /// <summary>
    /// Seconds between server ticks.
    /// </summary>
    public float TickInterval => 1f / tickRate;

    /// <summary>
    /// MovementSmoothing used by every particle of this configuration.
    /// </summary>
    public byte Smoothing => MovementSmoothingUtil.FromInterval(TickInterval);

    /// <summary>
    /// Size multiplier at normalized lifetime <paramref name="t"/>.
    /// </summary>
    public float SizeAt(float t)
    {
        Bake();
        return _sizeTable[Index(t)];
    }

    /// <summary>
    /// Color at normalized lifetime <paramref name="t"/>.
    /// </summary>
    public Color ColorAt(float t)
    {
        Bake();
        return _colorTable[Index(t)];
    }

    /// <summary>
    /// Creates the primitive every particle is cloned from.
    /// </summary>
    public AegisPrimitive CreateTemplate()
    {
        GameObject host = new("Particle");
        AegisPrimitive primitive = host.AddComponent<AegisPrimitive>();
        primitive.PrimitiveType = primitiveType;
        primitive.IsCollidable = false;
        primitive.IsVisible = true;
        primitive.Color = ColorAt(0f);
        primitive.movementSmoothing = Smoothing;
        return primitive;
    }

    /// <summary>
    /// Samples a spawn offset and direction in the emitter's local space.
    /// </summary>
    public void Sample(out Vector3 offset, out Vector3 direction)
    {
        switch (shape)
        {
            case ParticleShape.Sphere:
                direction = Random.onUnitSphere;
                offset = direction * (shapeRadius * Random.value);
                break;
            case ParticleShape.Cone:
                Vector2 circle = Random.insideUnitCircle;
                float spread = Mathf.Tan(coneAngle * Mathf.Deg2Rad);
                direction = new Vector3(circle.x * spread, 1f, circle.y * spread).normalized;
                offset = new Vector3(circle.x, 0f, circle.y) * shapeRadius;
                break;
            case ParticleShape.Box:
                direction = Vector3.up;
                offset = new Vector3(
                    (Random.value - 0.5f) * boxSize.x,
                    (Random.value - 0.5f) * boxSize.y,
                    (Random.value - 0.5f) * boxSize.z);
                break;
            default:
                direction = Random.onUnitSphere;
                offset = Vector3.zero;
                break;
        }
    }

    private static int Index(float t) => Mathf.Clamp(Mathf.RoundToInt(t * (Samples - 1)), 0, Samples - 1);

    private void Bake()
    {
        if (_sizeTable != null)
            return;

        _sizeTable = new float[Samples];
        _colorTable = new Color[Samples];
        for (int i = 0; i < Samples; i++)
        {
            float t = i / (float)(Samples - 1);
            _sizeTable[i] = sizeOverLifetime.Evaluate(t);
            _colorTable[i] = colorOverLifetime.Evaluate(t);
        }
    }
    
    private void OnValidate()
    {
        _sizeTable = null;
        _colorTable = null;
    }
}