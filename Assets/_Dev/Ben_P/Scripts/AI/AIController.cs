using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AIController plays the turns for the Player its attached to as long as that
 * player has Is AI ticked. It doesnt touch the board everything goes through BattleAdapter
 */

[RequireComponent(typeof(Player))]
public class AIController : MonoBehaviour
{
    [Header("Pacing")]
    [SerializeField] private float _phaseStartDelay = 0.6f;
    [SerializeField] private float _actionDelay = 0.4f;

    [Header("Board")]
    [SerializeField] private Transform _selectionBox;

    [Header("Debug")]
    [SerializeField] private bool _logActions = true;

    private Player _aiPlayer;
    private BattleAdapter _adapter;

    /*
     * The AI follows MatchControllers phases rather than tracking turns itself. 
     * When a phase starts it checks it's this AI's turn then
     * starts the summon or action coroutine to match
     */

    void Awake() => _aiPlayer = GetComponent<Player>();

    void OnEnable()
    {
        MatchController.OnPhaseStarted += HandlePhaseStarted;
        MatchController.OnMatchEnded += HandleMatchEnded;
    }

    void OnDisable()
    {
        MatchController.OnPhaseStarted -= HandlePhaseStarted;
        MatchController.OnMatchEnded -= HandleMatchEnded;
    }
    void HandlePhaseStarted(Player player, MatchState state, float timeLimit)
    {
        if (player != _aiPlayer || !_aiPlayer.Data.IsAI) return;
        _adapter ??= new BattleAdapter(_aiPlayer, _selectionBox);
        StopAllCoroutines();
        if (state == MatchState.TurnSummon) StartCoroutine(SummonPhase());
        else if (state == MatchState.TurnAction) StartCoroutine(ActionPhase());
    }
    void HandleMatchEnded(Player winner)
    {
        StopAllCoroutines();
        Log(winner == _aiPlayer ? "won the match." : "lost the match.");
    }

    /*
     * After a delay the AI keeps summoning its next card onto a
     * random free tile until it runs out of cards/energy/space then moves on to
     * the action phase
     */

    IEnumerator SummonPhase()
    {
        Log($"--- Summon phase. Energy {_adapter.Energy}");
        yield return new WaitForSeconds(_phaseStartDelay);

        while (_adapter.IsMyPhase(MatchState.TurnSummon) && _adapter.CanSummon)
        {
            List<Tile> free = _adapter.FreeTiles();
            if (free.Count == 0) { Log("No free tile to summon on."); break; }

            CreatureData card = _adapter.NextCard;
            Tile tile = free[Random.Range(0, free.Count)];
            if (_adapter.Summon(tile))
                Log($"SUMMON {card.CreatureName} at {tile.TileIndex}. Energy left {_adapter.Energy}");
            else
            {
                LogRefused($"summon {card.CreatureName} at {tile.TileIndex}");
                break;
            }
            yield return new WaitForSeconds(_actionDelay);
        }

        if (_adapter.IsMyPhase(MatchState.TurnSummon))
        {
            Log("Summon phase done.");
            _adapter.NextPhase();
        }
    }

    /*
     * The AI goes through its creatures in a random order and gives each
     * one a single action. If an enemy is in its attack range it attacks one at random
     * otherwise it moves to a random tile it can reach. It works from a copy of the
     * creature list in case one dies partway through and stops early if it runs out
     * of energy or the phase ends. Once every creature has had its go the turn ends
     * Choices are random for now I'll smarter decisions replace this later
     */

    IEnumerator ActionPhase()
    {
        Log($"--- Action phase. Energy {_adapter.Energy}, creatures {_adapter.MyCreatures.Count}");
        yield return new WaitForSeconds(_phaseStartDelay);

        var creatures = new List<Creature>(_adapter.MyCreatures);
        Shuffle(creatures);

        foreach (Creature c in creatures)
        {
            if (!_adapter.IsMyPhase(MatchState.TurnAction)) yield break;
            if (c == null) continue;
            if (_adapter.Energy <= 0) { Log("Out of energy."); break; }

            List<Tile> targets = _adapter.AttackTiles(c);
            if (targets.Count > 0)
            {
                Tile target = targets[Random.Range(0, targets.Count)];
                Creature victim = target.Occupant;
                string victimName = victim.Data.CreatureName;
                float hpBefore = victim.CurrentHealth;

                if (_adapter.Attack(c, target))
                {
                    float hpAfter = victim.CurrentHealth;
                    string result = hpAfter <= 0 ? "KILLED it" : $"HP {hpBefore} -> {hpAfter}";
                    Log($"ATTACK {c.Data.CreatureName} at {c.OccupyingTile.TileIndex} hit {victimName} at {target.TileIndex}: {result}. Energy left {_adapter.Energy}");
                }
                else LogRefused($"attack by {c.Data.CreatureName} on {victimName}");
            }
            else
            {
                List<Tile> moves = _adapter.MoveTiles(c);
                if (moves.Count == 0)
                {
                    Log($"{c.Data.CreatureName} has no enemy in range and nowhere to move.");
                }
                else
                {
                    Vector2Int from = c.OccupyingTile.TileIndex;
                    Tile to = moves[Random.Range(0, moves.Count)];
                    if (_adapter.Move(c, to))
                        Log($"MOVE {c.Data.CreatureName} {from} -> {to.TileIndex} (no enemy in range). Energy left {_adapter.Energy}");
                    else
                        LogRefused($"move by {c.Data.CreatureName} {from} -> {to.TileIndex}");
                }
            }
            yield return new WaitForSeconds(_actionDelay);
        }

        if (_adapter.IsMyPhase(MatchState.TurnAction))
        {
            Log("Turn done.");
            _adapter.NextPhase();
        }
    }

    // Helpers

    void Log(string message)
    {
        if (_logActions) Debug.Log($"<color=orange>[AI {_aiPlayer.Data.Name}]</color> {message}");
    }

    void LogRefused(string action) => Debug.LogWarning($"[AI {_aiPlayer.Data.Name}] The game refused {action}.");

    static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}