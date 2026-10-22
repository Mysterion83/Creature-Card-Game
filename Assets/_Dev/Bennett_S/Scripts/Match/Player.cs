using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private PlayerData _data;
    public PlayerData Data => _data;
    
    private void OnEnable()
    {
        PlayerRegistry.RegisterPlayer(_data.Name, this);
    }

    private void OnDisable()
    {
        PlayerRegistry.UnregisterPlayer(_data.Name);
    }

    public void StartTurn()
    {
        Debug.Log($"Starting turn for {_data.Name}");
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for {_data.Name}");
    }
}