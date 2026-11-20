using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Instantly destroys any <c>VisualEffect</c> Object once its completed
/// </summary>
[RequireComponent(typeof(VisualEffect))]
public class AutoDestroyVfxGraph : MonoBehaviour
{
    private VisualEffect _vfx;
    private bool _hasStarted;

    private void Awake()
    {
        _vfx = GetComponent<VisualEffect>();
    }

    private void Update()
    {
        if (!_hasStarted && _vfx.aliveParticleCount > 0)
        {
            _hasStarted = true;
        }
        else if (_hasStarted && _vfx.aliveParticleCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
