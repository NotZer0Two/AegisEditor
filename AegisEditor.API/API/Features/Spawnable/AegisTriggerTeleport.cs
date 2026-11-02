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
    private readonly HashSet<GameObject> _ignored = new();
    private readonly Dictionary<GameObject, int> _inside = new();
    private readonly Dictionary<GameObject, Coroutine> _pending = new();

    [SerializeField]
    private GameObject _destination;

    private AegisTriggerTeleport? _next;

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
    /// Replaces the allowed roles at runtime. An empty collection allows everything.
    /// </summary>
    /// <param name="roles">The roles allowed to teleport.</param>
    public void SetAllowedRoles(IEnumerable<SelectableRoles> roles)
    {
        allowedRoles.Clear();
        allowedRoles.AddRange(roles);
    }

    private void Awake() => destination = _destination;

    private void OnTriggerEnter(Collider other)
    {
        GameObject target = GetTarget(other);

        _inside.TryGetValue(target, out int count);
        _inside[target] = count + 1;

        if (count > 0)
            return;

        if (destination == null || _ignored.Contains(target) || !IsAllowed(target))
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