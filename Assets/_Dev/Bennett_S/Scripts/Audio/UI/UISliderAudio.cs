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
    IPointerEnterHandler, IPointerDownHandler, 
    IPointerUpHandler, ISelectHandler, 
    IDeselectHandler, ISubmitHandler
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _hoverSfx;
    [SerializeField] private SFXDefinition _clickSfx;
    [SerializeField] private SFXDefinition _tickSfx;
    [Tooltip("Optional sound played when releasing the slider handle.")]
    [SerializeField] private SFXDefinition _releaseSfx;

    [Header("Slider Step Settings")]
    [Tooltip("Normalized distance (0 to 1) the slider must move before playing the next tick.\n\n 0 = tick on every change.")]
    [SerializeField, Range(0f, 0.5f)] private float _normalisedStepInterval = 0.05f;
    
    [Header("Pitch Scaling")]
    [Tooltip("Scales the tick pitch based on the slider's normalized value (0 to 1).")]
    [SerializeField] private bool _scalePitchWithValue = true;
    [SerializeField] private Vector2 _pitchRange = new(0.85f, 1.2f);

    private Slider _slider;
    private bool _changedWhileSelected;
    private float _lastTickNormalisedValue;
    
     private void Awake()
    {
        _slider = GetComponent<Slider>();
        _lastTickNormalisedValue = _slider.normalizedValue;
        _slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnEnable()
    {
        if (_slider) _lastTickNormalisedValue = _slider.normalizedValue;
    }

    private void OnDestroy()
    {
        if (_slider) _slider.onValueChanged.RemoveListener(OnValueChanged);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play if the mouse isn't or barely moving
        if (!_slider.interactable || eventData.delta.sqrMagnitude <= 0.01f) return;
        PlaySfx(_hoverSfx);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_slider.interactable || eventData is PointerEventData) return;
        _changedWhileSelected = false;
        PlaySfx(_hoverSfx);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!_slider.interactable || !_changedWhileSelected) return;
        _changedWhileSelected = false;
        PlaySfx(_releaseSfx);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_slider.interactable) return;
        _lastTickNormalisedValue = _slider.normalizedValue;
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
        _changedWhileSelected = false;
        PlaySfx(_releaseSfx);
    }

    private void OnValueChanged(float value)
    {
        if (!_slider.interactable) return;
        _changedWhileSelected = true;

        if (!_tickSfx || !AudioManager.Instance) return;

        float sliderCurrentNormalised = _slider.normalizedValue;
        bool reachedASide = Mathf.Approximately(sliderCurrentNormalised, 0f) || Mathf.Approximately(sliderCurrentNormalised, 1f);

        // Skip the tick until the slider has moved a step interval (unless hitting 0% or 100%)
        if (_normalisedStepInterval > 0f && !reachedASide)
        {
            if (Mathf.Abs(sliderCurrentNormalised - _lastTickNormalisedValue) < _normalisedStepInterval) return;
        }

        _lastTickNormalisedValue = sliderCurrentNormalised;

        // Store the emitter and lerp the pitch over its full length as specified in _pitchRange (x to y)
        SFXEmitter emitter = AudioManager.Instance.SFX.Play(_tickSfx);
        if (emitter && _scalePitchWithValue)
        {
            emitter.SetPitch(Mathf.Lerp(_pitchRange.x, _pitchRange.y, sliderCurrentNormalised));
        }
    }

    private static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}
