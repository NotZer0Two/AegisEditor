using System;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Public;
using CommandSystem;
using LabApi.Features.Wrappers;
using MEC;
using UnityEngine;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Generic test command to test really anything (its just so i can simply test lol).
/// </summary>
public class TestCommand : ICommand
{
    /// <inheritdoc />
    public string Command { get; } = "test";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "rt" };

    /// <inheritdoc />
    public string Description { get; } = "Spawns a code built prefab in front of you (DEBUG ONLY)";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);

        if (player == null)
        {
            response = "Only players can use this command.";
            return false;
        }

        PrefabInfo prefab = PrefabBuilder.Create("RuntimeTest")
            .AddPrimitive(SpawnablePrimitive.Cube, Vector3.zero, new Vector3(4f, 0.2f, 4f), name: "Floor")
            .AddPrimitive(SpawnablePrimitive.Sphere, Vector3.up, Vector3.one * 0.5f, collidable: false, name: "Ball")
            .Spawn(player.Position + player.Camera.forward * 3f);

        if (prefab == null)
        {
            response = "Failed to spawn the prefab.";
            return false;
        }

        prefab.Show(player);

        Timing.CallDelayed(5f, () =>
        {
            if (!prefab.IsSpawned)
                return;

            AegisPrimitive ball = prefab.Get<AegisPrimitive>("Ball");

            if (ball != null)
                ball.Color = Color.red;
        });

        response = $"Spawned runtime prefab {prefab.Id}, the ball turns red in 5 seconds.";
        return true;
    }
}