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

    [Header("Brain")]
    [SerializeField] private AIProfile _profile;

    [Header("Debug")]
    [SerializeField] private bool _logActions = true;

    private Player _aiPlayer;
    private BattleAdapter _adapter;
    private AIProfile _fallbackProfile;
    private bool _lastActionWorked;
    private readonly HashSet<Creature> _movedThisTurn = new();
    private readonly HashSet<Creature> _attackedThisTurn = new();

    private AIProfile Profile => _profile != null ? _profile : (_fallbackProfile ??= ScriptableObject.CreateInstance<AIProfile>());

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

        yield return RunPhase(MatchState.TurnSummon);

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
        _movedThisTurn.Clear();
        _attackedThisTurn.Clear();
        yield return new WaitForSeconds(_phaseStartDelay);

        yield return RunPhase(MatchState.TurnAction);

        if (_adapter.IsMyPhase(MatchState.TurnAction))
        {
            Log("Turn done.");
            _adapter.NextPhase();
        }
    }

    IEnumerator RunPhase(MatchState state)
    {
        AIProfile profile = Profile;

        for (int i = 0; i < profile.MaxActionsPerPhase; i++)
        {
            if (!_adapter.IsMyPhase(state)) yield break;

            List<Creature> enemies = _adapter.EnemyCreatures();
            List<AIAction> actions = state == MatchState.TurnSummon
                ? ActionGenerator.Summons(_adapter)
                : ActionGenerator.Actions(_adapter, _movedThisTurn, _attackedThisTurn);

            if (actions.Count == 0)
            {
                Log(state == MatchState.TurnSummon ? "Nothing left to summon." : "No actions left.");
                yield break;
            }

            foreach (AIAction action in actions)
                action.Score = ActionScorer.Score(action, _adapter, profile, enemies);
            actions.Sort((a, b) => b.Score.CompareTo(a.Score));

            if (profile.LogDecisions) LogTopActions(actions, 5);

            AIAction best = actions[0];
            if (best.Score < profile.EndTurnThreshold)
            {
                Log($"Best score {best.Score:0.0} is below the end turn threshold, stopping.");
                yield break;
            }

            yield return Perform(best);
            if (!_lastActionWorked) yield break;

            yield return new WaitForSeconds(_actionDelay);
        }
    }

    IEnumerator Perform(AIAction action)
    {
        _lastActionWorked = false;
        string score = action.Score.ToString("0.0");

        switch (action.Type)
        {
            case AIActionType.Summon:
                if (_adapter.Summon(action.Tile))
                {
                    _lastActionWorked = true;
                    Log($"SUMMON {action.Card.CreatureName} at {action.Tile.TileIndex} (score {score}). Energy left {_adapter.Energy}");
                }
                else LogRefused($"summon {action.Card.CreatureName} at {action.Tile.TileIndex}");
                break;

            case AIActionType.Move:
                _lastActionWorked = DoMove(action.Actor, action.Tile, score);
                break;

            case AIActionType.Attack:
                _lastActionWorked = DoAttack(action.Actor, action.Tile, score);
                break;

            case AIActionType.MoveThenAttack:
                if (!DoMove(action.Actor, action.Tile, score)) break;
                yield return new WaitForSeconds(_actionDelay);
                _lastActionWorked = _adapter.IsMyPhase(MatchState.TurnAction) && DoAttack(action.Actor, action.AttackTarget, score);
                break;
        }
    }

    bool DoMove(Creature creature, Tile to, string score)
    {
        Vector2Int from = creature.OccupyingTile.TileIndex;
        if (_adapter.Move(creature, to))
        {
            _movedThisTurn.Add(creature);
            Log($"MOVE {creature.Data.CreatureName} {from} -> {to.TileIndex} (score {score}). Energy left {_adapter.Energy}");
            return true;
        }
        LogRefused($"move by {creature.Data.CreatureName} {from} -> {to.TileIndex}");
        return false;
    }

    bool DoAttack(Creature attacker, Tile target, string score)
    {
        Creature victim = target.Occupant;
        if (victim == null) return false;
        string victimName = victim.Data.CreatureName;
        float hpBefore = victim.CurrentHealth;

        if (_adapter.Attack(attacker, target))
        {
            _attackedThisTurn.Add(attacker);
            float hpAfter = victim.CurrentHealth;
            string result = hpAfter <= 0 ? "KILLED it" : $"HP {hpBefore} -> {hpAfter}";
            Log($"ATTACK {attacker.Data.CreatureName} at {attacker.OccupyingTile.TileIndex} hit {victimName} at {target.TileIndex}: {result} (score {score}). Energy left {_adapter.Energy}");
            return true;
        }
        LogRefused($"attack by {attacker.Data.CreatureName} on {victimName}");
        return false;
    }

    // Helpers

    void Log(string message)
    {
        if (_logActions) Debug.Log($"<color=orange>[AI {_aiPlayer.Data.Name}]</color> {message}");
    }

    void LogRefused(string action) => Debug.LogWarning($"[AI {_aiPlayer.Data.Name}] The game refused {action}.");

    void LogTopActions(List<AIAction> actions, int count)
    {
        string text = $"<color=orange>[AI {_aiPlayer.Data.Name}]</color> Top actions:";
        for (int i = 0; i < Mathf.Min(count, actions.Count); i++)
            text += $"\n  {actions[i].Score,7:0.0}  {actions[i]}";
        Debug.Log(text);
    }
}