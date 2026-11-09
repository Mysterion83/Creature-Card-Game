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