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

    [Header("Shop unlock")]
    [SerializeField] LevelData unlockLevel;

    Button shopTabButton;

    void Awake()
    {
        if (shopTab != null) shopTabButton = shopTab.GetComponent<Button>();
    }

    void OnEnable()
    {
        if (shopTabButton != null) shopTabButton.interactable = ShopUnlocked();

        if (openOnShop && ShopUnlocked()) ShowShop();
        else ShowDeckManager();
    }

    bool ShopUnlocked()
    {
        var pm = ProfileManager.Instance;
        return unlockLevel == null || (pm != null && pm.IsCompleted(unlockLevel));
    }

    // Tab buttons
    public void ShowShop()
    {
        if (ShopUnlocked()) Show(true);
    }
    public void ShowDeckManager() => Show(false);

    void Show(bool shop)
    {
        shopPanel.SetActive(shop);
        deckManagerPanel.SetActive(!shop);

        if (shopTab != null) shopTab.color = shop ? activeColour : inactiveColour;
        if (deckManagerTab != null) deckManagerTab.color = shop ? inactiveColour : activeColour;
    }
}