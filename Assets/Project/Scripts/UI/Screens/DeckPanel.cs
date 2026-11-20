using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckPanel : MonoBehaviour
{
    public const int MaxDeckSize = 8;

    [SerializeField] CardSlotView slotPrefab;
    [SerializeField] Transform list;          
    [SerializeField] TMP_Text countText;      
    readonly List<string> working = new();    // edits stay here until Save Deck
    public event Action OnDeckEdited;

    /*
    Controls the Current Deck panel. It copies the saved deck into a working list
    so you can remove cards or clear the deck without changing the save.        
    */

    void OnEnable()
    {
        working.Clear();
        var pm = ProfileManager.Instance;
        if (pm != null && pm.Profile != null) working.AddRange(pm.Profile.Deck);
        Refresh();
    }

    public int CopiesInDeck(string id) => working.FindAll(x => x == id).Count;

    public bool TryAdd(string id)
    {
        var pm = ProfileManager.Instance;
        if (working.Count >= MaxDeckSize || CopiesInDeck(id) >= pm.OwnedCount(id)) return false;
        working.Add(id);
        Refresh();
        return true;
    }

    public void Remove(string id)
    {
        if (working.Remove(id)) Refresh();
    }

    // Buttons
    public void ClearDeck()
    {
        working.Clear();
        Refresh();
    }

    public void SaveDeck() => ProfileManager.Instance.SaveDeck(working);

    void Refresh()
    {
        foreach (Transform child in list) Destroy(child.gameObject);

        var pm = ProfileManager.Instance;
        if (pm != null)
        {
            foreach (string id in working)
            {
                var entry = pm.Catalog.Get(id);
                if (entry == null) continue;
                CardSlotView slot = Instantiate(slotPrefab, list);
                slot.Show(entry, -1, "-");
                slot.ActionClicked += s => Remove(s.Id);
            }
        }

        if (countText != null) countText.text = $"{working.Count}/{MaxDeckSize}";
        OnDeckEdited?.Invoke();
    }
}
