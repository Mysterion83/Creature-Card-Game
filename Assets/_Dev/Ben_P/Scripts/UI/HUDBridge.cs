using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * HUDBridge feeds the match into Piotrs HUD the same way BattleAdapter sits
 * between the AI and the you guys code. Phase and turn update when the match state
 * changes energy and zones are checked every frame.
 */

public class HUDBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUD _hud;
    [SerializeField] private Button _advancePhaseButton;
    [SerializeField] private Transform _handContent;

    private MatchController _match;
    private Board _board;
    private readonly List<GameObject> _handCards = new();
    private readonly HashSet<Creature> _summoned = new();

    private void Awake()
    {
        if (_hud == null) _hud = FindAnyObjectByType<HUD>();
        if (_advancePhaseButton == null && _hud != null) _advancePhaseButton = FindChildButton("AdvancePhaseButton");
        _match = FindAnyObjectByType<MatchController>();
        _board = FindAnyObjectByType<Board>();
        if (_handContent == null && _hud != null) _handContent = FindHandContent();

        if (_hud == null || _match == null)
        {
            Debug.LogError("HUDBridge: needs a HUD and a MatchController in the scene.");
            enabled = false;
        }
    }

    private void OnEnable() => MatchController.OnMatchStateChanged += HandleStateChanged;
    private void OnDisable() => MatchController.OnMatchStateChanged -= HandleStateChanged;

    private void Start()
    {
        if (_advancePhaseButton != null) _advancePhaseButton.onClick.AddListener(_match.RequestNextPhase);
        BuildHand();
    }

    private void Update()
    {
        Player localPlayer = FindLocalPlayer();

        if (localPlayer != null)
        {
            Resource energy = localPlayer.EnergySystem.Energy;
            _hud.UpdateEnergyCounter((int)energy.CurrentAmount, (int)energy.MaxAmount);
        }

        int totalZones = _board != null ? _board.ActiveZones.Count : 0;
        _hud.UpdateObjectiveCounter(ZonesHeldBy(localPlayer), totalZones);

        UpdateHand(localPlayer);

        if (_advancePhaseButton != null)
            _advancePhaseButton.interactable = _match.IsMatchActive && _match.CurrentPlayer != null && !_match.CurrentPlayer.Data.IsAI;
    }

    private void HandleStateChanged(MatchState state)
    {
        if (state == MatchState.TurnSummon) _hud.UpdatePhase(true);
        else if (state == MatchState.TurnAction) _hud.UpdatePhase(false);

        if (_match.CurrentPlayer != null) _hud.UpdateTurn(_match.CurrentPlayer.Data);
    }

    private void BuildHand()
    {
        Player localPlayer = FindLocalPlayer();
        if (_handContent == null || localPlayer == null || !localPlayer.TryGetComponent(out PlayerDeck deck)) return;

        Card template = _handContent.GetComponentInChildren<Card>(true);
        if (template == null) return;

        foreach (CreatureData creature in deck.CreaturesToPlace)
        {
            Card card = Instantiate(template, _handContent);
            card.gameObject.SetActive(true);
            card.SetCardData(creature);
            _handCards.Add(card.gameObject);
        }

        foreach (Transform child in _handContent)
            if (!_handCards.Contains(child.gameObject)) Destroy(child.gameObject);
    }

    private void UpdateHand(Player localPlayer)
    {
        if (localPlayer == null || _handCards.Count == 0) return;

        foreach (Creature creature in localPlayer.ActiveCreatures)
            if (creature != null) _summoned.Add(creature);

        for (int i = 0; i < _handCards.Count; i++)
            if (_handCards[i] != null) _handCards[i].SetActive(i >= _summoned.Count);
    }

    private Transform FindHandContent()
    {
        Card card = _hud.GetComponentInChildren<Card>(true);
        return card != null ? card.transform.parent : null;
    }

    private Player FindLocalPlayer()
    {
        foreach (Player player in PlayerRegistry.MasterList)
            if (player != null && !player.Data.IsAI) return player;
        return null;
    }

    private int ZonesHeldBy(Player player)
    {
        if (_board == null || player == null) return 0;
        int count = 0;
        foreach (ZoneInstance zone in _board.ActiveZones)
            if (zone.Occupier == player) count++;
        return count;
    }

    private Button FindChildButton(string objectName)
    {
        foreach (Button button in _hud.GetComponentsInChildren<Button>(true))
            if (button.name == objectName) return button;
        return null;
    }
}
