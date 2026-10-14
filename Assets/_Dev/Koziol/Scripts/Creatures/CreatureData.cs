using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Scriptable Objects/CreatureData")]
public class CreatureData : ScriptableObject
{
    #region Inspector Styling
    const string STYLING_HEADER = "<size=200%><margin=0em><align=\"left\"><b>";
    const string STYLING_SUBHEADER = "<size=150%><margin=0em><align=\"left\">";
    #endregion

    [Header(STYLING_HEADER + "Creature Info")]
    public string CreatureName;
    public Element Element;

    public float MaxHealth;
    public float Attack;
    public float Defense;
    public float EnergyCost;
    public float PurchaseCost;

    // Patterns
    [Header(STYLING_HEADER + "Patterns")]
    public Pattern AttackPattern;
    public Pattern MovementPattern;

    // Prefabs
    [Header(STYLING_HEADER + "Prefabs")]
    public GameObject CreaturePrefab;
    public GameObject CardPrefab;

    // Evolution
    [Header(STYLING_HEADER + "Evolution")]
    public float XpToEvolve;
}
