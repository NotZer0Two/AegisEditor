using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEngine;

namespace AegisEditor.API.Features.Editor;

public class AegisHealthObject : MonoBehaviour
{
    public void Awake()
    {
        AegisServerCalls.RegisterDestruction.Invoke(gameObject, this);
    }

    /// <summary>
    /// Starting health.
    /// </summary>
    public float Health = 100f;

    /// <summary>
    /// Armor efficiency applied to incoming damage.
    /// </summary>
    [Range(0, 100)]
    public int ArmorEfficient;
    
    /// <summary>
    /// Delay in seconds before the death behaviours run.
    /// </summary>
    public float DeadActionDelay;
    
    /// <summary>
    /// Shows a hitmarker to the attacker when damaged.
    /// </summary>
    public bool ShowHitmarker = true;
    
    /// <summary>
    /// Damage rules checked in order. The first matching rule applies.
    /// </summary>
    public List<AegisDamageRule> DamageRules = new();
    
    /// <summary>
    /// Actions executed on death.
    /// </summary>
    public List<AegisAction> OnDead = new();
    
    /// <summary>
    /// Actions executed every time the object takes damage.
    /// </summary>
    public List<AegisAction> OnDamaged = new();

    /// <summary>
    /// Executes the death actions.
    /// </summary>
    /// <param name="executor">The object that caused the death.</param>
    public void InvokeDead(GameObject executor = null)
    {
        foreach (AegisAction action in OnDead)
        {
            if (action != null)
                action.Execute(executor);
        }
    }
    
    /// <summary>
    /// Executes the damage actions.
    /// </summary>
    /// <param name="executor">The object that caused the damage.</param>
    public void InvokeDamaged(GameObject executor = null)
    {
        foreach (AegisAction action in OnDamaged)
        {
            if (action != null)
                action.Execute(executor);
        }
    }
    
    /// <summary>
    /// Gets the damage multiplier for an attacker.
    /// </summary>
    /// <param name="role">The attacker's role.</param>
    /// <param name="item">The item the damage came from.</param>
    /// <returns>0 blocks the damage, above 1 boosts it. 1 when no rule matches.</returns>
    public float GetMultiplier(SelectableRoles role, SpawnableItemType item)
    {
        foreach (AegisDamageRule rule in DamageRules)
        {
            if (rule.Matches(role, item))
                return rule.Multiplier;
        }

        return 1f;
    }
    
    /// <summary>
    /// Changes or blocks damage for specific roles and items.
    /// </summary>
    [Serializable]
    public class AegisDamageRule
    {
        /// <summary>
        /// Roles this rule applies to. Empty matches every role.
        /// </summary>
        public List<SelectableRoles> Roles = new();

        /// <summary>
        /// Items this rule applies to. Empty matches every item.
        /// </summary>
        public List<SpawnableItemType> Items = new();

        /// <summary>
        /// Damage multiplier. 0 blocks the damage, above 1 boosts it.
        /// </summary>
        public float Multiplier = 1f;

        /// <summary>
        /// Checks whether the rule applies.
        /// </summary>
        /// <param name="role">The attacker's role.</param>
        /// <param name="item">The item the damage came from.</param>
        /// <returns>True if both filters match.</returns>
        public bool Matches(SelectableRoles role, SpawnableItemType item)
            => (Roles.Count == 0 || Roles.Contains(role)) && (Items.Count == 0 || Items.Contains(item));
    }
}