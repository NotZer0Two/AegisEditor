#nullable enable
using System.Collections.Generic;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.ServerSided;

/// <summary>
/// Class to make all <see cref="AegisSpawnable"/> be able to update.
/// </summary>
public abstract class ObjectUpdater : MonoBehaviour
{
    /// <summary>
    /// Instance of the current object updater.
    /// </summary>
    public static ObjectUpdater? Instance;

    /// <summary>
    /// The interval of updates.
    /// </summary>
    public virtual float Interval { get; set; } = 0.1f;

    /// <summary>
    /// List of all objects to update.
    /// </summary>
    public readonly List<AegisSpawnable> UpdatableObjects = new();

    private float _timer;

    /// <summary>
    /// Awake.
    /// </summary>
    protected virtual void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// When destroyed.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Register an object that needs to be updated.
    /// </summary>
    /// <param name="spawnable"></param>
    public virtual void Register(AegisSpawnable spawnable)
    {
        if (spawnable.UpdaterIndex >= 0)
            return;

        spawnable.UpdaterIndex = UpdatableObjects.Count;
        UpdatableObjects.Add(spawnable);
    }

    /// <summary>
    /// Unregister an object that doesn't need to be updated anymore.
    /// </summary>
    /// <param name="spawnable"></param>
    public virtual void Unregister(AegisSpawnable spawnable)
    {
        int index = spawnable.UpdaterIndex;
        if (index < 0)
            return;

        int last = UpdatableObjects.Count - 1;
        AegisSpawnable moved = UpdatableObjects[last];

        UpdatableObjects[index] = moved;
        moved.UpdaterIndex = index;

        UpdatableObjects.RemoveAt(last);
        spawnable.UpdaterIndex = -1;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < Interval)
            return;

        _timer = 0f;

        for (int i = UpdatableObjects.Count - 1; i >= 0; i--)
        {
            if (i < UpdatableObjects.Count)
                SendUpdate(UpdatableObjects[i]);
        }
    }

    /// <summary>
    /// Method used to send update or manipulate them.
    /// </summary>
    /// <param name="spawnable"></param>
    public virtual void SendUpdate(AegisSpawnable spawnable)
    {
        spawnable.SendUpdate();
    }
}