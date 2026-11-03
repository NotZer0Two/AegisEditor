using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminToys;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using CommandSystem;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;
using PrimitiveObjectToy = AdminToys.PrimitiveObjectToy;

namespace AegisEditor.Commands.DebugSubCommands;

/// <summary>
/// Spawns the same hierarchy with AegisEditor and with real game toys, and checks that they match.
/// </summary>
public class ParentTestCommand : ICommand
{
    private const float Tolerance = 0.01f;
    private const float AngleTolerance = 0.1f;

    private readonly record struct Part(string Name, string Parent, Vector3 Position, Quaternion Rotation, Vector3 Scale, bool Visible);

    private struct State
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public byte Smoothing;
        public bool IsStatic;
        public uint ParentId;
    }

    private sealed class Node
    {
        public AegisSpawnable Source;
        public ClientSideObject Client;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public byte Smoothing;
        public bool IsStatic;
    }

    private static readonly Part[] Parts =
    [
        new("ParentTest_A", null, new Vector3(2f, 0f, 0f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 2f, false),
        new("ParentTest_B", "ParentTest_A", new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, 0f, 30f), Vector3.one * 1.5f, false),
        new("ParentTest_Leaf", "ParentTest_B", new Vector3(1f, 0f, 0f), Quaternion.Euler(0f, 15f, 0f), new Vector3(0.5f, 1f, 0.5f), true),
        new("ParentTest_SideLeaf", "ParentTest_A", new Vector3(-1f, 0f, 0f), Quaternion.identity, Vector3.one * 0.5f, true),
        new("ParentTest_RootLeaf", null, new Vector3(0f, 0f, 3f), Quaternion.Euler(0f, 20f, 0f), Vector3.one * 0.5f, true),
    ];

    private static readonly Dictionary<string, string> ExpectedParents = new()
    {
        ["ParentTest_A"] = null,
        ["ParentTest_B"] = "ParentTest_A",
        ["ParentTest_Leaf"] = "ParentTest_B",
        ["ParentTest_SideLeaf"] = "ParentTest_A",
        ["ParentTest_RootLeaf"] = null,
        ["ParentTest_Culling"] = null,
        ["ParentTest_CullingChild"] = "ParentTest_Culling",
    };

    private static readonly List<Node> Nodes = new();
    private static readonly Dictionary<string, PrimitiveObjectToy> GameToys = new();
    private static List<Node> _spawnOrder = new();
    private static Dictionary<string, State> _gameSnapshot;
    private static HashSet<string> _aegisSent = new();
    private static GameObject _root;
    private static GameObject _gameRoot;
    private static Vector3 _gameOffset;
    private static Transform _groupA;
    private static Transform _leaf;
    private static Player _player;
    private static int _moveFrame;

    /// <inheritdoc />
    public string Command { get; } = "parenttest";

    /// <inheritdoc />
    public string[] Aliases { get; } = new string[] { "pt" };

    /// <inheritdoc />
    public string Description { get; } = "Spawns the same hierarchy with AegisEditor and with real game toys and checks they match (DEBUG ONLY)";

    /// <inheritdoc />
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);
        if (player == null)
        {
            response = "Run this as a player.";
            return false;
        }

        string action = arguments.Count > 0 ? arguments.At(0).ToLowerInvariant() : "spawn";

        switch (action)
        {
            case "spawn":
                return SpawnAndVerify(player, out response);
            case "move":
                return Move(false, out response);
            case "child":
                return Move(true, out response);
            case "compare":
                return Compare(out response);
            case "clear":
                response = $"Cleared {Clear()} test objects.";
                return true;
            default:
                response = "Usage: aegiseditor parenttest [spawn|move|child|compare|clear]\n"
                           + " spawn   - spawns the hierarchy with AegisEditor and with real game toys and compares them\n"
                           + " move    - moves the top group on both sides, then run compare one frame later\n"
                           + " child   - moves the leaf locally on both sides, then run compare one frame later\n"
                           + " compare - compares what AegisEditor sends with what the game toys sync\n"
                           + " clear   - removes everything";
                return false;
        }
    }

    private static bool SpawnAndVerify(Player player, out string response)
    {
        Clear();
        _player = player;

        try
        {
            Build(player);

            List<(PropertyUpdate propertyName, object value)> scratch = new();

            foreach (AegisSpawnable source in _root.GetComponentsInChildren<AegisSpawnable>())
            {
                source.UpdateClient(scratch);
                scratch.Clear();

                NetworkedObject obj = ClientSideRegistry.CreateObject(source, player);
                if (obj is not ClientSideObject client)
                    throw new InvalidOperationException($"{source.name} has no client object.");

                Node node = new()
                {
                    Source = source,
                    Client = client,
                    Position = client.SentPosition,
                    Rotation = client.SentRotation,
                    Scale = client.SentScale,
                    Smoothing = 30,
                };

                if (source is not INotAToyButAToy)
                {
                    ArraySegment<byte> payload = client.Message.payload;
                    State sent = Decode(new ArraySegment<byte>(payload.Array, payload.Offset + 2, payload.Count - 2));
                    node.Position = sent.Position;
                    node.Rotation = sent.Rotation;
                    node.Scale = sent.Scale;
                    node.Smoothing = sent.Smoothing;
                    node.IsStatic = sent.IsStatic;
                }

                Nodes.Add(node);
            }

            _spawnOrder = Nodes.OrderBy(n => n.Client.Priority).ToList();

            foreach (Node node in _spawnOrder)
                node.Client.Spawn(player);

            BuildGame();
        }
        catch (Exception e)
        {
            Clear();
            response = $"Failed ({e.GetType().Name}: {e.Message})";
            return false;
        }

        StringBuilder sb = new();
        bool passed = VerifySpawn(sb);
        passed &= CompareWithGame(sb);

        sb.AppendLine(passed ? "RESULT: PASS" : "RESULT: FAIL");
        sb.AppendLine($"The game copy is spawned {_gameOffset.x} units to the side on X and visible to everyone, use 'parenttest clear' to remove it.");
        sb.AppendLine("Run 'parenttest move' and 'parenttest child' to check the updates.");

        response = sb.ToString();
        return passed;
    }

    private static void Build(Player player)
    {
        Vector3 origin = player.Position + (player.Rotation * Vector3.forward * 4f);

        _root = new GameObject("ParentTest_Root");
        _root.transform.SetPositionAndRotation(origin, Quaternion.Euler(0f, 30f, 0f));

        Dictionary<string, Transform> created = new();

        foreach (Part part in Parts)
        {
            Transform parent = part.Parent == null ? _root.transform : created[part.Parent];

            GameObject go = new(part.Name);
            Transform t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = part.Position;
            t.localRotation = part.Rotation;
            t.localScale = part.Scale;

            if (part.Visible)
            {
                AegisPrimitive primitive = go.AddComponent<AegisPrimitive>();
                primitive.PrimitiveType = SpawnablePrimitive.Cube;
                primitive.Color = Color.red;
                primitive.IsVisible = true;
                primitive.IsCollidable = false;
            }

            created[part.Name] = t;
        }

        _groupA = created["ParentTest_A"];
        _leaf = created["ParentTest_Leaf"];

        GameObject cullingObject = new("ParentTest_Culling");
        cullingObject.transform.SetParent(_root.transform, false);
        cullingObject.transform.localPosition = new Vector3(-3f, 0f, 0f);

        AegisCullingParent culling = cullingObject.AddComponent<AegisCullingParent>();
        culling.InitializeInEditor();
        culling.boundsCollider.size = Vector3.one * 6f;

        GameObject cullingChild = new("ParentTest_CullingChild");
        cullingChild.transform.SetParent(cullingObject.transform, false);
        cullingChild.transform.localPosition = new Vector3(0f, 1f, 0f);
        cullingChild.transform.localScale = Vector3.one * 0.5f;

        AegisPrimitive childPrimitive = cullingChild.AddComponent<AegisPrimitive>();
        childPrimitive.PrimitiveType = SpawnablePrimitive.Cube;
        childPrimitive.Color = Color.yellow;
        childPrimitive.IsVisible = true;
        childPrimitive.IsCollidable = false;

        PrefabInfo.PrepareHierarchy(_root.transform);
    }

    private static void BuildGame()
    {
        PrimitiveObjectToy prefab = null;
        foreach (GameObject candidate in NetworkClient.prefabs.Values)
        {
            if (candidate != null && candidate.TryGetComponent(out PrimitiveObjectToy found))
            {
                prefab = found;
                break;
            }
        }

        if (prefab == null)
            throw new InvalidOperationException("PrimitiveObjectToy prefab not found.");

        _gameOffset = new Vector3(8f, 0f, 0f);
        _gameRoot = new GameObject("ParentTest_GameRoot");
        _gameRoot.transform.SetPositionAndRotation(_root.transform.position + _gameOffset, _root.transform.rotation);

        foreach (Part part in Parts)
        {
            PrimitiveObjectToy toy = UnityEngine.Object.Instantiate(prefab);
            toy.name = part.Name;
            Transform t = toy.transform;

            if (part.Parent == null)
            {
                t.SetPositionAndRotation(_gameRoot.transform.TransformPoint(part.Position), _gameRoot.transform.rotation * part.Rotation);
            }
            else
            {
                t.SetParent(GameToys[part.Parent].transform, false);
                t.localPosition = part.Position;
                t.localRotation = part.Rotation;
            }

            t.localScale = part.Scale;
            toy.NetworkMovementSmoothing = 30;
            GameToys[part.Name] = toy;
        }

        foreach (Part part in Parts)
            NetworkServer.Spawn(GameToys[part.Name].gameObject);
    }

    private static State Decode(ArraySegment<byte> data)
    {
        NetworkReader reader = new(data);
        State state = default;

        state.Position = reader.ReadVector3();
        state.Rotation = reader.ReadQuaternion();
        state.Scale = reader.ReadVector3();
        state.Smoothing = NetworkReaderExtensions.ReadByte(reader);
        state.IsStatic = reader.ReadBool();
        reader.ReadInt();
        reader.ReadColor();
        NetworkReaderExtensions.ReadByte(reader);
        state.ParentId = reader.ReadUInt();

        return state;
    }

    private static State DecodeGame(PrimitiveObjectToy toy)
    {
        using NetworkWriterPooled writer = NetworkWriterPool.Get();
        toy.OnSerialize(writer, true);
        return Decode(writer.ToArraySegment());
    }

    private static bool VerifySpawn(StringBuilder sb)
    {
        bool legacy = !AegisAPI.UseGameParenting;
        bool passed = true;

        sb.AppendLine($"UseGameParenting: {AegisAPI.UseGameParenting}");

        int expectedCount = legacy ? 5 : 7;
        if (Nodes.Count != expectedCount)
        {
            sb.AppendLine($"FAIL object count is {Nodes.Count}, expected {expectedCount}");
            passed = false;
        }

        foreach (Node node in Nodes)
        {
            List<string> problems = new();
            string name = node.Source.name;
            uint expectedParent = ExpectedParentNetId(node.Source);

            if (node.Client.ParentNetId != expectedParent)
                problems.Add($"parent id sent {node.Client.ParentNetId}, expected {expectedParent}");

            if (!legacy && ExpectedParents.TryGetValue(name, out string expectedName))
            {
                string actual = node.Source.ClientParent != null ? node.Source.ClientParent.name : null;
                if (actual != expectedName)
                    problems.Add($"parent is {actual ?? "none"}, expected {expectedName ?? "none"}");
            }

            if (node.Client.ParentNetId != 0)
            {
                Node parent = FindNode(node.Client.ParentNetId);
                if (parent != null && _spawnOrder.IndexOf(parent) >= _spawnOrder.IndexOf(node))
                    problems.Add("spawns before its parent");
            }

            AppendWorldCheck(node, legacy, problems);

            passed &= problems.Count == 0;
            sb.AppendLine($"{(problems.Count == 0 ? "PASS" : "FAIL")} {name} id={node.Client.NetId} parent={node.Client.ParentNetId}"
                          + (problems.Count == 0 ? string.Empty : " | " + string.Join("; ", problems)));
        }

        if (legacy)
            sb.AppendLine("Legacy mode: transforms of parented objects are world space and are not checked.");

        return passed;
    }

    private static bool CompareWithGame(StringBuilder sb)
    {
        sb.AppendLine("Comparison with real game toys:");

        if (!AegisAPI.UseGameParenting)
        {
            sb.AppendLine("Skipped, enable UseGameParenting.");
            return true;
        }

        bool passed = true;

        foreach (Part part in Parts)
        {
            Node node = Nodes.Find(n => n.Source.name == part.Name);
            if (node == null || !GameToys.TryGetValue(part.Name, out PrimitiveObjectToy toy) || toy == null)
            {
                sb.AppendLine($"FAIL {part.Name} | missing on one side");
                passed = false;
                continue;
            }

            List<string> problems = new();
            State game;

            try
            {
                game = DecodeGame(toy);
            }
            catch (Exception e)
            {
                sb.AppendLine($"FAIL {part.Name} | could not decode the game data ({e.GetType().Name}: {e.Message})");
                passed = false;
                continue;
            }

            string aegisParent = node.Client.ParentNetId == 0 ? null : FindNode(node.Client.ParentNetId)?.Source.name;
            string gameParent = game.ParentId == 0 ? null : (NetworkServer.spawned.TryGetValue(game.ParentId, out NetworkIdentity identity) ? identity.name : $"id {game.ParentId}");

            if (aegisParent != gameParent)
                problems.Add($"parent aegis={aegisParent ?? "none"} game={gameParent ?? "none"}");

            Vector3 gamePosition = gameParent == null ? game.Position - _gameOffset : game.Position;

            if (Vector3.Distance(node.Position, gamePosition) > Tolerance)
                problems.Add($"position aegis={node.Position} game={gamePosition}");

            if (Quaternion.Angle(node.Rotation, game.Rotation) > AngleTolerance)
                problems.Add($"rotation aegis={node.Rotation.eulerAngles} game={game.Rotation.eulerAngles}");

            if (Vector3.Distance(node.Scale, game.Scale) > Tolerance)
                problems.Add($"scale aegis={node.Scale} game={game.Scale}");

            if (node.Smoothing != game.Smoothing)
                problems.Add($"smoothing aegis={node.Smoothing} game={game.Smoothing}");

            if (node.IsStatic != game.IsStatic)
                problems.Add($"static aegis={node.IsStatic} game={game.IsStatic}");

            passed &= problems.Count == 0;
            sb.AppendLine($"{(problems.Count == 0 ? "PASS" : "FAIL")} {part.Name} matches the game"
                          + (problems.Count == 0 ? string.Empty : " | " + string.Join("; ", problems)));
        }

        if (_gameSnapshot != null)
        {
            HashSet<string> changed = new();

            foreach (Part part in Parts)
            {
                if (!GameToys.TryGetValue(part.Name, out PrimitiveObjectToy toy) || toy == null || !_gameSnapshot.TryGetValue(part.Name, out State before))
                    continue;

                State now = DecodeGame(toy);
                if (Vector3.Distance(before.Position, now.Position) > 0.0001f
                    || Quaternion.Angle(before.Rotation, now.Rotation) > 0.001f
                    || Vector3.Distance(before.Scale, now.Scale) > 0.0001f)
                    changed.Add(part.Name);
            }

            _gameSnapshot = null;

            if (changed.SetEquals(_aegisSent))
            {
                sb.AppendLine($"PASS the same objects send updates in the game and in AegisEditor: {string.Join(", ", changed)}");
            }
            else
            {
                passed = false;
                sb.AppendLine($"FAIL updates differ | game: {string.Join(", ", changed)} | aegis: {string.Join(", ", _aegisSent)}");
            }
        }

        return passed;
    }

    private static bool Move(bool child, out string response)
    {
        if (Nodes.Count == 0 || _root == null || _player == null || _player.IsDestroyed)
        {
            response = "Run 'parenttest' first.";
            return false;
        }

        Dictionary<string, State> snapshot = new();
        foreach (KeyValuePair<string, PrimitiveObjectToy> pair in GameToys)
        {
            if (pair.Value != null)
                snapshot[pair.Key] = DecodeGame(pair.Value);
        }

        Transform moved = child ? _leaf : _groupA;
        Vector3 offset = new(0f, 0.5f, 0f);
        moved.localPosition += offset;

        if (child)
        {
            GameToys[_leaf.name].transform.localPosition += offset;
        }
        else
        {
            moved.localRotation *= Quaternion.Euler(0f, 40f, 0f);

            Transform game = GameToys[_groupA.name].transform;
            game.SetPositionAndRotation(_gameRoot.transform.TransformPoint(moved.localPosition), _gameRoot.transform.rotation * moved.localRotation);
        }

        HashSet<string> expected = new();
        if (child || AegisAPI.UseGameParenting)
        {
            expected.Add(moved.name);
        }
        else
        {
            foreach (Node node in Nodes)
            {
                if (node.Source.transform.IsChildOf(moved))
                    expected.Add(node.Source.name);
            }
        }

        HashSet<string> sent = new();

        foreach (Node node in Nodes)
        {
            List<(PropertyUpdate propertyName, object value)> changes = new();
            if (!node.Source.UpdateClient(changes))
                continue;

            sent.Add(node.Source.name);

            foreach ((PropertyUpdate property, object value) in changes)
            {
                switch (property)
                {
                    case PropertyUpdate.Position:
                        node.Position = (Vector3)value;
                        break;
                    case PropertyUpdate.Rotation:
                        node.Rotation = (Quaternion)value;
                        break;
                    case PropertyUpdate.Scale:
                        node.Scale = (Vector3)value;
                        break;
                }
            }

            _player.SendFakeSyncVar(node.Client.NetId, node.Source.Type.ToType(), changes);
        }

        _gameSnapshot = snapshot;
        _aegisSent = sent;
        _moveFrame = Time.frameCount;

        StringBuilder sb = new();
        bool passed = true;

        sb.AppendLine($"UseGameParenting: {AegisAPI.UseGameParenting}");
        sb.AppendLine($"Moved {moved.name} on both sides");
        sb.AppendLine($"Updates sent by AegisEditor for: {(sent.Count == 0 ? "nothing" : string.Join(", ", sent))}");

        string[] missing = expected.Except(sent).ToArray();
        string[] extra = sent.Except(expected).ToArray();

        if (missing.Length > 0 || extra.Length > 0)
        {
            passed = false;
            sb.AppendLine($"FAIL expected updates for: {string.Join(", ", expected)}");
            if (missing.Length > 0)
                sb.AppendLine($"  missing: {string.Join(", ", missing)}");
            if (extra.Length > 0)
                sb.AppendLine($"  unexpected: {string.Join(", ", extra)}");
        }
        else
        {
            sb.AppendLine("PASS only the expected objects sent updates");
        }

        bool legacy = !AegisAPI.UseGameParenting;

        foreach (Node node in Nodes)
        {
            List<string> problems = new();
            AppendWorldCheck(node, legacy, problems);

            if (problems.Count == 0)
                continue;

            passed = false;
            sb.AppendLine($"FAIL {node.Source.name} | {string.Join("; ", problems)}");
        }

        if (passed)
            sb.AppendLine("PASS client world transforms match the server after the update");

        sb.AppendLine(passed ? "RESULT: PASS" : "RESULT: FAIL");
        sb.AppendLine("The game applies its sync a frame later, run 'parenttest compare' now to compare with the game toys.");

        response = sb.ToString();
        return passed;
    }

    private static bool Compare(out string response)
    {
        if (Nodes.Count == 0 || GameToys.Count == 0)
        {
            response = "Run 'parenttest' first.";
            return false;
        }

        if (Time.frameCount == _moveFrame)
        {
            response = "The game applies its sync a frame later, run it again.";
            return false;
        }

        StringBuilder sb = new();
        bool passed = CompareWithGame(sb);

        sb.AppendLine(passed ? "RESULT: PASS" : "RESULT: FAIL");
        response = sb.ToString();
        return passed;
    }

    private static void AppendWorldCheck(Node node, bool legacy, List<string> problems)
    {
        if (legacy && node.Client.ParentNetId != 0)
            return;

        if (!TryGetWorld(node, 0, out Matrix4x4 world, out string error))
        {
            problems.Add(error);
            return;
        }

        if (!Matches(world, node.Source.transform.localToWorldMatrix))
            problems.Add("client world transform differs from the server");
    }

    private static bool TryGetWorld(Node node, int depth, out Matrix4x4 world, out string error)
    {
        world = default;
        error = null;

        Matrix4x4 local = Matrix4x4.TRS(node.Position, node.Rotation, node.Scale);
        uint parentId = node.Client.ParentNetId;

        if (parentId == 0)
        {
            world = local;
            return true;
        }

        Node parent = FindNode(parentId);
        if (parent == null)
        {
            error = $"parent id {parentId} was not spawned";
            return false;
        }

        if (depth > 32)
        {
            error = "parent loop";
            return false;
        }

        Matrix4x4 parentWorld = Matrix4x4.identity;
        if (parent.Source is not INotAToyButAToy && !TryGetWorld(parent, depth + 1, out parentWorld, out error))
            return false;

        world = parentWorld * local;
        return true;
    }

    private static uint ExpectedParentNetId(AegisSpawnable source)
    {
        if (source is INotAToyButAToy)
            return 0;

        if (!AegisAPI.UseGameParenting)
            return source.CullingParent is { NetID: not 0 } culling ? culling.NetID : 0;

        Transform parent = source.transform.parent;
        if (parent != null && parent.TryGetComponent(out AegisSpawnable spawnable) && spawnable is INetworked { NetID: not 0 } networked)
            return networked.NetID;

        return 0;
    }

    private static Node FindNode(uint netId) => Nodes.Find(n => n.Client.NetId == netId);

    private static bool Matches(Matrix4x4 a, Matrix4x4 b)
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(a[i] - b[i]) > Tolerance)
                return false;
        }

        return true;
    }

    private static int Clear()
    {
        int count = Nodes.Count + GameToys.Count;

        foreach (Node node in Nodes)
        {
            if (_player != null && !_player.IsDestroyed)
                node.Client.Destroy(_player);

            ClientSideRegistry.ForgetTest(node.Client.NetId);
            node.Client.Release();
        }

        for (int i = Parts.Length - 1; i >= 0; i--)
        {
            if (GameToys.TryGetValue(Parts[i].Name, out PrimitiveObjectToy toy) && toy != null)
                NetworkServer.Destroy(toy.gameObject);
        }

        Nodes.Clear();
        GameToys.Clear();
        _spawnOrder = new List<Node>();
        _gameSnapshot = null;
        _aegisSent = new HashSet<string>();

        if (_root != null)
            UnityEngine.Object.Destroy(_root);

        if (_gameRoot != null)
            UnityEngine.Object.Destroy(_gameRoot);

        _root = null;
        _gameRoot = null;
        _groupA = null;
        _leaf = null;
        _player = null;
        return count;
    }
}
