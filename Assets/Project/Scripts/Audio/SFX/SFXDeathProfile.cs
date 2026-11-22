using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFXDeathProfile", menuName = "Project/Audio/Profiles/SFXDeathProfile")]
public class SFXDeathProfile : ScriptableObject
{
    [Header("Profile List")]
    [SerializeField] private List<SFXProfile> _sounds = new();

    public IReadOnlyList<SFXProfile> Sounds => _sounds;
}