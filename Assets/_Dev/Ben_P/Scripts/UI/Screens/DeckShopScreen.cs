using UnityEngine;
using UnityEngine.UI;

public class DeckShopScreen : MonoBehaviour
{
    [Header("Left hand panels")]
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject deckManagerPanel;

    [Header("Tab buttons")]
    [SerializeField] Image shopTab;
    [SerializeField] Image deckManagerTab;
    [SerializeField] Color activeColour = new Color(0.25f, 0.25f, 0.25f);
    [SerializeField] Color inactiveColour = Color.white;

    [SerializeField] bool openOnShop = false;   // which tab shows first

    void OnEnable()
    {
        if (openOnShop) ShowShop();
        else ShowDeckManager();
    }

    // Tab buttons
    public void ShowShop() => Show(true);
    public void ShowDeckManager() => Show(false);

    void Show(bool shop)
    {
        shopPanel.SetActive(shop);
        deckManagerPanel.SetActive(!shop);

        if (shopTab != null) shopTab.color = shop ? activeColour : inactiveColour;
        if (deckManagerTab != null) deckManagerTab.color = shop ? inactiveColour : activeColour;
    }
}