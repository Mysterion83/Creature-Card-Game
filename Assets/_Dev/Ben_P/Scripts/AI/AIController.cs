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
     * After a delay the AI keeps summoning its next card onto the free spawn tile
     * closest to an enemy (or the middle of the board if there are no enemies yet)
     * until it runs out of cards/energy/space then moves on to the action phase
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
            Tile tile = AIRules.ChooseSummonTile(free, _adapter.EnemyCreatures(), _adapter.BoardCentre);
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
     * The AI goes through its creatures closest to the enemy first and asks AIRules
     * what each one should do. If it advances it then attacks if an enemy is now in range. 
     * It works from a copy of the creature list in case one dies partway through and 
     * stops early if it runs out of energy or the phase ends.
     */

    IEnumerator ActionPhase()
    {
        Log($"--- Action phase. Energy {_adapter.Energy}, creatures {_adapter.MyCreatures.Count}");
        yield return new WaitForSeconds(_phaseStartDelay);

        var creatures = new List<Creature>(_adapter.MyCreatures);
        creatures.RemoveAll(c => c == null);
        List<Creature> startEnemies = _adapter.EnemyCreatures();
        creatures.Sort((a, b) => AIRules.DistanceToNearest(a.OccupyingTile.TileIndex, startEnemies)
            .CompareTo(AIRules.DistanceToNearest(b.OccupyingTile.TileIndex, startEnemies)));

        foreach (Creature c in creatures)
        {
            if (!_adapter.IsMyPhase(MatchState.TurnAction)) yield break;
            if (c == null) continue;
            if (_adapter.Energy <= 0) { Log("Out of energy."); break; }

            RuleChoice choice = AIRules.ChooseAction(c, _adapter, _adapter.EnemyCreatures());
            bool advanced = false;

            if (choice.Rule == RuleType.Advance)
            {
                Vector2Int from = c.OccupyingTile.TileIndex;
                Tile to = choice.Target;
                if (!_adapter.Move(c, to))
                {
                    LogRefused($"move by {c.Data.CreatureName} {from} -> {to.TileIndex}");
                    yield return new WaitForSeconds(_actionDelay);
                    continue;
                }
                Log($"ADVANCE {c.Data.CreatureName} {from} -> {to.TileIndex}. Energy left {_adapter.Energy}");
                advanced = true;
                if (_adapter.Energy <= 0) { Log("Out of energy."); break; }
                yield return new WaitForSeconds(_actionDelay);
                choice = AIRules.ChooseAttack(c, _adapter);
            }

            if (choice.Rule == RuleType.Wait)
            {
                if (!advanced) Log($"WAIT {c.Data.CreatureName}: no enemy in range and no move gets closer.");
            }
            else
            {
                Tile target = choice.Target;
                Creature victim = target.Occupant;
                string victimName = victim.Data.CreatureName;
                float hpBefore = victim.CurrentHealth;

                if (_adapter.Attack(c, target))
                {
                    float hpAfter = victim.CurrentHealth;
                    string result = hpAfter <= 0 ? "KILLED it" : $"HP {hpBefore} -> {hpAfter}";
                    Log($"{choice.Rule.ToString().ToUpper()} {c.Data.CreatureName} at {c.OccupyingTile.TileIndex} hit {victimName} at {target.TileIndex}: {result}. Energy left {_adapter.Energy}");
                }
                else LogRefused($"attack by {c.Data.CreatureName} on {victimName}");
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
}