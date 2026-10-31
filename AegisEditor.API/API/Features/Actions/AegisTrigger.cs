using System.Collections.Generic;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Actions;

/// <summary>
/// Runs actions when something enters, stays in, or exits this collider.
/// </summary>
[RequireComponent(typeof(Collider))]
public class AegisTrigger : MonoBehaviour, IKeepComponents
{
    /// <summary>
    /// Actions run when something enters.
    /// </summary>
    public List<AegisAction> onEnter = new();

    /// <summary>
    /// Actions run every physics step while something stays inside.
    /// </summary>
    public List<AegisAction> onStay = new();

    /// <summary>
    /// Actions run when something exits.
    /// </summary>
    public List<AegisAction> onExit = new();

    /// <inheritdoc />
    public IEnumerable<Component> KeptComponents => GetComponents<Collider>();
    
    /// <summary>
    /// Only allow <see cref="onEnter"/> to run once.
    /// </summary>
    public bool once;

    private bool _used;

    private void OnTriggerEnter(Collider other) => Enter(other);

    private void OnCollisionEnter(Collision collision) => Enter(collision.collider);

    private void OnTriggerStay(Collider other) => Run(onStay, other);

    private void OnTriggerExit(Collider other) => Run(onExit, other);

    private void Enter(Collider other)
    {
        if (once && _used)
            return;

        _used = true;
        Run(onEnter, other);
    }

    private static void Run(List<AegisAction> actions, Collider other)
    {
        foreach (AegisAction action in actions)
        {
            if (action != null)
                action.Execute(other.gameObject);
        }
    }
}