using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SFXBoardProfile", menuName = "Project/Audio/Profiles/SFXBoardProfile")]
public class SFXBoardProfile : ScriptableObject
{
    [Header("Profile Lists")]
    [SerializeField] private List<SFXProfile> _selectSounds = new();
    [SerializeField] private List<SFXProfile> _summonSounds = new();
    [SerializeField] private List<SFXProfile> _moveSounds = new();

    public IReadOnlyList<SFXProfile> SelectSounds => _selectSounds;
    public IReadOnlyList<SFXProfile> SummonSounds => _summonSounds;
    public IReadOnlyList<SFXProfile> MoveSounds => _moveSounds;
}
