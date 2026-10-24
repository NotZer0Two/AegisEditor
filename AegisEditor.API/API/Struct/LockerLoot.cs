#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using AegisEditor.API.Enum;
using UnityEngine;

namespace AegisEditor.API.Struct;

[Serializable]
public class LockerLoot
{
    [Tooltip("The item type that spawns if this loot is chosen. CustomItemName must be empty for this to work")]
    public SpawnableItemType ItemType;

    [Tooltip("The minimum amount of the ItemType to spawn in the chamber if this loot is chosen")]
    public int MinSpawn = 1;

    [Tooltip("The maximum amount of the ItemType to spawn in the chamber if this loot is chosen")]
    public int MaxSpawn = 1;

    [Tooltip("The weight this loot has in being chosen. Higher weights increase the chance this loot is chosen")]
    public int Weight = 1;

    [Tooltip("How many times this loot can be chosen")]
    public int MaxLootRepeats = 1;
}