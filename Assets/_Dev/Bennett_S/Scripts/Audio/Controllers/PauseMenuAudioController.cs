using UnityEngine;

/// <summary>
/// Switches between two AudioMixerSnapshots on pause and resume
/// </summary>
public class PauseMenuAudioController : MonoBehaviour
{
    [Header("Snapshot Names")]
    [SerializeField] private string _pausedSnapshot = "Paused";
    [SerializeField] private string _unpausedSnapshot = "Default";

    [Header("Transition Override")]
    [Tooltip("Leave negative to use AudioManager's defaults.")]
    [SerializeField] private float _transitionTime = -1f;

    private void OnEnable()
    {
        AudioManager.Instance?.TransitionToSnapshot(_pausedSnapshot, _transitionTime);
    }

    private void OnDisable()
    {
        AudioManager.Instance?.TransitionToSnapshot(_unpausedSnapshot, _transitionTime);
    }
}