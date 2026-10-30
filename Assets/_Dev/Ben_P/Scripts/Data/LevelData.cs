using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Ben_P/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public string Id;                 
    public Element Wing;              // piotr enum: Fire, Water, Nature (Earth), Air (Wind)
    public int Number = 1;            
    public string LevelName;          

    [Header("Details panel")]
    [Range(1, 4)] public int Difficulty = 2;
    [TextArea(2, 4)] public string Description;
    public Sprite Preview;

    [Header("Loading")]
    public string SceneName;

    [Header("Rewards")]
    public int CoinReward = 200;

    [Header("Unlocking")]
    [Tooltip("Leave empty for the first level of a wing")]
    public LevelData RequiredLevel;

   
    void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Id)) Id = name;
    }
}