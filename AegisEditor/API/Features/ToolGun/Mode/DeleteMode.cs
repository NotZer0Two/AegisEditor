using AegisEditor.API.Features.Spawnable;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class DeleteMode : ToolGunMode
{
    public override string Name => "Delete";
    public override int Order => 4;
    public override string Primary => "Delete the object you aim at and its children";
    public override string Secondary => "Delete its whole prefab (only prefabs spawned here)";

    public override void OnPrimary(ToolGunContext context)
    {
        AegisSpawnable target = context.Pick(out _);
        if (target != null)
            MapEditor.Delete(target, out _);
    }

    public override void OnSecondary(ToolGunContext context)
    {
        AegisSpawnable target = context.Pick(out _);
        if (target != null && target.SpawnedBy != null)
            MapEditor.Remove(MapEditor.GetOrTrack(target.SpawnedBy), out _);
    }
}