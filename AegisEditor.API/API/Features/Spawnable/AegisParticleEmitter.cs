using AegisEditor.API.Enum;
using AegisEditor.API.Features.Particles;
using AegisEditor.API.Interfaces;
using UnityEngine;

namespace AegisEditor.API.Features.Spawnable;

/// <summary>
/// Emits client-side primitive particles defined by an <see cref="AegisParticleSettings"/> asset.
/// </summary>
public class AegisParticleEmitter : AegisSpawnable
{
    private const byte Free = 0;
    private const byte Active = 1;
    private const byte Cooling = 2;
    private const byte CooldownTicks = 2;

    private struct Particle
    {
        public byte State;
        public byte Cooldown;
        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Parked;
        public Vector3 ParkedDirection;
        public float Age;
        public float Lifetime;
        public float Size;
    }

    public AegisParticleSettings settings;

    /// <summary>
    /// If the emitter starts playing as soon as its prefab spawns.
    /// </summary>
    public bool playOnSpawn = true;

    private Particle[] _particles;
    private int[] _free;
    private int _freeCount;
    private IParticleTarget _target;
    private float _lead;
    private float _accumulator;
    private float _emitCarry;
    private float _elapsed;
    private int _pendingBurst;
    private bool _emitting;
    private bool _playing;
    private bool _parkPending;

    /// <inheritdoc />
    public override SpawnableType Type => SpawnableType.Particles;

    /// <summary>
    /// If the emitter has everything it needs to run.
    /// </summary>
    public bool IsValid => settings != null;

    /// <summary>
    /// If the emitter is simulating, including particles still finishing after <see cref="Stop"/>.
    /// </summary>
    public bool IsPlaying => _playing;

    /// <summary>
    /// If the emitter is creating new particles.
    /// </summary>
    public bool IsEmitting => _playing && _emitting;

    /// <inheritdoc />
    public override void OnStart()
    {
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        if (!_playing)
            return;

        _accumulator += Time.deltaTime;
        if (_accumulator < settings.TickInterval)
            return;

        float dt = _accumulator;
        _accumulator = 0f;
        Tick(dt);
    }

    /// <summary>
    /// Runs one simulation step, skipped when nobody can see the particles.
    /// </summary>
    public void Tick(float dt)
    {
        if (!_playing || !_target.Begin())
            return;

        Step(Mathf.Min(dt, 0.25f));
    }

    /// <summary>
    /// Starts emitting, or resumes emitting if it was stopped and is still finishing.
    /// </summary>
    public void Play()
    {
        if (_target == null || !IsValid)
            return;

        if (_playing)
        {
            if (_emitting)
                return;

            _emitting = true;
            _elapsed = 0f;
            _pendingBurst = settings.burst;
            return;
        }

        int count = settings.maxParticles;
        _particles = new Particle[count];
        _free = new int[count];
        _freeCount = 0;
        _lead = MovementSmoothingUtil.Lead(settings.Smoothing);

        for (int i = 0; i < count; i++)
        {
            Park(ref _particles[i]);
            _particles[i].State = Cooling;
            _particles[i].Cooldown = CooldownTicks;
        }

        _accumulator = 0f;
        _emitCarry = 0f;
        _elapsed = 0f;
        _pendingBurst = settings.burst;
        _emitting = true;
        _parkPending = true;
        _playing = true;
    }

    /// <summary>
    /// Stops emitting, live particles finish.
    /// </summary>
    public void Stop() => _emitting = false;

    /// <summary>
    /// Connects the emitter to whatever sends its particles and starts playing if <see cref="playOnSpawn"/> is set. Called by the plugin.
    /// </summary>
    public void Bind(IParticleTarget target)
    {
        _target = target;

        if (playOnSpawn)
            Play();
    }

    /// <summary>
    /// Disconnects the emitter. Called by the plugin when the prefab despawns.
    /// </summary>
    public void Unbind()
    {
        _target = null;
        _playing = false;
    }

    /// <inheritdoc />
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Unbind();
    }

    private void Step(float dt)
    {
        if (_parkPending)
        {
            Color start = settings.ColorAt(0f);
            for (int i = 0; i < _particles.Length; i++)
                _target.Set(i, _particles[i].Parked, Vector3.zero, start);

            _parkPending = false;
            return;
        }

        if (_emitting)
        {
            _elapsed += dt;
            if (!settings.loop && _elapsed >= settings.duration)
                _emitting = false;
            else
                EmitTimed(dt);
        }

        bool anyAlive = false;
        for (int i = 0; i < _particles.Length; i++)
        {
            ref Particle p = ref _particles[i];
            if (p.State == Free)
                continue;

            anyAlive = true;
            if (p.State == Cooling)
            {
                if (--p.Cooldown == 0)
                {
                    p.State = Free;
                    _free[_freeCount++] = i;
                }

                continue;
            }

            p.Age += dt;
            if (p.Age >= p.Lifetime)
            {
                Kill(i, ref p);
                continue;
            }

            p.Velocity += settings.gravity * dt;
            p.Velocity *= 1f - Mathf.Min(1f, settings.drag * dt);
            p.Position += p.Velocity * dt;

            float t = p.Age / p.Lifetime;
            _target.Set(
                i,
                p.Position + p.Velocity * _lead,
                Vector3.one * (p.Size * settings.SizeAt(t)),
                settings.ColorAt(t));
        }

        if (!_emitting && !anyAlive)
            _playing = false;
    }

    private void EmitTimed(float dt)
    {
        while (_pendingBurst > 0 && Emit())
            _pendingBurst--;

        _emitCarry += settings.emissionRate * dt;
        while (_emitCarry >= 1f)
        {
            _emitCarry -= 1f;
            if (!Emit())
            {
                _emitCarry = 0f;
                break;
            }
        }
    }

    private bool Emit()
    {
        if (_freeCount == 0)
            return false;

        int index = _free[--_freeCount];
        ref Particle p = ref _particles[index];
        p.State = Active;
        p.Age = 0f;
        p.Lifetime = Mathf.Max(0.05f, Random.Range(settings.lifetime.x, settings.lifetime.y));
        p.Size = Random.Range(settings.startSize.x, settings.startSize.y);
        p.Position = p.Parked;
        p.Velocity = p.ParkedDirection * Random.Range(settings.startSpeed.x, settings.startSpeed.y);
        return true;
    }

    private void Kill(int index, ref Particle p)
    {
        p.State = Cooling;
        p.Cooldown = CooldownTicks;
        Park(ref p);
        _target.Set(index, p.Parked, Vector3.zero, settings.ColorAt(0f));
    }

    private void Park(ref Particle p)
    {
        settings.Sample(out Vector3 offset, out Vector3 direction);
        p.Parked = transform.TransformPoint(offset);
        p.ParkedDirection = transform.TransformDirection(direction);
    }
}