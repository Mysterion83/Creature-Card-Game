using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image CreatureArt;
    [SerializeField] private Image MovementPattern;
    [SerializeField] private Image AttackPattern;
    [SerializeField] private Image Element;
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private TextMeshProUGUI AttackText;
    [SerializeField] private TextMeshProUGUI DefenseText;
    [SerializeField] private TextMeshProUGUI EnergyCostText;

    public CreatureData creatureData;


    public void SetCardData(CreatureData data)
    {
        creatureData = data;
        CreatureArt.sprite = data.CreatureImage;
        MovementPattern.sprite = null;
        AttackPattern.sprite = null;
        Element.sprite = null;
        NameText.text = data.CreatureName;
        HPText.text = data.MaxHealth.ToString();
        AttackText.text = data.Attack.ToString();
        DefenseText.text = data.Defense.ToString();
        EnergyCostText.text = data.EnergyCost.ToString();
    }

    public void OnValidate()
    {
        SetCardData(creatureData);
    }
}
