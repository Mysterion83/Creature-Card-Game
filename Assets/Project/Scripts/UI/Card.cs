using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [HideInInspector] private Image _card;
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

    private void Awake()
    {
        _card = GetComponent<Image>();
    }

    public void SetCardData(CreatureData data)
    {
        _card.color = data.Element.Color;
        creatureData = data;
        CreatureArt.sprite = data.CreatureImage;
        MovementPattern.sprite = null;
        AttackPattern.sprite = null;
        Element.sprite = data.Element.Icon;
        NameText.text = data.CreatureName;
        HPText.text = data.MaxHealth.ToString();
        AttackText.text = data.Attack.ToString();
        DefenseText.text = data.Defense.ToString();
        EnergyCostText.text = data.EnergyCost.ToString();
    }

    public void OnValidate()
    {
        _card = GetComponent<Image>();
        SetCardData(creatureData);
    }
}
