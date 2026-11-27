using System.Collections.Generic;
using AegisEditor.API.Features;
using AegisEditor.API.Features.Editor;
using AegisEditor.API.Features.Particles;
using AegisEditor.API.Features.Spawnable;
using AegisEditor.API.Interfaces;
using UnityEditor;
using UnityEngine;

namespace AegisEditor.Editor.EditorView;

[CustomEditor(typeof(AegisParticleEmitter))]
public class EditorViewParticleEmitter : UnityEditor.Editor
{
    private static readonly Dictionary<AegisParticleEmitter, Preview> Previews = new();

    private UnityEditor.Editor _settingsEditor;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        AssemblyReloadEvents.beforeAssemblyReload += StopAll;
        EditorApplication.playModeStateChanged += _ => StopAll();
    }

    /// <inheritdoc />
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AegisParticleEmitter emitter = (AegisParticleEmitter)target;

        if (emitter.settings != null)
        {
            CreateCachedEditor(emitter.settings, null, ref _settingsEditor);
            _settingsEditor.OnInspectorGUI();
        }

        using (new EditorGUI.DisabledScope(!emitter.IsValid || Application.isPlaying))
        {
            if (!Previews.ContainsKey(emitter))
            {
                if (GUILayout.Button("Preview"))
                    Previews[emitter] = new Preview(emitter, () => Previews.Remove(emitter));
            }
            else if (GUILayout.Button("Stop Preview"))
            {
                Previews[emitter].Dispose();
                Previews.Remove(emitter);
            }
        }
    }

    private void OnDisable()
    {
        if (_settingsEditor != null)
            DestroyImmediate(_settingsEditor);
    }

    private static void StopAll()
    {
        foreach (Preview preview in new List<Preview>(Previews.Values))
            preview.Dispose();

        Previews.Clear();
    }

    private sealed class Preview : IParticleTarget
    {
        private readonly AegisParticleEmitter _emitter;
        private readonly System.Action _onEnded;
        private readonly AegisPrimitive[] _primitives;
        private readonly Vector3[] _targetPositions;
        private readonly Vector3[] _targetScales;
        private readonly float _smoothing;
        private readonly float _interval;
        private double _last;
        private float _accumulator;
        private bool _disposed;

        public Preview(AegisParticleEmitter emitter, System.Action onEnded)
        {
            _emitter = emitter;
            _onEnded = onEnded;

            AegisParticleSettings settings = emitter.settings;
            int count = settings.maxParticles;
            _smoothing = settings.Smoothing;
            _interval = settings.TickInterval;
            _primitives = new AegisPrimitive[count];
            _targetPositions = new Vector3[count];
            _targetScales = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                AegisPrimitive primitive = settings.CreateTemplate();
                primitive.gameObject.hideFlags = HideFlags.HideAndDontSave;
                primitive.InitializeInEditor();
                primitive.OnStart();

                if (primitive.ObjectCollider != null)
                    Object.DestroyImmediate(primitive.ObjectCollider);

                primitive.transform.SetPositionAndRotation(emitter.transform.position, Quaternion.identity);
                primitive.transform.localScale = Vector3.zero;

                _primitives[i] = primitive;
                _targetPositions[i] = emitter.transform.position;
            }

            _last = EditorApplication.timeSinceStartup;
            EditorApplication.update += Update;
            emitter.Bind(this);
            emitter.Play();
        }

        public bool Begin() => true;

        public void Set(int index, Vector3 position, Vector3 scale, Color color)
        {
            _targetPositions[index] = position;
            _targetScales[index] = scale;
            _primitives[index].Color = color;
            _primitives[index].OnUpdate();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            EditorApplication.update -= Update;

            if (_emitter != null)
                _emitter.Unbind();

            foreach (AegisPrimitive primitive in _primitives)
            {
                if (primitive == null)
                    continue;

                MaterialCache.StopUsingColor(primitive);
                primitive.IsParentOnly = true;
                Object.DestroyImmediate(primitive.gameObject);
            }

            SceneView.RepaintAll();
        }

        private void Update()
        {
            if (_emitter == null || !_emitter.IsPlaying)
            {
                Dispose();
                _onEnded();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float dt = Mathf.Min((float)(now - _last), 0.1f);
            _last = now;

            _accumulator += dt;
            if (_accumulator >= _interval)
            {
                _emitter.Tick(_accumulator);
                _accumulator = 0f;
            }

            float t = Mathf.Clamp01(dt * _smoothing * 0.3f);
            for (int i = 0; i < _primitives.Length; i++)
            {
                Transform tr = _primitives[i].transform;
                tr.position = Vector3.Lerp(tr.position, _targetPositions[i], t);
                tr.localScale = Vector3.Lerp(tr.localScale, _targetScales[i], t);
            }

            SceneView.RepaintAll();
        }
    }
}