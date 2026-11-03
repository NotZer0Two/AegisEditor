using AegisEditor.API.Features.Spawnable;

namespace AegisEditor.API.Features.Actions.impl.Speaker;

/// <summary>
/// Plays a named clip on a speaker.
/// </summary>
public class PlaySpeakerAction : AegisAction
{
    /// <summary>
    /// The speaker to play on.
    /// </summary>
    public AegisSpeaker Speaker;

    /// <summary>
    /// The name of the clip to play.
    /// </summary>
    public string ClipName;

    /// <inheritdoc />
    protected override void OnExecute()
    {
        if (Speaker == null)
            return;

        Speaker.Play(ClipName);
    }
}