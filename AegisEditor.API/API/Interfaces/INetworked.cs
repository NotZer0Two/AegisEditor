using AegisEditor.API.Features.Spawnable;

namespace AegisEditor.API.Interfaces;

/// <summary>
/// Used for <see cref="AegisSpawnable"/> that are networked.
/// </summary>
public interface INetworked
{
    /// <summary>
    /// Network id of <see cref="AegisSpawnable"/>
    /// </summary>
    /// <remarks>Can be 0 for static objects.</remarks>
    public uint NetID { get; set; } // maybe we can change this is 0 for static (?)
}