using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/*
 * BattleDeckLoader swaps the players battle deck for the deck saved in their
 * profile. ProfileManager calls it when a scene loads after Awake but before any
 * Start so PlayerDeck picks its first card from the saved deck instead of the one
 * typed into the scene
 */

public static class BattleDeckLoader
{
    public static void Apply(PlayerProfile profile, CreatureCatalog catalog, LevelData level)
    {
        ApplyPlayerDeck(profile, catalog);
        ApplyEnemyDeck(level);
    }

    private static void ApplyPlayerDeck(PlayerProfile profile, CreatureCatalog catalog)
    {
        if (profile == null || catalog == null) return;

        PlayerDeck deck = GetLocalPlayerDeck();
        if (deck == null) return;

        var cards = new List<CreatureData>();
        foreach (string id in profile.Deck)
        {
            CreatureCatalog.Entry entry = catalog.Get(id);
            if (entry == null)
            {
                Debug.LogWarning($"BattleDeckLoader: '{id}' isn't in the Creature Catalog, skipping it.");
                continue;
            }
            if (entry.Creature.CreaturePrefab == null)
            {
                Debug.LogWarning($"BattleDeckLoader: {entry.Creature.name} has no Creature Prefab so it can't be summoned, skipping it.");
                continue;
            }
            cards.Add(entry.Creature);
        }

        if (cards.Count == 0)
        {
            Debug.LogWarning("BattleDeckLoader: saved deck had no usable cards, keeping the scene's deck.");
            return;
        }

        SetDeck(deck, cards);
        Debug.Log($"BattleDeckLoader: loaded {cards.Count} cards from the saved deck.");
    }

    private static void ApplyEnemyDeck(LevelData level)
    {
        if (level == null || level.EnemyCreatures == null || level.EnemyCreatures.Count == 0) return;

        PlayerDeck deck = GetAIPlayerDeck();
        if (deck == null) return;

        var cards = new List<CreatureData>();
        foreach (CreatureData creature in level.EnemyCreatures)
        {
            if (creature == null) continue;
            if (creature.CreaturePrefab == null)
            {
                Debug.LogWarning($"BattleDeckLoader: {creature.name} has no Creature Prefab so the AI can't summon it, skipping it.");
                continue;
            }
            cards.Add(creature);
        }

        if (cards.Count == 0) return;

        SetDeck(deck, cards);
        Debug.Log($"BattleDeckLoader: loaded {cards.Count} enemy cards from {level.name}.");
    }

    private static PlayerDeck GetAIPlayerDeck()
    {
        foreach (Player player in PlayerRegistry.MasterList)
            if (player != null && player.Data.IsAI && player.TryGetComponent(out PlayerDeck deck)) return deck;
        return null;
    }

    private static PlayerDeck GetLocalPlayerDeck()
    {
        foreach (Player player in PlayerRegistry.MasterList)
            if (player != null && !player.Data.IsAI && player.TryGetComponent(out PlayerDeck deck)) return deck;
        return null;
    }

    private static void SetDeck(PlayerDeck deck, List<CreatureData> cards)
    {
        FieldInfo field = typeof(PlayerDeck).GetField("_creaturesToPlace", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogError("BattleDeckLoader: PlayerDeck no longer has _creaturesToPlace, the deck wasn't loaded.");
            return;
        }
        field.SetValue(deck, cards);
    }
}
