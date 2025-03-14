/*****************************************************************************
// File Name :         FireBehavior.cs
// Author :            Mark Hanson
// Contributors:       Andrea Swihart-DeCoster
// Creation Date :     6/19/2024
//
// Brief Description : Function for controlling fire scale
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

public class FireBehavior : MonoBehaviour
{
    public static Action<FireBehavior> OnFireExtinguished;

    [Header("Settings")]
    [SerializeField] private int _waterCooldown;
    private float _timeBetweenScaleChanges = 0.01f;

    [Range(0f,50f)]
    [Tooltip("How much the fire changes size each time it grows or shrinks")]
    private float _absoluteScaleChange = 0.01f;

    [Range(0f, 2f)]
    [Tooltip("Minimum size the fire can start at.")]
    [SerializeField] private float _minFireSize;
    [Range(0f, 2f)]
    [SerializeField] private float _maxFireSize;

    [SerializeField] private FMODUnity.EventReference fireEvent;
    [SerializeField] private FMODUnity.EventReference fireDouseEvent;

    private EventInstance _fireInstance;
    // _scaleChange starts at _absoluteScaleChange then changes to pos or neg situationally
    private float _scaleChange;


    private ParticleSystem _particleSyst;

    private void Start()
    {
        // _scaleChange changes signs based on growing or shrinking and needs to be initialized with the default scale change value
        _scaleChange = _absoluteScaleChange;
        StartCoroutine(ChangeFireScale());

        _particleSyst = GetComponent<ParticleSystem>();
        _fireInstance = AudioManager.PlaySound(fireEvent, transform.position);
    }

    private void OnDestroy()
    {
        AudioManager.StopSound(_fireInstance);
    }

    #region Particle Trigger
    private void OnParticleTrigger()
    {
        // Particles inside and particles that have exited the collider
        List<ParticleSystem.Particle> particlesInside = new List<ParticleSystem.Particle>();
        List<ParticleSystem.Particle> particlesExited = new List<ParticleSystem.Particle>();

        // get the particles which matched the trigger conditions this frame
        int numParticleInside = _particleSyst.GetTriggerParticles(ParticleSystemTriggerEventType.Inside, particlesInside);
        int numParticlesExited = _particleSyst.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, particlesExited);

        // If there are no particles in the fire collider, start fire growth
        if (numParticlesExited > 0 && numParticleInside <= 0)
        {
            StartFireGrow();
        }
        // If there are particles in the fire collider, it should begin to grow
        else if (particlesInside.Count > 0)
        {
            StartFireShrink();
        }
    }
    #endregion Particle Trigger

    public void SetStartFireSize()
    {
        // Each fire should be a diff size so they get random sizes in the range
        float startingSize = UnityEngine.Random.Range(_minFireSize, _maxFireSize);
        transform.localScale = new Vector3(startingSize, startingSize, startingSize);
    }

    /// <summary>
    /// Begins the fire growing logic
    /// </summary>
    public void StartFireGrow()
    {
        if (_scaleChange <= 0)
        {
            StartCoroutine(CooldownBeforeGrowing());
        }
    }

    /// <summary>
    /// Begins the fire shrinking logic
    /// </summary>
    public void StartFireShrink()
    {
        if (_scaleChange >= 0)
        {
            StopCoroutine(CooldownBeforeGrowing());
            _scaleChange = -_absoluteScaleChange;
            AudioManager.PlaySound(fireDouseEvent, transform.position);
        }
    }

    /// <summary>
    /// After the cooldown period, the fire should start growing
    /// </summary>
    /// <returns></returns>
    private IEnumerator CooldownBeforeGrowing()
    {
        // Only start the cooldown if the fire is currently shrinking (interacting with water)
        if (_scaleChange <= 0)
        {
            _scaleChange = 0f;
            yield return new WaitForSeconds(_waterCooldown);
            _scaleChange = _absoluteScaleChange;
        }
    }

    /// <summary>
    /// Changes the scale of the fire.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeFireScale()
    {
        // Stops once size is 0
        while (transform.localScale.x > 0)
        {
            // Shouldn't grow if bigger than maxFireSize
            if (CanFireGrow() || _scaleChange < 0)
            {
                // Changes fire scale
                float newScale = transform.localScale.x + _scaleChange;
                transform.localScale = new Vector3(newScale, newScale, newScale);

                // Destroy object if fully shrunk
                if (transform.localScale.x <= 0)
                {
                    OnFireExtinguished?.Invoke(this);
                    Destroy(transform.parent.gameObject);
                }
            }

            yield return new WaitForSeconds(_timeBetweenScaleChanges);
        }
    }

    /// <summary>
    /// Checks if fire scale is positive (indicates it's supposed to grow) and that the size has not exceeded the max fire size
    /// </summary>
    /// <returns></returns>
    private bool CanFireGrow()
    {
        return _scaleChange > 0 && transform.localScale.x <= _maxFireSize;
    }
}
