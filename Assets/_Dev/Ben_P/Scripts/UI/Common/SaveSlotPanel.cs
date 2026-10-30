using UnityEngine;

public class SaveSlotPanel : MonoBehaviour
{
    enum Mode { NewGame, Load }

    [SerializeField] MenuController menu;
    [SerializeField] SaveSlotView[] slots;
    [SerializeField] GameObject overwriteConfirm;
    [SerializeField] GameObject starterDeckPanel;
    [SerializeField] GameObject levelSelectPanel;

    // The Starter Deck screen reads this to know which slot to create the game in
    public static int ChosenSlot { get; private set; } = -1;

    Mode mode;
    int pendingSlot = -1;

    void Awake()
    {
        foreach (var view in slots) view.Clicked += OnSlotClicked;
    }

    // Play panel buttons
    public void OpenForNewGame() { mode = Mode.NewGame; menu.Open(gameObject); }
    public void OpenForLoad() { mode = Mode.Load; menu.Open(gameObject); }

    void OnEnable()
    {        
        foreach (var view in slots) view.Refresh(mode == Mode.NewGame);
    }

    void OnDisable()
    {
        if (overwriteConfirm != null) overwriteConfirm.SetActive(false);
    }

    void OnSlotClicked(SaveSlotView view)
    {
        if (mode == Mode.Load)
        {
            if (ProfileManager.Instance.LoadGame(view.Slot))
                menu.OpenFresh(levelSelectPanel);
            return;
        }

        pendingSlot = view.Slot;
        if (view.HasSave) overwriteConfirm.SetActive(true);
        else ConfirmNewGame();
    }

    // Overwrite window buttons
    public void ConfirmNewGame()
    {
        overwriteConfirm.SetActive(false);
        ChosenSlot = pendingSlot;
        menu.Open(starterDeckPanel);
    }

    public void CancelOverwrite() => overwriteConfirm.SetActive(false);
}