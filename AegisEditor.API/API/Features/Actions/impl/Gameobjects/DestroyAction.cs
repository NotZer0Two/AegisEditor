using UnityEngine;

namespace AegisEditor.API.Features.Actions.impl.Gameobjects;

/// <summary>
/// Destroys an object.
/// </summary>
public class DestroyAction : AegisAction
{
    /// <inheritdoc />
    protected override void OnExecute()
    {
        Destroy(gameObject);
    }
}