using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotView : MonoBehaviour
{
    [SerializeField] int slot;
    [SerializeField] Button button;
    [SerializeField] TMP_Text slotTitle;
    [SerializeField] TMP_Text progressText;
    [SerializeField] TMP_Text savedAtText;

    public int Slot => slot;
    public bool HasSave { get; private set; }
    public event Action<SaveSlotView> Clicked;

    void Awake() => button.onClick.AddListener(() => Clicked?.Invoke(this));

    // Reads the file without making it the active game
    public void Refresh(bool emptySlotsClickable)
    {
        PlayerProfile save = SaveSystem.Load(slot);
        HasSave = save != null;

        slotTitle.text = $"Save Slot {slot + 1}";
        progressText.text = HasSave ? save.ProgressLabel : "Empty";
        savedAtText.text = HasSave ? save.SavedAt : "";
        button.interactable = HasSave || emptySlotsClickable;
    }
}