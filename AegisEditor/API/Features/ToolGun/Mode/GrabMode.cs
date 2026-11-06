using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using UnityEngine;

namespace AegisEditor.API.Features.ToolGun.Mode;

public sealed class GrabMode : ToolGunMode
{
    public override string Name => "Grab";
    public override int Order => 1;
    public override string Primary => "Pick up and drop the selection (selects it first if empty)";

    public override void OnPrimary(ToolGunContext context)
    {
        ToolGunSession session = context.Session;

        if (!session.HasSelection)
        {
            context.SelectAimed(false);
            return;
        }

        if (!session.Grabbing)
        {
            session.Grabbing = true;
            session.GrabDistance = Vector3.Distance(context.Player.Camera.position, session.Handle.Center);
            return;
        }

        session.Grabbing = false;
        Vector3 point = GrabPoint(context);
        AegisSpawnable target = session.Target;

        if (target == null)
            MapEditor.MoveRoot(session.Placed, point);
        else
            MapEditor.Edit(target, "Position", MemberAccessor.Format(point), out _);
    }

    public override void OnTick(ToolGunContext context)
    {
        if (context.Session.Visible && context.Session.Grabbing && context.Session.Handle != null)
            context.Session.Handle.MoveTo(GrabPoint(context));
    }

    private static Vector3 GrabPoint(ToolGunContext context) =>
        context.Player.Camera.position + context.Player.Camera.forward * context.Session.GrabDistance;
}