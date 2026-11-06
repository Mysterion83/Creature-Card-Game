using System;                       
using System.Collections.Generic;  
using UnityEngine;
using UnityEngine.SceneManagement; 

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance { get; private set; }
    public static event Action OnProfileChanged; 

    [SerializeField] CreatureCatalog catalog;
    [SerializeField] int shopSize = 5;
    [SerializeField] int startingCoins = 500;

    public CreatureCatalog Catalog => catalog;
    public PlayerProfile Profile { get; private set; }
    public int ActiveSlot { get; private set; } = -1;
    public LevelData CurrentLevel { get; private set; }
    public bool ReturnToLevelSelect { get; set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Save slots
    public void NewGame(int slot, StarterDeck starter)
    {
        ActiveSlot = slot;
        Profile = new PlayerProfile
        {
            Coins = startingCoins,
            StarterElement = starter.Element,
            ProgressLabel = "New game"
        };
        var cards = starter.CardIds();
        Profile.Collection.AddRange(cards);
        Profile.Deck.AddRange(cards);
        RerollShop();
        Save();
        Changed();
    }

    public bool LoadGame(int slot)
    {
        PlayerProfile loaded = SaveSystem.Load(slot);
        if (loaded == null) return false;
        Profile = loaded;
        ActiveSlot = slot;
        Changed();
        return true;
    }

    public void Save()
    {
        if (Profile == null || ActiveSlot < 0)
        {
            Debug.LogWarning("ProfileManager: no game running, nothing to save.");
            return;
        }
        SaveSystem.Save(ActiveSlot, Profile);
    }
    public void CloseGame()
    {
        Profile = null;
        ActiveSlot = -1;
        CurrentLevel = null;
        Changed();
    }


    // Coins
    public void AddCoins(int amount)
    {
        if (Profile == null || amount <= 0) return;
        Profile.Coins += amount;
        Changed();
    }

    public bool TrySpend(int amount)
    {
        if (Profile == null || amount < 0 || Profile.Coins < amount) return false;
        Profile.Coins -= amount;
        Changed();
        return true;
    }

    // Levels
    public bool IsCompleted(LevelData level) =>
        Profile != null && level != null && Profile.CompletedLevels.Contains(level.Id);

    public bool IsUnlocked(LevelData level) =>
        level != null && (level.RequiredLevel == null || IsCompleted(level.RequiredLevel));

    public void StartLevel(LevelData level)
    {
        if (!IsUnlocked(level)) return;
        CurrentLevel = level;
        SceneManager.LoadScene(level.SceneName);
    }

    public void CompleteCurrentLevel()
    {
        if (Profile == null || CurrentLevel == null) return;

        if (!Profile.CompletedLevels.Contains(CurrentLevel.Id))
            Profile.CompletedLevels.Add(CurrentLevel.Id);

        Profile.Coins += CurrentLevel.CoinReward;
        Profile.ProgressLabel = $"Level {CurrentLevel.Number} - {ElementLabel.For(CurrentLevel.Wing)} Wing";
        RerollShop();
        Save();
        Changed();
    }

    // Shop
    public void RerollShop()
    {
        Profile.ShopStock.Clear();

        var pool = new List<string>();
        foreach (var entry in catalog.Entries)
            if (entry.Creature != null) pool.Add(entry.Id);

        for (int i = 0; i < shopSize && pool.Count > 0; i++)
        {
            int pick = UnityEngine.Random.Range(0, pool.Count);   
            Profile.ShopStock.Add(pool[pick]);
            pool.RemoveAt(pick);
        }
    }

    // Collection and deck
    public int OwnedCount(string id) =>
        Profile == null ? 0 : Profile.Collection.FindAll(x => x == id).Count;

    public void SaveDeck(List<string> deck)
    {
        Profile.Deck = new List<string>(deck);
        Save();
        Changed();
    }

    // Testing only
    public void DebugSetCurrentLevel(LevelData level) => CurrentLevel = level;

    void Changed() => OnProfileChanged?.Invoke();
}