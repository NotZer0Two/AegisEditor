using System.Collections.Generic;
using UnityEngine;

namespace AegisEditor.API.Interfaces;

/// <summary>
/// Components returned here are not stripped when building a bundle.
/// </summary>
public interface IKeepComponents
{
    /// <summary>
    /// Components to keep.
    /// </summary>
    IEnumerable<Component> KeptComponents { get; }
}