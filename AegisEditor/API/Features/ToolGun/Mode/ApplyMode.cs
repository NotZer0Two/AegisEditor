using System.Linq;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun.Editor;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class ApplyMode : ToolGunMode
{
    public override string Name => "Apply";
    public override int Order => 5;
    public override string Primary => "Rebuild the selected (or aimed) prefab";
    public override string Secondary => "Rebuild every prefab with pending changes";

    public override void OnPrimary(ToolGunContext context)
    {
        PlacedPrefab placed = context.Session.HasSelection ? context.Session.Placed : null;

        if (placed == null)
        {
            AegisSpawnable target = context.Pick(out _);
            if (target == null || target.SpawnedBy == null)
                return;

            placed = MapEditor.GetOrTrack(target.SpawnedBy);
        }

        MapEditor.Rebuild(placed);
    }

    public override void OnSecondary(ToolGunContext context)
    {
        foreach (PlacedPrefab placed in MapEditor.Placed.Where(p => p.NeedsRebuild).ToArray())
            MapEditor.Rebuild(placed);
    }
}