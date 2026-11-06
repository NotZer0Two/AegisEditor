using System;
using System.Collections.Generic;
using System.Linq;
using AegisEditor.API.Features;
using AegisEditor.API.Features.ServerSided.Editor;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Features.ToolGun;
using AegisEditor.API.Features.ToolGun.Editor;
using CommandSystem;
using LabApi.Features.Wrappers;
using MapGeneration;
using UnityEngine;

namespace AegisEditor.Commands;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class AegisToolCommand : ICommand
{
    private const string Usage =
        "aegistool give | remove\n"
        + "new [name]                 create an empty schematic\n"
        + "select <bundle> <prefab>   select a prefab that is already spawned\n"
        + "add <Type>                 add an object (creates a schematic if nothing is selected)\n"
        + "edit member=value ...      edit the selection, values: n  ~  +n  *n  /n\n"
        + "list                       show the members of the selection\n"
        + "apply                      rebuild prefabs with pending changes\n"
        + "save [name]                save the selected prefab\n"
        + "load <name>                load a file (spawns it, or edits the selected / spawned prefab)\n"
        + "files                      list saved files\n"
        + "boxes [on|off]             selectable boxes";

    private static readonly Dictionary<string, Func<Player, string[], string>> Handlers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["give"] = (player, _) => ToolGunItem.TryAdd(player) ? "Tool gun given." : "Could not add the tool gun.",
        ["remove"] = (player, _) => ToolGunItem.Remove(player) ? "Tool gun removed." : "You have no tool gun.",
        ["new"] = New,
        ["select"] = Select,
        ["add"] = Add,
        ["edit"] = Edit,
        ["list"] = List,
        ["apply"] = (_, _) => $"Rebuilt {Apply()} prefab(s).",
        ["save"] = Save,
        ["load"] = Load,
        ["files"] = (_, _) => string.Join("\n", MapStorage.List().DefaultIfEmpty("No files yet.")),
        ["boxes"] = Boxes,
    };

    /// <inheritdoc />
    public string Command => "aegistool";

    /// <inheritdoc />
    public string[] Aliases { get; } = ["at"];

    /// <inheritdoc />
    public string Description => "Tool gun, schematics, edits, save and load.";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);
        if (player == null)
        {
            response = "Run this as a player.";
            return false;
        }

        if (arguments.Count == 0 || !Handlers.TryGetValue(arguments.At(0), out Func<Player, string[], string> handler))
        {
            response = Usage;
            return false;
        }

        try
        {
            response = handler(player, arguments.Skip(1).ToArray());
            return true;
        }
        catch (InvalidOperationException e)
        {
            response = e.Message;
            return false;
        }
    }

    private static ToolGunSession Selection(Player player)
    {
        ToolGunSession session = ToolGunManager.GetSession(player);

        return session.HasSelection
            ? session
            : throw new InvalidOperationException("Nothing selected. Use the gun (Select mode, ADS + shoot) or: aegistool select <bundle> <prefab>");
    }

    private static Vector3 Aim(Player player) => new ToolGunContext(player, ToolGunManager.GetSession(player)).AimPoint();

    private static string New(Player player, string[] args)
    {
        PlacedPrefab placed = MapEditor.NewSchematic(args.Length > 0 ? args[0] : MapEditor.NextName(), Aim(player), RoomOf(player))
                              ?? throw new InvalidOperationException("Could not create the schematic.");

        ToolGunManager.GetSession(player).Select(placed, null);
        string room = placed.Data.Room == "Unknown" ? "world position" : $"room {placed.Data.Room}";
        return $"Created {placed.Name} ({room}). Add objects with: aegistool add <Type>";
    }

    private static string Select(Player player, string[] args)
    {
        if (args.Length < 2)
            throw new InvalidOperationException("Usage: aegistool select <bundle> <prefab>");

        PlacedPrefab placed = MapEditor.FindSpawned(args[0], args[1])
                              ?? throw new InvalidOperationException($"No spawned prefab {args[0]}/{args[1]}.");

        ToolGunManager.GetSession(player).Select(placed, null);
        return $"Selected {placed.Name}.";
    }

    private static string Add(Player player, string[] args)
    {
        string types = string.Join(", ", ClientSideRegistry.CreatableTypes.Select(t => t.Name));

        if (args.Length == 0)
            throw new InvalidOperationException($"Usage: aegistool add <Type>\nTypes: {types}");

        Type type = ClientSideRegistry.CreatableTypes.FirstOrDefault(t => string.Equals(t.Name, args[0], StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidOperationException($"Unknown type. Types: {types}");

        ToolGunSession session = ToolGunManager.GetSession(player);
        string prefix = string.Empty;

        if (!session.HasSelection)
        {
            PlacedPrefab created = MapEditor.NewSchematic(MapEditor.NextName(), Aim(player), RoomOf(player))
                                   ?? throw new InvalidOperationException("Could not create the schematic.");

            session.Select(created, null);
            prefix = $"Created {created.Name}. ";
        }

        AegisSpawnable parent = session.Target;
        Transform transform = parent != null ? parent.transform : session.Placed.Prefab.Object.transform;
        MapEditor.AddObject(session.Placed, transform, type, Aim(player));
        return $"{prefix}Added {type.Name} to {session.Placed.Name}.";
    }

    private static string Edit(Player player, string[] args)
    {
        ToolGunSession session = Selection(player);
        if (args.Length == 0)
            throw new InvalidOperationException("Usage: aegistool edit member=value ...  (aegistool list shows the members)");

        List<string> lines = new();

        foreach (string argument in args)
        {
            int split = argument.IndexOf('=');
            if (split <= 0 || split == argument.Length - 1)
                throw new InvalidOperationException($"Invalid argument \"{argument}\", use member=value.");

            string member = argument.Substring(0, split);
            string text = argument.Substring(split + 1);
            AegisSpawnable target = session.Target;

            bool ok = target == null
                ? MapEditor.EditRoot(session.Placed, member, text, out string result)
                : MapEditor.Edit(target, member, text, out result);

            lines.Add(ok ? result : $"{member}: {result}");
        }

        session.RefreshHandle();
        return string.Join("\n", lines);
    }

    private static string List(Player player, string[] args)
    {
        ToolGunSession session = Selection(player);
        AegisSpawnable target = session.Target;

        if (target == null)
        {
            SchematicData data = session.Placed.Data;
            return $"{session.Placed.Name} (room: {data.Room})\nPosition = {data.Position}\nRotation = {data.Rotation}\nScale = {data.Scale}";
            
        }

        return string.Join("\n", MemberAccessor.For(target.GetType()).Values
            .OrderBy(m => m.Name)
            .Select(m => $"{m.Name} ({m.ValueType.Name}) = {MemberAccessor.Format(m.Get(target))}"));
    }

    private static int Apply()
    {
        PlacedPrefab[] pending = MapEditor.Placed.Where(p => p.NeedsRebuild).ToArray();

        foreach (PlacedPrefab placed in pending)
            MapEditor.Rebuild(placed);

        return pending.Length;
    }

    private static string Save(Player player, string[] args) =>
        MapStorage.Save(Selection(player).Placed, args.Length > 0 ? args[0] : null);

    private static string Load(Player player, string[] args)
    {
        if (args.Length == 0)
            throw new InvalidOperationException("Usage: aegistool load <name>  (aegistool files lists them)");

        ToolGunSession session = ToolGunManager.GetSession(player);
        string message = MapStorage.Load(args[0], session.HasSelection ? session.Placed : null, RoomOf(player), out PlacedPrefab placed);

        if (placed != null)
            session.Select(placed, null);

        return message;
    }

    private static string Boxes(Player player, string[] args)
    {
        SelectableOverlay overlay = ToolGunManager.GetSession(player).Overlay;
        overlay.Enabled = args.Length == 0 ? !overlay.Enabled : string.Equals(args[0], "on", StringComparison.OrdinalIgnoreCase);
        return $"Boxes {(overlay.Enabled ? "on" : "off")}.";
    }
    
    private static RoomIdentifier RoomOf(Player player) => player.Room?.Base;
}