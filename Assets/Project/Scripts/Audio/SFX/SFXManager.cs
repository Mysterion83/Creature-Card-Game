using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class SFXManager : MonoBehaviour
{
    [Header("SFX Emitter Settings")]
    [SerializeField] private SFXEmitter _sfxEmitterPrefab;
    [SerializeField] private Transform _container;
    
    [Header("Pool Settings")]
    [Tooltip("The default pool capacity, will reduce down to this size when extra capacity is not needed")]
    [SerializeField] private int _defaultCapacity = 15;
    [Tooltip("The absolute maximum pool capacity, will not increase past this size")]
    [SerializeField] private int _maxCapacity = 50;
    [Tooltip("Enable to double check the collection, helps prevent dual-release mistakes")]
    [SerializeField] private bool _doCollectionCheck;
    
    private IObjectPool<SFXEmitter> _sfxEmitterPool;
    public readonly List<SFXEmitter> ActiveSoundEmitters = new();
    
    private void Awake()
    {
        InitialisePool();
    }

    private void InitialisePool()
    {
        _sfxEmitterPool = new ObjectPool<SFXEmitter>(
            createFunc: CreateSoundEmitter,
            actionOnGet: OnTakeFromPool,
            actionOnRelease: OnReturnedToPool,
            actionOnDestroy: OnDestroyedPoolObject,
            collectionCheck: _doCollectionCheck,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxCapacity);
    }

    #region Playback
    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c>
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <returns>The allocated <c>SFXEmitter</c> to play the provided sound</returns>
    public SFXEmitter Play(SFXDefinition sfxDefinition)
    {
        return Play(sfxDefinition, transform.position);
    }

    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c> at a given world-space position
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <param name="position">The world-space position to play the sound at</param>
    /// <returns>The allocated <c>SFXEmitter</c> to play the provided sound</returns>
    public SFXEmitter Play(SFXDefinition sfxDefinition, Vector3 position)
    {
        if (!sfxDefinition || !sfxDefinition.IsValid()) return null;
        
        // Grab an emitter from the pool, initialise and then play it
        SFXEmitter emitter = _sfxEmitterPool.Get();
        emitter.transform.position = position;
        emitter.Initialise(sfxDefinition);
        emitter.Play();

        return emitter;
    }
    
    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c> under a given Transform with an optional offset
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <param name="anchor">The Transform to play under</param>
    /// <param name="offset">The local position offset within the anchor</param>
    /// <returns>The allocated <c>SFXEmitter</c> to play the provided sound</returns>
    public SFXEmitter Play(SFXDefinition sfxDefinition, Transform anchor, Vector3 offset = default)
    {
        return !anchor ? null : Play(sfxDefinition, anchor.TransformPoint(offset));
    }
    
    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c> after a delay
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <param name="delay">The delay in seconds until the sound plays</param>
    public void PlayDelayed(SFXDefinition sfxDefinition, float delay)
    {
        PlayDelayed(sfxDefinition, delay, transform.position);
    }

    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c> after a delay at a given world-space position 
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <param name="delay">The delay in seconds until the sound plays</param>
    /// <param name="position">The world-space position to play the sound at</param>
    public void PlayDelayed(SFXDefinition sfxDefinition, float delay, Vector3 position)
    {
        if (!sfxDefinition) return;

        if (delay <= 0f)
        {
            Play(sfxDefinition, position);
            return;
        }

        StartCoroutine(PlayDelayedRoutine(sfxDefinition, delay, position));
    }

    /// <summary>
    /// Play a Sound Effect from a provided <c>SFXDefinition</c> at a given world-space position after a given delay
    /// </summary>
    /// <param name="sfxDefinition">The <c>SFXDefinition</c> to retrieve from</param>
    /// <param name="delay">The delay in seconds until the sound plays</param>
    /// <param name="anchor">The transform to play under</param>
    /// <param name="offset">The local position offset within the anchor</param>
    public void PlayDelayed(SFXDefinition sfxDefinition, float delay, Transform anchor, Vector3 offset = default)
    {
        // If the anchor exists then allocate its position with any offset, otherwise use a fallback (SFXManager's position)
        Vector3 position = anchor ? anchor.TransformPoint(offset) : transform.position;
        PlayDelayed(sfxDefinition, delay, position);
    }
    #endregion
    
    #region Delay Coroutines
    private IEnumerator PlayDelayedRoutine(SFXDefinition sfxDefinition, float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);
        Play(sfxDefinition, position);
    }
    #endregion
    
    /// <summary>
    /// Wrapper method for retrieving a <c>SFXEmitter</c> from the pool
    /// </summary>
    public SFXEmitter Get()
    {
        return _sfxEmitterPool.Get();
    }
    
    /// <summary>
    /// Wrapper method for releasing a <c>SFXEmitter</c> back to the pool
    /// </summary>
    public void ReleaseToPool(SFXEmitter sfxEmitter)
    {
        sfxEmitter.transform.localPosition = Vector3.zero;
        _sfxEmitterPool.Release(sfxEmitter);
    }
    
    #region IObjectPool Callback Methods
    private SFXEmitter CreateSoundEmitter()
    {
        SFXEmitter emitter = Instantiate(_sfxEmitterPrefab, _container);
        emitter.gameObject.SetActive(false);
        return emitter;
    }
    
    private void OnTakeFromPool(SFXEmitter sfxEmitter)
    {
        sfxEmitter.gameObject.SetActive(true);
        ActiveSoundEmitters.Add(sfxEmitter);
    }

    private void OnReturnedToPool(SFXEmitter sfxEmitter)
    {
        sfxEmitter.gameObject.SetActive(false);
        ActiveSoundEmitters.Remove(sfxEmitter);
    }
    
    private void OnDestroyedPoolObject(SFXEmitter sfxEmitter)
    {
        if (!sfxEmitter) return; 
        Destroy(sfxEmitter.gameObject);
    }
    #endregion
}