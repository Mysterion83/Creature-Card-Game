using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarterDeckScreen : MonoBehaviour
{
    [SerializeField] MenuController menu;
    [SerializeField] GameObject levelSelectPanel;
    [SerializeField] Button confirmButton;
    [SerializeField] TMP_Text confirmLabel;

    StarterDeck selected;

    void OnEnable()
    {
        selected = null;
        confirmButton.interactable = false;
        confirmLabel.text = "Choose a deck";
    }

    // element buttons call this passing their deck asset in the On Click slot
    public void Select(StarterDeck deck)
    {
        selected = deck;
        confirmButton.interactable = true;
        confirmLabel.text = $"Confirm {ElementLabel.For(deck.Element)} Deck";
    }

    public void Confirm()
    {
        if (selected == null) return;
        if (SaveSlotPanel.ChosenSlot < 0)
        {
            Debug.LogWarning("StarterDeckScreen: no save slot chosen. Start from Play › New Game.");
            return;
        }
        ProfileManager.Instance.NewGame(SaveSlotPanel.ChosenSlot, selected);
        menu.OpenFresh(levelSelectPanel);  
    }
}