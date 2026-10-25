using UnityEngine;

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Stats")]
    [SerializeField] private Resource _energy;
    
    public Resource Energy => _energy;

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
}
