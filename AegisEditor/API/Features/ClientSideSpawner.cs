using System;
using System.Collections.Generic;
using System.Diagnostics;
using LabApi.Features.Console;
using LabApi.Features.Wrappers;
using MEC;

namespace AegisEditor.API.Features;

/// <summary>
/// Occupies to spawn the objects to a client in a more performant way.
/// </summary>
public static class ClientSideSpawner
{
    private readonly record struct Job(List<NetworkedObject> Batch, NetworkedObject Obj, Action<Player> OnLast);
    
    public static double BudgetMs = 2.0;

    private static readonly Dictionary<Player, Queue<Job>> Pending = new();
    private static readonly HashSet<NetworkedObject> Skipped = new();
    private static readonly List<Player> Finished = new();
    private static CoroutineHandle _handle;

    /// <summary>
    /// Enqueues a player to spawn a schematic.
    /// </summary>
    /// <param name="player">The player the schematic must be seen.</param>
    /// <param name="objects">List of all client sided objects.</param>
    /// <param name="onComplete">Action to call when completed.</param>
    public static void Enqueue(Player player, List<NetworkedObject> objects, Action<Player> onComplete = null)
    {
        if (objects.Count == 0)
        {
            onComplete?.Invoke(player);
            return;
        }

        if (!Pending.TryGetValue(player, out Queue<Job> queue))
            Pending[player] = queue = new Queue<Job>(objects.Count);

        for (int i = 0; i < objects.Count; i++)
            queue.Enqueue(new Job(objects, objects[i], i == objects.Count - 1 ? onComplete : null));

        if (!_handle.IsRunning)
            _handle = Timing.RunCoroutine(Process(), Segment.Update);
    }

    /// <summary>
    /// Cancel the player form the list of Spawning.
    /// </summary>
    /// <param name="player"></param>
    public static void Cancel(Player player) => Pending.Remove(player);
    
    /// <summary>
    /// Delets a specific job from the player.
    /// </summary>
    /// <param name="player">Player to remove.</param>
    /// <param name="batch">What batch to remove.</param>
    public static void Cancel(Player player, List<NetworkedObject> batch)
    {
        if (!Pending.TryGetValue(player, out Queue<Job> queue))
            return;

        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            Job job = queue.Dequeue();
            if (job.Batch != batch)
                queue.Enqueue(job);
        }
    }
    
    /// <summary>
    /// Prevents a queued object from being spawned.
    /// </summary>
    /// <param name="obj">Object to skip.</param>
    public static void Skip(NetworkedObject obj)
    {
        if (Pending.Count > 0)
            Skipped.Add(obj);
    }

    private static IEnumerator<float> Process()
    {
        long budget = (long)(BudgetMs * Stopwatch.Frequency / 1000.0);

        while (Pending.Count > 0)
        {
            long end = Stopwatch.GetTimestamp() + budget;

            foreach (KeyValuePair<Player, Queue<Job>> pair in Pending)
            {
                Player player = pair.Key;
                Queue<Job> queue = pair.Value;

                if (player.IsDestroyed)
                {
                    Finished.Add(player);
                    continue;
                }

                while (queue.Count > 0 && Stopwatch.GetTimestamp() < end)
                {
                    Job job = queue.Dequeue();
                    if (!Skipped.Contains(job.Obj))
                        job.Obj.Spawn(player);
                    job.OnLast?.Invoke(player);
                }

                if (queue.Count == 0)
                    Finished.Add(player);
            }

            foreach (Player player in Finished)
                Pending.Remove(player);

            Finished.Clear();
            yield return Timing.WaitForOneFrame;
        }
        Skipped.Clear();
    }
}