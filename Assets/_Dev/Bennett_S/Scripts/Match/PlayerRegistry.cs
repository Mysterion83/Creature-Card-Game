using System.Collections.Generic; 

public static class PlayerRegistry
{
    private static readonly List<IPlayer> _registeredPlayers;

    public static void RegisterPlayer(IPlayer player)
    {
        if (_registeredPlayers.Contains(player)) return;
        _registeredPlayers.Add(player);
    }
    public static void UnregisterPlayer(IPlayer player)
    {
        if (!_registeredPlayers.Contains(player)) return;
        _registeredPlayers.Remove(player);
    }

    public static List<IPlayer> GetAllRegistered()
    {
        return new List<IPlayer>(_registeredPlayers);
    }
    
    public static IPlayer GetPlayer(int index)
    {
        if (index >= 0 && index < _registeredPlayers.Count) return _registeredPlayers[index];
        
        UnityEngine.Debug.Log($"PlayerRegistry Warning: Index '{index}' was out of range, registered count is between 0 and {_registeredPlayers.Count}.");
        return null;
    }
    
    public static IPlayer GetPlayer(string name)
    {
        foreach (IPlayer player in _registeredPlayers)
        {
            if (player.Name == name) return player;
        }

        UnityEngine.Debug.Log($"PlayerRegistry Warning: Name '{name}' does not match anything within the registry.");
        return null;
    }
}