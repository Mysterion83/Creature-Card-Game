using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
Uses Interfaces from EventSystems to integrate selection functions. Should work for controller input as well through
the ISelectHandler and ISubmitHandler interfaces, the pointer ones are just the mouse equivalent events.
*/

/// <summary>
/// Base class for all UIAudio classes
/// </summary>
public abstract class UISelectableAudioBase : MonoBehaviour,
    IPointerEnterHandler, ISelectHandler
{
    [Header("Settings")]
    [Tooltip("If a Selectable component is attached, do not play audio while it is inactive.")]
    [SerializeField] private bool _muteOnInactive = true;

    private Selectable _selectable;
    
    protected abstract SFXDefinition HoverSfx { get; }

    protected virtual void Awake()
    {
        TryGetComponent(out _selectable);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanPlayAudio() || eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(HoverSfx);
    }

    public virtual void OnSelect(BaseEventData eventData)
    {
        if (!CanPlayAudio() || eventData is PointerEventData) return;
        PlaySfx(HoverSfx);
    }

    protected bool CanPlayAudio()
    {
        if (!_selectable) return true;
        return !_muteOnInactive || _selectable.interactable;
    }

    protected static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}