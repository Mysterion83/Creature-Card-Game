using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchLogger : MonoBehaviour
{
    private const string END_COLOR_TAG = "</color>";
    /// <summary>
    /// Stores the log entries recorded during the match with formatting tags.
    /// </summary>
    private LogList _logs;
    /// <summary>
    /// Stores the log entries recorded during the match without formatting tags.
    /// </summary>
    private LogList _rawLogs;

    /// <summary>
    /// Gets the log entries collected so far.
    /// </summary>
    /// <returns>The list of log entries.</returns>
    public List<LogEntry> GetLogs() { return _logs.Logs; }
    /// <summary>
    /// Gets the raw log entries.
    /// </summary>
    /// <remarks>The returned list is the logger's underlying list.</remarks>
    /// <returns>The list of raw log entries.</returns>
    public List<LogEntry> GetRawLogs() { return _rawLogs.Logs; }

    /// <summary>
    /// Creates a log entry and adds it to the internal log collection.
    /// </summary>
    /// <param name="logList">A log list that is initialized if it is null.</param>
    /// <param name="idCounter">The counter used to assign the entry's log ID; incremented after assignment.</param>
    /// <param name="logType">The type of the log entry.</param>
    /// <param name="logText">The text of the log entry.</param>
    private void Log(ref LogList logList, LogType logType, string logText)
    {
        if (logList.Logs == null) 
        { 
            logList = new LogList(); 
            logList.IDCounter = 0; 
        }
        LogEntry entry = new LogEntry
        {
            LogID = logList.IDCounter++,
            LogType = logType,
            LogText = logText
        };
        logList.Logs.Add(entry);
    }

    /// <summary>
    /// Logs an attack, including its outcome and any damage dealt.
    /// </summary>
    /// <param name="attackResult">The result containing the attacker, defender, damage dealt, and whether the defender was killed.</param>
    public void LogAttack(AttackResult attackResult)
    {
        string logEntry = "";
        string rawLogEntry = "";

        logEntry += $"<color={attackResult.Attacker.Owner.Data.TeamColor}>{attackResult.Attacker.Owner.Data.Name}'s {attackResult.Attacker.Data.CreatureName}{END_COLOR_TAG}";
        rawLogEntry += $"{attackResult.Attacker.Owner.Data.Name}'s {attackResult.Attacker.Data.CreatureName}";

        if (attackResult.IsDead)
        {
            logEntry += " killed";
            rawLogEntry += " killed";
        }
        else
        {
            logEntry += "attacked";
            rawLogEntry += "attacked";
        }

        logEntry += $" {attackResult.Defender.Owner.Data.TeamColor}{attackResult.Defender.Owner.Data.Name}'s {attackResult.Defender.Data.CreatureName}{END_COLOR_TAG}";
        rawLogEntry += $" {attackResult.Defender.Owner.Data.Name}'s {attackResult.Defender.Data.CreatureName} ";

        if (attackResult.DamageDealt > 0 && !attackResult.IsDead)
        {
            logEntry += $" for {attackResult.DamageDealt} damage.";
            rawLogEntry += $" for {attackResult.DamageDealt} damage.";
        }
        else if (attackResult.DamageDealt == 0 && !attackResult.IsDead)
        {
            logEntry += " but it was ineffective.";
            rawLogEntry += "but it was ineffective.";
        }

        Log(ref _logs, LogType.Attack, logEntry);
        Log(ref _rawLogs, LogType.Attack, rawLogEntry);
    }
    /// <summary>
    /// Logs a phase change to the summoning or combat phase.
    /// </summary>
    /// <param name="isSummoningPhase">true to log a change to the summoning phase; false to log a change to the combat phase.</param>
    public void LogPhaseChange(bool isSummoningPhase)
    {
        string logEntry = "";
        if (isSummoningPhase)
        {
            logEntry = "Phase changed to Summoning.";
        }
        else
        {
            logEntry = "Phase changed to Combat.";
        }

        Log(ref _logs, LogType.PhaseChange, logEntry);
        Log(ref _rawLogs, LogType.PhaseChange, logEntry);
    }
    /// <summary>
    /// Records a turn-change log entry for the specified player.
    /// </summary>
    /// <param name="player">The player whose turn it is.</param>
    public void LogTurnChange(Player player)
    {
        string logEntry = $"<color={player.Data.TeamColor}>{player.Data.Name}'s{END_COLOR_TAG} turn.";
        string rawLogEntry = $"{player.Data.Name}'s turn.";
        Log(ref _logs, LogType.TurnChange, logEntry);
        Log(ref _rawLogs, LogType.TurnChange, rawLogEntry);
    }
    /// <summary>
    /// Records a creature's movement in the match logs.
    /// </summary>
    /// <param name="creature">The creature that moved.</param>
    /// <param name="from">The creature's starting position.</param>
    /// <param name="to">The creature's destination position.</param>
    public void LogMovement(Creature creature, Vector2Int from, Vector2Int to)
    {
        string logEntry = $"<color={creature.Owner.Data.TeamColor}>{creature.Owner.Data.Name}'s {creature.Data.CreatureName}{END_COLOR_TAG} moved from {from} to {to}.";
        string rawLogEntry = $"{creature.Owner.Data.Name}'s {creature.Data.CreatureName} moved from {from} to {to}.";
        Log(ref _logs, LogType.Movement, logEntry);
        Log(ref _rawLogs, LogType.Movement, rawLogEntry);
    }
}

public struct LogList
{
    public uint IDCounter;
    public List<LogEntry> Logs;
}

[Serializable]
public struct LogEntry 
{
    public uint LogID;
    public LogType LogType;
    public string LogText;

    public override string ToString()
    {
        return $"[{LogID}] {LogType}: {LogText}";
    }
}

public enum LogType 
{
    TurnChange,
    PhaseChange,
    Movement,
    Attack
}