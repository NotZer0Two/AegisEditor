using System.Collections;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Teleports whatever enters this collider to <see cref="destination"/>, optionally restricted by role.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AegisTriggerTeleport : MonoBehaviour, IKeepComponents
{
    private static readonly List<AegisTriggerTeleport> _instances = new();

    private readonly HashSet<GameObject> _ignored = new();
    private readonly Dictionary<GameObject, int> _inside = new();
    private readonly Dictionary<GameObject, Coroutine> _pending = new();

    [SerializeField]
    private GameObject _destination;

    private AegisTriggerTeleport? _next;

    /// <summary>
    /// Every active teleporter that has been registered.
    /// </summary>
    public static IReadOnlyList<AegisTriggerTeleport> Instances => _instances;

    /// <summary>
    /// Optional identifier used to find this teleporter at runtime.
    /// </summary>
    public string id;

    /// <summary>
    /// Optional id of the teleporter to teleport to. Only used when <see cref="destination"/> is not set.
    /// </summary>
    public string destinationId;

    /// <summary>
    /// The object to teleport to. Must have the same geometry as this object.
    /// </summary>
    public GameObject destination
    {
        get => _destination;
        set
        {
            _destination = value;
            _next = null;

            if (value != null)
                value.TryGetComponent(out _next);
        }
    }

    /// <summary>
    /// Roles allowed to teleport. If empty, everything is allowed.
    /// </summary>
    public List<SelectableRoles> allowedRoles = new();

    /// <summary>
    /// Seconds to wait before teleporting. If 0, teleports instantly. Leaving the trigger cancels the teleport.
    /// </summary>
    public float delay;

    /// <summary>
    /// Whether to compensate for ping when teleporting players.
    /// </summary>
    public bool lagCompensation = true;

    /// <summary>
    /// Actions run on the teleported object after it has been teleported.
    /// </summary>
    public List<AegisAction> onTeleport = new();

    /// <inheritdoc />
    public IEnumerable<Component> KeptComponents => GetComponents<Collider>();

    /// <summary>
    /// Gets the first teleporter with the given id.
    /// </summary>
    /// <param name="id">The id to look for.</param>
    /// <param name="teleporter">The teleporter found, if any.</param>
    /// <returns>If a teleporter was found.</returns>
    public static bool TryGet(string id, out AegisTriggerTeleport teleporter)
    {
        teleporter = null;

        if (string.IsNullOrEmpty(id))
            return false;

        foreach (AegisTriggerTeleport instance in _instances)
        {
            if (instance.id == id)
            {
                teleporter = instance;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets every teleporter with the given id.
    /// </summary>
    /// <param name="id">The id to look for.</param>
    /// <returns>The teleporters found.</returns>
    public static IEnumerable<AegisTriggerTeleport> GetAll(string id)
    {
        foreach (AegisTriggerTeleport instance in _instances)
        {
            if (instance.id == id)
                yield return instance;
        }
    }

    /// <summary>
    /// Replaces the allowed roles at runtime. An empty collection allows everything.
    /// </summary>
    /// <param name="roles">The roles allowed to teleport.</param>
    public void SetAllowedRoles(IEnumerable<SelectableRoles> roles)
    {
        allowedRoles.Clear();
        allowedRoles.AddRange(roles);
    }

    private void Awake()
    {
        destination = _destination;
        _instances.Add(this);
    }

    private void OnDestroy() => _instances.Remove(this);

    private void OnTriggerEnter(Collider other)
    {
        GameObject target = GetTarget(other);

        _inside.TryGetValue(target, out int count);
        _inside[target] = count + 1;

        if (count > 0)
            return;

        if (_ignored.Contains(target) || !IsAllowed(target) || !ResolveDestination())
            return;

        if (delay <= 0f)
            Teleport(target);
        else
            _pending[target] = StartCoroutine(TeleportDelayed(target));
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject target = GetTarget(other);

        if (!_inside.TryGetValue(target, out int count))
            return;

        if (count > 1)
        {
            _inside[target] = count - 1;
            return;
        }

        _inside.Remove(target);
        _ignored.Remove(target);

        if (_pending.TryGetValue(target, out Coroutine routine))
        {
            StopCoroutine(routine);
            _pending.Remove(target);
        }
    }

    private void OnDisable()
    {
        _pending.Clear();
        _ignored.Clear();
        _inside.Clear();
    }

    private static GameObject GetTarget(Collider other)
    {
        return other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
    }

    private bool ResolveDestination()
    {
        if (destination != null)
            return true;

        if (TryGet(destinationId, out AegisTriggerTeleport found) && found != this)
            destination = found.gameObject;

        return destination != null;
    }

    private bool IsAllowed(GameObject target)
    {
        if (allowedRoles.Count == 0)
            return true;

        SelectableRoles role = AegisServerCalls.GetRole?.Invoke(target) ?? SelectableRoles.None;
        return allowedRoles.Contains(role);
    }

    private IEnumerator TeleportDelayed(GameObject target)
    {
        yield return new WaitForSeconds(delay);

        _pending.Remove(target);

        if (target != null)
            Teleport(target);
    }

    private void Teleport(GameObject target)
    {
        if (_next != null)
            _next._ignored.Add(target);

        AegisServerCalls.Teleport?.Invoke(target, gameObject, destination, lagCompensation);

        foreach (AegisAction action in onTeleport)
        {
            if (action != null)
                action.Execute(target);
        }
    }
}