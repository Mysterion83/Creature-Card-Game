using System;
using UnityEngine;

/// <summary>
/// Class for controlling and manipulating a resource between 0 and a given maximum value.
/// </summary>
[Serializable]
public class Resource
{
    [SerializeField] private float currentAmount;
    [SerializeField] private float maxAmount;

    public float CurrentAmount => currentAmount;
    public float MaxAmount => maxAmount;
    
    #region Events
    /// <summary>
    /// Called when the resource increases; passes the increase amount
    /// </summary>
    public event Action<Resource, float> OnIncreased;
    /// <summary>
    /// Called when the resource decreases; passes the decrease amount
    /// </summary>
    public event Action<Resource, float> OnDecreased;
    /// <summary>
    /// Called when the resource is equal to its maximum amount
    /// </summary>
    public event Action<Resource> OnFilled;
    /// <summary>
    /// Called when the resource reaches 0
    /// </summary>
    public event Action<Resource> OnDepleted;
    #endregion

    #region Constructors
    public Resource(float maxAmount, bool isFull)
    {
        currentAmount = isFull ? maxAmount : 0f;
        this.maxAmount = maxAmount;
    }
    public Resource(float maxAmount, float currentAmount)
    {
        this.currentAmount = currentAmount;
        this.maxAmount = maxAmount;
    }
    #endregion
    
    /// <summary>
    /// Fills the resource by a given amount, exits if amount is negative or resource is full
    /// </summary>
    /// <param name="amount">Amount to add to the <c>Resource</c></param>
    public void Add(float amount)
    {
        if (amount <= 0 || currentAmount >= maxAmount) return;
        
        float prevAmount = currentAmount;
        currentAmount = MathF.Min(currentAmount + amount, maxAmount);
        
        float actualAmount = currentAmount - prevAmount;
        OnIncreased?.Invoke(this, actualAmount);
        
        if (currentAmount >= maxAmount) OnFilled?.Invoke(this);
    }
    
    /// <summary>
    /// Removes a given amount from the resource
    /// </summary>
    /// <param name="amount">Amount to remove from the <c>Resource</c></param>
    public void Remove(float amount)
    {
        if (amount <= 0) return; 
        
        float prevAmount = currentAmount;
        currentAmount = MathF.Max(currentAmount - amount, 0f);
        
        float actualAmount = prevAmount - currentAmount;
        OnDecreased?.Invoke(this, actualAmount);
        
        if (currentAmount <= 0) OnDepleted?.Invoke(this);
    }

    /// <summary>
    /// Fill the resource
    /// </summary>
    public void Fill()
    {
        if (currentAmount >= maxAmount) return;
        currentAmount = maxAmount;
        OnFilled?.Invoke(this);
    }
    
    /// <summary>
    /// Set the resource to 0
    /// </summary>
    public void Deplete()
    {
        if (currentAmount <= 0) return;
        currentAmount = 0;
        OnDepleted?.Invoke(this);
    }

    #region Setters
    /// <summary>
    /// Set the new current amount for a resource
    /// </summary>
    /// <param name="newAmount">The new current amount for this resource</param>
    public void SetCurrentAmount(float newAmount) => currentAmount = Mathf.Clamp(newAmount, 0f, maxAmount);
    
    /// <summary>
    /// Set the new maximum amount for a resource
    /// </summary>
    /// <param name="newMaxAmount">The new max amount for this resource</param>
    /// <param name="adjustCurrentResource">If the current amount should equal the new maximum</param>
    public void SetMaxAmount(float newMaxAmount, bool adjustCurrentResource)
    {
        this.maxAmount = newMaxAmount;
        if (!adjustCurrentResource) return;
        
        currentAmount = Mathf.Min(currentAmount, this.maxAmount);
    }
    #endregion
}
