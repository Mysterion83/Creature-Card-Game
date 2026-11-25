using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Component for playing hover and on/off audio for UI Toggles
/// </summary>
[RequireComponent(typeof(Toggle))]
public class UIToggleAudio : MonoBehaviour, 
    IPointerEnterHandler, IPointerClickHandler, 
    ISelectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _toggleOnSfx;
    [SerializeField] private SFXDefinition _toggleOffSfx;

    private Toggle _toggle;

    private void Awake() => _toggle = GetComponent<Toggle>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_toggle.interactable || eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(_hoverSfx);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_toggle.interactable || eventData is PointerEventData) return;
        PlaySfx(_hoverSfx);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_toggle.interactable) return;
        PlaySfx(_toggle.isOn ? _toggleOnSfx : _toggleOffSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!_toggle.interactable) return;
        PlaySfx(_toggle.isOn ? _toggleOnSfx : _toggleOffSfx);
    }

    private static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}