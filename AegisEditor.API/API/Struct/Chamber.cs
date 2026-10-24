#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using System.Collections.Generic;
using AegisEditor.API.Enum;
using UnityEngine;
using UnityEngine.Serialization;

namespace AegisEditor.API.Struct;

[Serializable]
public class Chamber
{
    [Tooltip("The list of valid item types which can spawn in this chamber type. Loot with ItemTypes not on this list will not spawn in this chamber type")]
    public List<SpawnableItemType> validItemTypes = new();
}

[Serializable]
public class ChamberCard : Chamber
{
    [Tooltip("The keycard permissions for this chamber type")]
    public DoorPermissionFlags keycardPermissions = DoorPermissionFlags.None;
}