using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component for playing audio for any visual element 
/// </summary>
public class UIElementAudio : UISelectableAudioBase, IPointerExitHandler, IDeselectHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _enterSfx;
    [SerializeField] private SFXDefinition _exitSfx;

    [Tooltip("Ignore hover if the mouse cursor is stationary.")]
    [SerializeField] private bool _requireCursorMovement = true;

    protected override SFXDefinition HoverSfx => _enterSfx;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanPlayAudio()) return;
        if (_requireCursorMovement && eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(_enterSfx);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_exitSfx);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!CanPlayAudio() || eventData is PointerEventData) return;
        PlaySfx(_exitSfx);
    }
}