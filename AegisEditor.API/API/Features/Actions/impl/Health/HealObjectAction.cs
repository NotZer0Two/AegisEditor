using AegisEditor.API.Features.Editor;

namespace AegisEditor.API.Features.Actions.impl.Health;

/// <summary>
/// Heals a health object.
/// </summary>
public class HealAction : AegisAction
{
    /// <summary>
    /// The health object to heal.
    /// </summary>
    public AegisHealthObject HealthObject;

    /// <summary>
    /// Amount of health.
    /// </summary>
    public float Amount = 10f;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (HealthObject == null)
            return;

        AegisServerCalls.HealHealthObject?.Invoke(HealthObject, Amount);
    }
}