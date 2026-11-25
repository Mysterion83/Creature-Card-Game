using UnityEngine;

/// <summary>
/// Component for playing audio for UI Popups when enabled and disabled
/// </summary>
public class UIPopupAudio : MonoBehaviour
{
    [Header("SFX Definitions")]
    [SerializeField] private SFXDefinition _openSfx;
    [SerializeField] private SFXDefinition _closeSfx;
    
    private void OnEnable()
    {
        Play(_openSfx);
    }
    
    private void OnDisable()
    {
        Play(_closeSfx);
    }
    
    private static void Play(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}
