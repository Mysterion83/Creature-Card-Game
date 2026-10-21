using UnityEngine;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] string _playerName;
    public string Name => _playerName;

    private void OnEnable()
    {
        PlayerRegistry.RegisterPlayer(this);
    }

    private void OnDisable()
    {
        PlayerRegistry.UnregisterPlayer(this);
    }

    public void StartTurn()
    {
        Debug.Log($"Starting turn for {_playerName}");
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for {_playerName}");
    }
}
