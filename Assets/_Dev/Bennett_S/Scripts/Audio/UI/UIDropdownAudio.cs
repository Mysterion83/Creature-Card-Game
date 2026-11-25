using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Component for playing hover and open audio for TMP_Dropdowns
/// </summary>
[RequireComponent(typeof(TMP_Dropdown))]
public class UIDropdownAudio : MonoBehaviour, 
    IPointerEnterHandler, IPointerDownHandler, 
    ISelectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _openSfx;

    private TMP_Dropdown _dropdown;

    private void Awake() => _dropdown = GetComponent<TMP_Dropdown>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_dropdown.interactable || eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(_hoverSfx);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_dropdown.interactable || eventData is PointerEventData) return;
        PlaySfx(_hoverSfx);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_dropdown.interactable) return;
        PlaySfx(_openSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!_dropdown.interactable) return;
        PlaySfx(_openSfx);
    }

    private static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}