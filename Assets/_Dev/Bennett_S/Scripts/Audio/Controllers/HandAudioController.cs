using UnityEngine;

/// <summary>
/// Class for handling audio within <c>PlayerHand</c>
/// </summary>
public class HandAudioController : MonoBehaviour
{
    [Header("Card Interaction SFX")]
    [SerializeField] private SFXDefinition _cardHoverSfx;
    [SerializeField] private SFXDefinition _cardSelectSfx;
    [SerializeField] private SFXDefinition _cardDeselectSfx;
    [SerializeField] private SFXDefinition _cannotAffordSfx;

    [Header("Deck SFX")]
    [SerializeField] private SFXDefinition _handPopulatedSfx;

    private void OnEnable()
    {
        PlayerHand.OnCardHovered += HandleCardHovered;
        PlayerHand.OnCardSelected += HandleCardSelected;
        PlayerHand.OnCardDeselected += HandleCardDeselected;
        PlayerHand.OnCannotAffordCard += HandleCannotAfford;
        PlayerHand.OnHandPopulated += HandleHandPopulated;
    }

    private void OnDisable()
    {
        PlayerHand.OnCardHovered -= HandleCardHovered;
        PlayerHand.OnCardSelected -= HandleCardSelected;
        PlayerHand.OnCardDeselected -= HandleCardDeselected;
        PlayerHand.OnCannotAffordCard -= HandleCannotAfford;
        PlayerHand.OnHandPopulated -= HandleHandPopulated;
    }

    private void HandleCardHovered(CreatureData creature) => PlaySfx(_cardHoverSfx);
    private void HandleCardSelected(CreatureData creature) => PlaySfx(_cardSelectSfx);
    private void HandleCardDeselected(CreatureData creature) => PlaySfx(_cardDeselectSfx);
    private void HandleCannotAfford() => PlaySfx(_cannotAffordSfx);
    private void HandleHandPopulated(int cardCount) => PlaySfx(_handPopulatedSfx);

    private static void PlaySfx(SFXDefinition definition)
    {
        if (definition && AudioManager.Instance)
        {
            AudioManager.Instance.SFX.Play(definition);
        }
    }
}