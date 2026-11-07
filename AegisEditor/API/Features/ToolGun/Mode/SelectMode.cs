namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class SelectMode : ToolGunMode
{
    public override string Name => "Select";
    public override int Order => 0;
    public override string Primary => "Select the object you aim at";
    public override string Secondary => "Select its whole prefab";

    public override void OnPrimary(ToolGunContext context) => context.SelectAimed(false);

    public override void OnSecondary(ToolGunContext context) => context.SelectAimed(true);
}