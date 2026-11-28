using UnityEngine;

namespace AegisEditor.API.Features.Particles;

/// <summary>
/// Converts an update interval into AdminToy MovementSmoothing values.
/// </summary>
public static class MovementSmoothingUtil
{
    private const float GameMultiplier = 0.3f;
    private const float ReachFactor = 3f;

    /// <summary>
    /// MovementSmoothing byte that lets the client settle on a target within one update interval.
    /// </summary>
    public static byte FromInterval(float interval)
    {
        float smoothing = ReachFactor / (GameMultiplier * Mathf.Max(interval, 0.001f));
        return (byte)Mathf.Clamp(Mathf.RoundToInt(smoothing), 1, 255);
    }

    /// <summary>
    /// Seconds of motion to add to a target so the smoothed client position has no visible lag.
    /// </summary>
    public static float Lead(byte smoothing) => 1f / (GameMultiplier * Mathf.Max((int)smoothing, 1));
}