using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.Actions.Gameobjects;

/// <summary>
/// What an <see cref="AegisParticleAction"/> does to its emitter.
/// </summary>
public enum ParticleActionMode : byte
{
    Play,
    Stop,
    Toggle
}

/// <summary>
/// Plays or stops an <see cref="AegisParticleEmitter"/>.
/// </summary>
public class AegisParticleAction : AegisAction
{
    public AegisParticleEmitter emitter;
    public ParticleActionMode mode = ParticleActionMode.Play;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (emitter == null)
            emitter = GetComponent<AegisParticleEmitter>();

        if (emitter == null)
            return;

        switch (mode)
        {
            case ParticleActionMode.Play:
                emitter.Play();
                break;
            case ParticleActionMode.Stop:
                emitter.Stop();
                break;
            default:
                if (emitter.IsEmitting)
                    emitter.Stop();
                else
                    emitter.Play();
                break;
        }
    }
}