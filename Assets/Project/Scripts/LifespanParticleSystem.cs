using UnityEngine;

public class LifespanParticleSystem : MonoBehaviour
{
    [SerializeField] private float _lifespan = 5f;
    private void Start()
    {
        Invoke(nameof(Despawn), _lifespan);
    }
    private void Despawn()
    {
        Destroy(gameObject);
    }
}
