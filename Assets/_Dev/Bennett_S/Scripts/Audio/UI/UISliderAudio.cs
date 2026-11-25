using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Component for playing audio for sliders
/// </summary>
[RequireComponent(typeof(Slider))]
public class UISliderAudio : UISelectableAudioBase, 
    IPointerDownHandler, IPointerUpHandler, 
    IDeselectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _clickSfx;
    [SerializeField] private SFXDefinition _releaseSfx;

    protected override SFXDefinition HoverSfx => _hoverSfx;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_clickSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_clickSfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_releaseSfx);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!CanPlayAudio() || eventData is PointerEventData) return;
        PlaySfx(_releaseSfx);
    }
}