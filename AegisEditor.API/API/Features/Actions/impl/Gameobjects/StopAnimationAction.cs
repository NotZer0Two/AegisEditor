using UnityEngine;

namespace AegisEditor.API.Features.Actions.impl.Gameobjects;

/// <summary>
/// Stops an <see cref="Animator"/>.
/// </summary>
public class StopAnimationAction : AegisAction
{
    /// <summary>
    /// Animator to stop. Uses the one on this object if not set.
    /// </summary>
    public Animator animator;

    /// <summary>
    /// Resets the object to its default pose before stopping.
    /// </summary>
    public bool resetToDefault;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            return;

        if (resetToDefault)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        animator.enabled = false;
    }
}