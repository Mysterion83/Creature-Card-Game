using System;
using UnityEngine;

public class Creature : MonoBehaviour
{
    public Player Owner { get; set; }

    public CreatureData Data;

    public Tile OccupyingTile;

    public float CurrentHealth;
    public float Experience;

    public Vector3 TargetPosition;

    [SerializeField] public GameObject HitParticlePrefab;

    public event Action<Creature> OnCreatureDeath;
    public event Action<Creature, float> OnCreatureDamaged;

    public void Awake()
    {
        TargetPosition = transform.position;
    }

    public void Update()
    {
        transform.position = Vector3.Lerp(transform.position, TargetPosition, Time.deltaTime * 5f);

        Vector3 movementDirection = TargetPosition - transform.position;
        movementDirection.y = 0f;

        if (movementDirection.sqrMagnitude <= 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void TakeDamage(CreatureData attacker, TileData attackerTile)
    {
        float damage = Mathf.Max(0, attacker.Attack * attackerTile.AttackerTileModifier - Data.Defense);

        CurrentHealth -= damage;
        OnCreatureDamaged?.Invoke(this, damage);
        if (HitParticlePrefab != null)
        {
            GameObject hitParticle = Instantiate(HitParticlePrefab, transform.position, Quaternion.identity);
            Material hitMaterial = hitParticle.GetComponentInChildren<Renderer>().material;
            hitMaterial.color = attacker.Element.Color;
        }
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnCreatureDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public Vector2Int[] ShowValidMovement()
    {
        return Data.MovementPattern.ValidPositions;
    }
    public Vector2Int[] ShowValidAttacks()
    {
        return Data.AttackPattern.ValidPositions;
    }

    public Creature(CreatureData data, Player owner)
    {
        Data = data;
        Owner = owner;
        CurrentHealth = data.MaxHealth;
        Experience = 0;
    }
}