using AegisEditor.API.Features.Spawnable;

namespace AegisEditor.API.Features.Actions.impl.Speaker;

/// <summary>
/// Stops the audio playing on a speaker.
/// </summary>
public class StopSpeakerAction : AegisAction
{
    /// <summary>
    /// The speaker to stop.
    /// </summary>
    public AegisSpeaker Speaker;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (Speaker == null)
            return;

        Speaker.Stop();
    }
}