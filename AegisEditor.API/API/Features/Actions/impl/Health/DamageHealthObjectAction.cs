using AegisEditor.API.Features.Editor;

namespace AegisEditor.API.Features.Actions.impl.Health;

/// <summary>
/// Damages a health object.
/// </summary>
public class DamageHealthObjectAction : AegisAction
{
    /// <summary>
    /// The health object to damage.
    /// </summary>
    public AegisHealthObject HealthObject;

    /// <summary>
    /// Amount of damage.
    /// </summary>
    public float Amount = 10f;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (HealthObject == null)
            return;

        AegisServerCalls.DamageHealthObject?.Invoke(HealthObject, Amount, Activator);
    }
}