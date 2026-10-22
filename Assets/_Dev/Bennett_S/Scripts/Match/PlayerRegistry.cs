using System.Collections.Generic;

/// <summary>
/// Static class for player initialisation
/// </summary>
public static class PlayerRegistry
{
    // Network Lookup
    private static readonly Dictionary<string, IPlayer> _playerIDLookup = new();
    
    // Name Lookup
    private static readonly Dictionary<string, IPlayer> _playerNameLookup = new();

    // Master list
    private static readonly List<IPlayer> _masterList = new();
    public static IReadOnlyList<IPlayer> MasterList => _masterList;

    public static int PlayerCount => _masterList.Count;

    /// <summary>
    /// Registers a player into the registry.
    /// </summary>
    /// <param name="id">Unique player Network ID</param>
    /// <param name="player">The player to register</param>
    public static void RegisterPlayer(string id, IPlayer player)
    {
        if (player?.Data == null) return;
        if (!_playerIDLookup.TryAdd(id, player)) return;

        _masterList.Add(player);

        if (!string.IsNullOrEmpty(player.Data.Name))
        {
            // Protect against duplicate names breaking the dictionary
            _playerNameLookup[player.Data.Name] = player;
        }
    }

    /// <summary>
    /// Unregister a player from the registry. 
    /// </summary>
    /// <param name="id">The target player ID</param>
    public static void UnregisterPlayer(string id)
    {
        if (!_playerIDLookup.Remove(id, out var player)) return;

        _masterList.Remove(player);

        if (player.Data != null && !string.IsNullOrEmpty(player.Data.Name))
        {
            _playerNameLookup.Remove(player.Data.Name);
        }
    }

    public static IPlayer GetPlayerById(string id)
    {
        _playerIDLookup.TryGetValue(id, out var player);
        return player;
    }

    public static IPlayer GetPlayerByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        _playerNameLookup.TryGetValue(name, out var player);
        return player;
    }
    
    /// <summary>
    /// Check to see if a player is AI
    /// </summary>
    public static bool IsPlayerAI(string id)
    {
        var player = GetPlayerById(id);
        return player?.Data != null && player.Data.IsAI;
    }
    
    /// <summary>
    /// Clears the registry completely; mainly used for resetting between scenes.
    /// </summary>
    public static void Clear()
    {
        _playerIDLookup.Clear();
        _playerNameLookup.Clear();
        _masterList.Clear();
    }
}