using System.Collections.Generic;
using AegisEditor.API.Enum;
using AegisEditor.API.Extensions;
using AegisEditor.API.Interfaces;
using TMPro;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Text toy.
/// </summary>
public class AegisText : AegisSpawnable, INetworked, IPreCheckUseless
{
    private string _lastText;
    private Vector2 _lastSize;
    
    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Text;
    
    /// <summary>
    /// The size of the display / text mesh pro.
    /// </summary>
    public Vector2 displaySize = new Vector2(200f, 50f);
    
    /// <summary>
    /// Text.
    /// </summary>
    public string text = "Hello World!";
    
    /// <summary>
    /// If true, rotation is marked dirty every update so the client keeps it facing the camera.
    /// </summary>
    public bool billboard;
    
    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!Application.isEditor)
            return;
        
        if (text != _lastText)
        {
            if (textMesh != null)
            {
                textMesh.text = text;
            }
            
            _lastText = text;
        }
        
        if (displaySize != _lastSize) 
        {
            if (textMesh != null)
                textMesh.rectTransform.sizeDelta = displaySize;
            
            _lastSize = displaySize;
        }
    }
    
    /// <inheritdoc />
    public override bool UpdateClient(List<(PropertyUpdate propertyName, object value)> value)
    {
        bool changed = base.UpdateClient(value);

        if (billboard && !value.Exists(x => x.propertyName == PropertyUpdate.Rotation))
        {
            value.Add((PropertyUpdate.Rotation, transform.rotation));
            changed = true;
        }
        
        if (text != _lastText)
        {
            value.Add((PropertyUpdate.Text, text));
            _lastText = text;
            changed = true;
        }
        
        if (displaySize != _lastSize)
        {
            value.Add((PropertyUpdate.DisplaySize, displaySize));
            _lastSize = displaySize;
            changed = true;
        }

        return changed;
    }
    
    private TextMeshPro textMesh;

    /// <inheritdoc />
    public override void InitializeInEditor()
    {
        textMesh = this.GetComponentOrCreate<TextMeshPro>();

        textMesh.hideFlags = HideFlags.HideInInspector;
        textMesh.rectTransform.hideFlags = HideFlags.HideInInspector;

        var meshRenderer = textMesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.hideFlags = HideFlags.HideInInspector;

        var meshFilter = textMesh.GetComponent<MeshFilter>();
        if (meshFilter != null) meshFilter.hideFlags = HideFlags.HideInInspector;

        textMesh.text = text;
        textMesh.enableAutoSizing = false;
        textMesh.margin = Vector4.zero;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.rectTransform.sizeDelta = displaySize;
    }
    
    /// <inheritdoc />
    [HideInInspector]
    public uint NetID { get; set; }
    
    /// <inheritdoc />
    public bool ShouldSpawn() => string.IsNullOrEmpty(text);
}