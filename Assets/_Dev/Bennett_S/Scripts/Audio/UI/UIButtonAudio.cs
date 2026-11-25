using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Component for playing audio for buttons
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonAudio : UISelectableAudioBase, IPointerDownHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _clickSfx;

    protected override SFXDefinition HoverSfx => _hoverSfx;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_clickSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_clickSfx);
    }
}