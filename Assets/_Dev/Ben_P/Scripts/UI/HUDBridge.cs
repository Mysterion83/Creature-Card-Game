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

    private MatchController _match;
    private Board _board;

    private void Awake()
    {
        if (_hud == null) _hud = FindAnyObjectByType<HUD>();
        if (_advancePhaseButton == null && _hud != null) _advancePhaseButton = FindChildButton("AdvancePhaseButton");
        _match = FindAnyObjectByType<MatchController>();
        _board = FindAnyObjectByType<Board>();

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
    }

    private void Update()
    {
        Player human = FindHumanPlayer();

        if (human != null)
        {
            Resource energy = human.EnergySystem.Energy;
            _hud.UpdateEnergyCounter((int)energy.CurrentAmount, (int)energy.MaxAmount);
        }

        int totalZones = _board != null ? _board.ActiveZones.Count : 0;
        _hud.UpdateObjectiveCounter(ZonesHeldBy(human), totalZones);

        if (_advancePhaseButton != null)
            _advancePhaseButton.interactable = _match.IsMatchActive && _match.CurrentPlayer != null && !_match.CurrentPlayer.Data.IsAI;
    }

    private void HandleStateChanged(MatchState state)
    {
        if (state == MatchState.TurnSummon) _hud.UpdatePhase(true);
        else if (state == MatchState.TurnAction) _hud.UpdatePhase(false);

        if (_match.CurrentPlayer != null) _hud.UpdateTurn(_match.CurrentPlayer.Data);
    }

    private Player FindHumanPlayer()
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
