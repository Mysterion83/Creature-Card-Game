using System.Collections.Generic;
using UnityEngine;

public class CollectionPanel : MonoBehaviour
{
    [SerializeField] CardSlotView slotPrefab;
    [SerializeField] Transform grid;          
    [SerializeField] DeckPanel deckPanel;

    /*
    Controls the card collection panel. It shows one card for each creature you own,
    with a count of the spare copies that aren't already in the deck
    Pressing + adds a copy to the Current Deck, and the card greys out when no spares are left
    */

    void OnEnable()
    {
        deckPanel.OnDeckEdited += Refresh;
        Refresh();
    }

    void OnDisable() => deckPanel.OnDeckEdited -= Refresh;

    void Refresh()
    {
        foreach (Transform child in grid) Destroy(child.gameObject);

        var pm = ProfileManager.Instance;
        if (pm == null || pm.Profile == null) return;

        var shown = new List<string>();
        foreach (string id in pm.Profile.Collection)
        {
            if (shown.Contains(id)) continue;   // one card per creature, duplicates shown as "x2"
            shown.Add(id);

            var entry = pm.Catalog.Get(id);
            if (entry == null) continue;

            int spare = pm.OwnedCount(id) - deckPanel.CopiesInDeck(id);   // copies not already in the deck
            CardSlotView slot = Instantiate(slotPrefab, grid);
            slot.Show(entry, -1, "+", spare);
            slot.SetInteractable(spare > 0);
            slot.ActionClicked += s => deckPanel.TryAdd(s.Id);
        }
    }
}
