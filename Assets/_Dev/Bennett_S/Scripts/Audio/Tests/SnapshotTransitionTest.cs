using UnityEngine;
using UnityEngine.Audio;

public class SnapshotTransitionTest : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("The project AudioMixer")]
    [SerializeField] private AudioMixer _mixer;
    [Space]
    [Tooltip("A snapshot to test")]
    [SerializeField] private string _snapshotAName;
    [Tooltip("A snapshot to test against")]
    [SerializeField] private string _snapshotBName;
    [Tooltip("The transition time, leave blank for default transition time")]
    [SerializeField] private float _customTransitionTime;
    
    public void TransitionToA()
    {
        float transitionTime = _customTransitionTime > 0 ? _customTransitionTime : 0;
        AudioManager.Instance.TransitionToSnapshot(_snapshotAName, transitionTime);
    }

    public void TransitionToB()
    {
        float transitionTime = _customTransitionTime > 0 ? _customTransitionTime : 0;
        AudioManager.Instance.TransitionToSnapshot(_snapshotBName, transitionTime);
    }
}
