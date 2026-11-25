using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
Uses Interfaces from EventSystems to integrate selection functions. Should work for controller input as well through
the ISelectHandler and ISubmitHandler interfaces, the pointer ones are just the mouse equivalent events.
*/

/// <summary>
/// Component for playing audio for sliders
/// </summary>
[RequireComponent(typeof(Slider))]
public class UISliderAudio : MonoBehaviour, 
    IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler, 
    ISelectHandler, IDeselectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _clickSfx;
    [Tooltip("Optional sound played when releasing the slider handle.")]
    [SerializeField] private SFXDefinition _releaseSfx;

    private Slider _slider;
    
    private void Awake() => _slider = GetComponent<Slider>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play if the mouse isn't or barely moving
        if (!_slider.interactable || eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(_hoverSfx);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_slider.interactable || eventData is PointerEventData) return;
        PlaySfx(_hoverSfx);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!_slider.interactable || eventData is PointerEventData) return;
        PlaySfx(_releaseSfx);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_slider.interactable) return;
        PlaySfx(_clickSfx);
    }
    
    public void OnSubmit(BaseEventData eventData)
    {
        if (!_slider.interactable) return;
        PlaySfx(_clickSfx);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_slider.interactable) return;
        PlaySfx(_releaseSfx);
    }
    
    private static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}
