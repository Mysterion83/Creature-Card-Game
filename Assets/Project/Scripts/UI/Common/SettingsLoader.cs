using UnityEngine;
using UnityEngine.Audio;

public class SettingsLoader : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    void Start() => SettingsMenu.ApplySaved(mixer);
}