using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Component for playing hover and on/off audio for UI Toggles
/// </summary>
[RequireComponent(typeof(Toggle))]
public class UIToggleAudio : UISelectableAudioBase, 
    IPointerClickHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _toggleOnSfx;
    [SerializeField] private SFXDefinition _toggleOffSfx;

    private Toggle _toggle;

    protected override SFXDefinition HoverSfx => _hoverSfx;

    protected override void Awake()
    {
        base.Awake();
        _toggle = GetComponent<Toggle>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_toggle.isOn ? _toggleOnSfx : _toggleOffSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (CanPlayAudio()) PlaySfx(_toggle.isOn ? _toggleOnSfx : _toggleOffSfx);
    }
}