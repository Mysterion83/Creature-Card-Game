using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Project/CreatureData")]
public class CreatureData : ScriptableObject
{
    #region Inspector Styling
    const string STYLING_HEADER = "<size=200%><margin=0em><align=\"left\"><b>";
    const string STYLING_SUBHEADER = "<size=150%><margin=0em><align=\"left\">";
    #endregion

    [Header(STYLING_HEADER + "Creature Info")]
    public string CreatureName;
    public Element Element;
    public CreatureType Type;

    public float MaxHealth;
    public float Attack;
    public float Defense;
    public float EnergyCost;
    public float PurchaseCost;
    
    [Header(STYLING_SUBHEADER + "Attack Durations")] 
    [Min(0)] public float WindupDuration = 2.5f;
    [Min(0)] public float AttackDuration = 0.2f;
    [Min(0)] public float RecoveryDuration = 2.5f;
    
    // Patterns
    [Header(STYLING_HEADER + "Patterns")]
    public Pattern AttackPattern;
    public Pattern MovementPattern;

    // Prefabs
    [Header(STYLING_HEADER + "Prefabs")]
    public GameObject CreaturePrefab;
    public Sprite CreatureImage;

    // Evolution
    [Header(STYLING_HEADER + "Evolution")]
    public float XpToEvolve;
}
