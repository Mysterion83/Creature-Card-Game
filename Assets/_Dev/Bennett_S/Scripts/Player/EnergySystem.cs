using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Stats")]
    [SerializeField] private Resource _energy;
    [SerializeField] private float _baseEnergyPerRound = 15f;
    [SerializeField] private float _bonusEnergyHardCap = 50f;
    
    private Resource _bonusEnergy;
    
    // Resource accessors
    public Resource Energy => _energy;
    public Resource BonusEnergy => _bonusEnergy;

    private void Awake()
    {
        _bonusEnergy = new Resource(_bonusEnergyHardCap, 0);
    }

    /// <summary>
    /// Recharge energy at the start of a turn, applies base plus bonus energy
    /// </summary>
    public void Recharge() => AddEnergy(_baseEnergyPerRound + _bonusEnergy.CurrentAmount);
    
    /// <summary>
    /// Add energy to the resource
    /// </summary>
    /// <param name="amount">Amount to add</param>
    public void AddEnergy(float amount) => _energy.Add(amount);
    /// <summary>
    /// Remove energy from the resource
    /// </summary>
    /// <param name="amount">Amount to remove</param>
    public void RemoveEnergy(float amount) => _energy.Remove(amount);
    /// <summary>
    /// Refresh energy back to full
    /// </summary>
    public void Refresh() => _energy.Fill();
    /// <summary>
    /// Set energy to zero
    /// </summary>
    public void ZeroEnergy() => _energy.Deplete();
    
    /// <summary>
    /// Add bonus energy to gain each round
    /// </summary>
    /// <param name="amount">The amount to gain</param>
    public void AddBonusEnergy(float amount) => _bonusEnergy.Add(amount);
    /// <summary>
    /// Lower bonus energy gain
    /// </summary>
    /// <param name="amount">Amount to remove</param>
    public void RemoveBonusEnergy(float amount) => _bonusEnergy.Remove(amount);
    /// <summary>
    /// Set bonus energy gain to zero
    /// </summary>
    public void ZeroBonusEnergy() => _bonusEnergy.Deplete();
}
