using System.Collections;
using System.Text;
using UnityEngine;

public class SFXTest : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("The SFX Definition to test")]
    [SerializeField] private SFXDefinition _testSound;
    
    [Tooltip("How many times per second the sound should play")]
    [SerializeField, Min(0.1f)] private float _timesPerSecond = 5f;

    [Tooltip("How many seconds the test should run")]
    [SerializeField, Min(0.1f)] private float _testDuration = 5f;

    [Tooltip("Check to play at this object's position, uncheck to play globally")]
    [SerializeField] private bool _usePosition = true;

    private Coroutine _playSoundRoutine;
    
    private void Start()
    {
        if (!_testSound)
        {
            Debug.LogWarning("[SFXTest] No test sound assigned!", this);
        }
    }

    public void StartTest()
    {
        EndTest();
        _playSoundRoutine = StartCoroutine(PlaySoundForDurationRoutine());
    }

    public void EndTest()
    {
        if (_playSoundRoutine == null) return;
        
        StopCoroutine(_playSoundRoutine);
        Debug.Log("[SFXTest] Test Ended Early");
    }
    
    private IEnumerator PlaySoundForDurationRoutine()
    {
        float delay = 1f / _timesPerSecond;
        var waitInterval = new WaitForSeconds(delay);
        int soundsTriggeredCount = 0;
        
        // Use this as coroutines mess up time
        float startTime = Time.realtimeSinceStartup;

        while (Time.realtimeSinceStartup - startTime < _testDuration)
        {
            var emitter = _usePosition 
                ? AudioManager.Instance.SFX.Play(_testSound, transform.position) 
                : AudioManager.Instance.SFX.Play(_testSound);
    
            if (emitter) soundsTriggeredCount++;

            yield return waitInterval;
        }
        
        float actualDuration = Time.realtimeSinceStartup - startTime;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[AudioTestScript] Test Report:");
        sb.AppendLine($"  - Target Duration: {_testDuration:F2}s");
        sb.AppendLine($"  - Actual Duration: {actualDuration:F2}s");
        sb.AppendLine($"  - Sounds Triggered: {soundsTriggeredCount}");

        Debug.Log(sb.ToString());
    }
}