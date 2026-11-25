using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component for playing hover and open audio for TMP_Dropdowns
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropdownAudio : UISelectableAudioBase, IPointerDownHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _openSfx;

    protected override SFXDefinition HoverSfx => _hoverSfx;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_openSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_openSfx);
    }
}