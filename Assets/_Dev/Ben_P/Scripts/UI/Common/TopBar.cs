using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : MonoBehaviour
{
    [Header("This bar's own parts")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text coinText;
    [SerializeField] Image muteIcon;
    [SerializeField] Sprite soundOnSprite;
    [SerializeField] Sprite soundOffSprite;
    [SerializeField] GameObject backButton;   
    [SerializeField] GameObject tabsGroup;     
    [SerializeField] GameObject coinGroup;     
    [SerializeField] GameObject titleGroup;    

    [Header("Change these on each copy")]
    [SerializeField] string title = "Level Selector";
    [SerializeField] bool showBackButton = true;    
    [SerializeField] bool showTabs = false;    

    [Header("Things in HUB (drag them in)")]
    [SerializeField] MenuController menu;   
    [SerializeField] Popup settingsPopup;
    [SerializeField] Popup exitPopup;
    [SerializeField] GameObject deckShopPanel; 

    const string MuteKey = "Muted";
    static bool muted;
    bool refreshPending;

    void Awake()
    {
        if (menu == null) menu = FindAnyObjectByType<MenuController>(FindObjectsInactive.Include);
    }

    void Start()
    {
        SetMuted(PlayerPrefs.GetInt(MuteKey, 0) == 1);
    }

    void OnEnable()
    {
        if (titleText != null) titleText.text = title;

        ProfileManager.OnProfileChanged += Refresh;
        refreshPending = true; 
        RefreshMuteIcon();
    }

    void OnDisable()
    {
        ProfileManager.OnProfileChanged -= Refresh;
    }

    void LateUpdate()
    {
        if (!refreshPending) return;
        refreshPending = false;
        Refresh();
    }
    public void Configure(string newTitle, bool back, bool tabs)
    {
        title = newTitle;
        showBackButton = back;
        showTabs = tabs;
        if (titleText != null) titleText.text = title;
        refreshPending = true;
    }

    // Buttons
    public void OnBack() { if (menu != null) menu.Back(); }
    public void OnSettings() { if (settingsPopup != null) settingsPopup.Show(); }
    public void OnExit() { if (exitPopup != null) exitPopup.Show(); }
    public void OnMute() => SetMuted(!muted);

    public void OnDeck()
    {
        if (menu != null && deckShopPanel != null) menu.Open(deckShopPanel);
    }

    // Display 
    
    void Refresh()
    {
        var pm = ProfileManager.Instance;
        bool inGame = pm != null && pm.Profile != null;

        if (backButton != null) backButton.SetActive(showBackButton);                
        if (tabsGroup != null) tabsGroup.SetActive(inGame && showTabs);
        bool showTitle = !showTabs && !string.IsNullOrEmpty(title);                  
        if (titleGroup != null) titleGroup.SetActive(showTitle);
        else if (titleText != null) titleText.gameObject.SetActive(showTitle);
        if (coinGroup != null) coinGroup.SetActive(inGame);

        RefreshCoins();
    }

    void RefreshCoins()
    {
        if (coinText == null) return;
        var profile = ProfileManager.Instance != null ? ProfileManager.Instance.Profile : null;
        coinText.text = profile != null ? profile.Coins.ToString("N0") : "0";
    }

    void SetMuted(bool value)
    {
        muted = value;
        AudioListener.volume = muted ? 0f : 1f;
        PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
        RefreshMuteIcon();
    }

    void RefreshMuteIcon()
    {
        if (muteIcon != null) muteIcon.sprite = muted ? soundOffSprite : soundOnSprite;
    }
}