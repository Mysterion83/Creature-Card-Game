using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
Uses Interfaces from EventSystems to integrate selection functions. Should work for controller input as well through
the ISelectHandler and ISubmitHandler interfaces, the pointer ones are just the mouse equivalent events.
*/

/// <summary>
/// Component for playing audio for buttons
/// </summary>
[RequireComponent(typeof(Selectable))]
public class UIButtonAudio : MonoBehaviour, 
    IPointerEnterHandler, IPointerDownHandler, 
    ISelectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _clickSfx;
    
    private Selectable _selectable;

    private void Awake() => _selectable = GetComponent<Selectable>();
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_selectable.interactable || eventData.delta.sqrMagnitude <= 0.01f) return;
        Play(_hoverSfx);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_selectable.interactable) return;
        Play(_clickSfx);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_selectable.interactable || eventData is PointerEventData) return;
        Play(_hoverSfx);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!_selectable.interactable) return;
        Play(_clickSfx);
    }
    
    private static void Play(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}
