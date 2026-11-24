using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AegisEditor.API.Enum;
using AegisEditor.API.Features.Editor;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.ThrowableProjectiles;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Logger = LabApi.Features.Console.Logger;

namespace AegisEditor.API.Features.Components;

/// <summary>
/// Runtime destructible bound to an <see cref="AegisHealthObject"/>.
/// </summary>
public class DestructableComponent : MonoBehaviour, IDestructible
{
    private static readonly Dictionary<ExplosionType, ItemType> ExplosionItems = new()
    {
        { ExplosionType.Grenade, ItemType.GrenadeHE },
        { ExplosionType.Disruptor, ItemType.ParticleDisruptor },
        { ExplosionType.Jailbird, ItemType.Jailbird },
        { ExplosionType.Cola, ItemType.SCP207 },
        { ExplosionType.PinkCandy, ItemType.SCP330 },
        { ExplosionType.SCP018, ItemType.SCP018 },
    };

    private DestructableComponent _owner;
    private AegisHealthObject _source;
    private float _health;
    private bool _dead;

    /// <inheritdoc />
    public uint NetworkId => 0;

    /// <inheritdoc />
    public Vector3 CenterOfMass => Vector3.zero;
    
    private static readonly HashSet<DestructableComponent> Roots = new();

    private void OnDestroy() => Roots.Remove(this);
    
    private int _lastFrame = -1;
    private DamageHandlerBase _lastHandler;

    public bool Damage(float damage, DamageHandlerBase handler, Vector3 exactHitPos)
    {
        if (_owner != this)
            return _owner.Damage(damage, handler, exactHitPos);

        if (_dead || _source == null)
            return false;

        if (_lastFrame == Time.frameCount && _lastHandler == handler)
            return false;

        _lastFrame = Time.frameCount;
        _lastHandler = handler;

        ReferenceHub hub = null;

        if (handler is AttackerDamageHandler { Attacker.Hub: not null } attacker)
        {
            hub = attacker.Attacker.Hub;
            damage *= _source.GetMultiplier((SelectableRoles)attacker.Attacker.Role, (SpawnableItemType)GetItem(handler, hub));
        }

        if (damage <= 0f)
            return false;

        damage *= 1f - _source.ArmorEfficient / 100f;

        if (hub != null && _source.ShowHitmarker && handler is not FirearmDamageHandler)
            Hitmarker.SendHitmarkerDirectly(hub, damage / 10f);

        Apply(damage, hub != null ? hub.gameObject : null);
        return true;
    }

    /// <summary>
    /// Damages a health object directly, ignoring damage rules and armor.
    /// </summary>
    /// <param name="source">The health object.</param>
    /// <param name="amount">The damage.</param>
    /// <param name="executor">The object that caused the damage.</param>
    public static void Hurt(AegisHealthObject source, float amount, GameObject executor)
    {
        DestructableComponent component = source.GetComponent<DestructableComponent>();
        if (component == null || component._dead)
            return;

        component.Apply(amount, executor);
    }

    /// <summary>
    /// Heals a health object up to its starting health.
    /// </summary>
    /// <param name="source">The health object.</param>
    /// <param name="amount">The health to add.</param>
    public static void Restore(AegisHealthObject source, float amount)
    {
        DestructableComponent component = source.GetComponent<DestructableComponent>();
        if (component == null || component._dead)
            return;

        component._health = Mathf.Min(component._health + amount, source.Health);
    }

    /// <summary>
    /// Attaches destructibles to the object and every collider below it.
    /// </summary>
    /// <param name="target">The object that holds the health object.</param>
    /// <param name="source">The API data.</param>
    public static void Attach(GameObject target, AegisHealthObject source)
    {
        DestructableComponent root = target.AddComponent<DestructableComponent>();
        root._owner = root;
        root._source = source;
        root._health = source.Health;
        Roots.Add(root);

        foreach (Collider collider in target.GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject == target)
                continue;

            DestructableComponent child = collider.gameObject.AddComponent<DestructableComponent>();
            child._owner = root;
        }
    }

    private void Apply(float damage, GameObject executor)
    {
        _health -= damage;
        _source.InvokeDamaged(executor);

        if (_health > 0f)
            return;

        _dead = true;
        Timing.CallDelayed(_source.DeadActionDelay, () =>
        {
            if (_source == null)
                return;

            _source.InvokeDead(executor);
            Destroy(_source.gameObject);
        });
    }

    private static ItemType GetItem(DamageHandlerBase handler, ReferenceHub hub)
    {
        if (handler is ExplosionDamageHandler explosion)
            return ExplosionItems.TryGetValue(explosion.ExplosionType, out ItemType item) ? item : ItemType.None;

        return hub.inventory.CurItem.TypeId;
    }
    
    /// <summary>
    /// Applies grenade damage to every health object in range.
    /// </summary>
    /// <param name="grenade">The exploding grenade.</param>
    /// <param name="position">The explosion position.</param>
    /// <param name="thrower">The player who threw it.</param>
    public static void Explode(ExplosionGrenade grenade, Vector3 position, ReferenceHub thrower)
    {
        AnimationCurve curve = grenade._playerDamageOverDistance;
        float range = curve.keys[curve.length - 1].time;

        foreach (DestructableComponent root in Roots.ToArray())
        {
            if (root._dead || root._source == null)
                continue;

            float distance = Vector3.Distance(root.transform.position, position);
            if (distance > range)
                continue;

            float damage = curve.Evaluate(distance);

            if (thrower != null)
                damage *= root._source.GetMultiplier((SelectableRoles)thrower.GetRoleId(), SpawnableItemType.GrenadeHE);

            if (damage <= 0f)
                continue;

            damage *= 1f - root._source.ArmorEfficient / 100f;

            if (thrower != null && root._source.ShowHitmarker)
                Hitmarker.SendHitmarkerDirectly(thrower, damage / 10f);

            root.Apply(damage, thrower != null ? thrower.gameObject : null);
        }
    }
}