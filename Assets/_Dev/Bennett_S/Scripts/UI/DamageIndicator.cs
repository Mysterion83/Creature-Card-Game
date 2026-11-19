using System.Collections;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private TMP_Text _damageText;
    [SerializeField, Min(0)] private int _normalFontSize = 38;
    [SerializeField, Min(0)] private int _advantageFontSize = 40;
    [SerializeField, Min(0)] private int _disadvantageFontSize = 36;
    [SerializeField] private float _distanceScaleMultiplier = 0.002f;
    
    [Header("Gradient Presets")]
    [SerializeField] private TMP_ColorGradient _normalGradient;
    [SerializeField] private TMP_ColorGradient _advantageGradient;
    [SerializeField] private TMP_ColorGradient _disadvantageGradient;
    
    [Header("Spawning Settings")]
    [SerializeField] private Vector3 _spawnOffset = new(0f, 2f, 0f);
    [SerializeField] private float _duration = 2f;

    private Transform _cameraTransform;
    private Coroutine _routine;
    
    private void Awake()
    {
        if (Camera.main) _cameraTransform = Camera.main.transform;
    }
    
    public void Display(DamageType damageType, float damage)
    {
        transform.position += _spawnOffset;
        // Displays to 1.d.p, using F1 would make integers have a .0 at the end, this does not
        _damageText.text = damage.ToString("0.#");
        
        switch (damageType)
        {
            case DamageType.Normal:
                _damageText.fontSize = _normalFontSize;
                _damageText.colorGradientPreset = _normalGradient;
                break;
            case DamageType.Advantage:
                _damageText.fontSize = _advantageFontSize;
                _damageText.colorGradientPreset = _advantageGradient;
                break;
            case DamageType.Disadvantage:
                _damageText.fontSize = _disadvantageFontSize;
                _damageText.colorGradientPreset = _disadvantageGradient;
                break;
        }

        StartCoroutine(FadeOutRoutine());
    }
    
    private void LateUpdate()
    {
        if (!_cameraTransform) return;
        
        transform.LookAt(transform.position + _cameraTransform.forward);
        
        float distance = Vector3.Distance(transform.position, _cameraTransform.position);
        transform.localScale = Vector3.one * (distance * _distanceScaleMultiplier);
    }
    
    private IEnumerator FadeOutRoutine()
    {
        for (float elapsed = 0f; elapsed < _duration; elapsed += Time.deltaTime)
        {
            _damageText.alpha = Mathf.Lerp(1f, 0f, elapsed / _duration);
            yield return null;
        }

        Destroy(gameObject);
    }
}