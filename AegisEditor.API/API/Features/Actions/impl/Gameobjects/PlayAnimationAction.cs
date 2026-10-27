using UnityEngine;

namespace AegisEditor.API.Features.Actions.impl.Gameobjects;

/// <summary>
/// Plays an animation state on an <see cref="Animator"/>.
/// </summary>
public class PlayAnimationAction : AegisAction
{
    /// <summary>
    /// Animator to control. Uses the one on this object if not set.
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Name of the animation state to play.
    /// </summary>
    [Tooltip("Name of the animation state to play.")]
    public string stateName;

    /// <summary>
    /// Cross-fade duration in seconds. Zero plays instantly.
    /// </summary>
    [Tooltip("Cross-fade duration in seconds. Zero plays instantly.")]
    public float transitionDuration;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            return;

        if (transitionDuration > 0f)
            animator.CrossFade(stateName, transitionDuration);
        else
            animator.Play(stateName);
    }
}