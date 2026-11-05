using System.Collections.Generic;
using UnityEngine;

/*
 * BattleAdapter sits between the AI and the rest of you guys code. Anything the AI
 * needs to know about the match, or anything it wants to do on the board, goes through
 * here instead of calling MatchController / BoardController or Player directly. That way
 * if those scripts change, this is the only file that needs updating. The constructor
 * just stores the AI's player and the selection box and finds the match and board in
 * the scene.
 */

public class BattleAdapter
{
    private readonly Player _aiPlayer;
    private readonly MatchController _match;
    private readonly BoardController _boardController;
    private readonly Board _board;
    private readonly Transform _selectionBox;

    public BattleAdapter(Player aiPlayer, Transform selectionBox)
    {
        _aiPlayer = aiPlayer;
        _selectionBox = selectionBox;
        _match = Object.FindAnyObjectByType<MatchController>();
        _boardController = Object.FindAnyObjectByType<BoardController>();
        _board = _boardController.GetComponent<Board>();
    }

    /*
     * These answer the basic questions the AI asks every turn:
     * -is it my phase
     * -how much energy do I have
     * -what card is next
     * -can I afford it
     * -which creatures do I have on the board etc
     * The deck hands out cards in a fixed order so the AI can only choose where to summon
     */

    public bool IsMyPhase(MatchState state) =>
        _match.IsMatchActive && _match.CurrentPlayer == _aiPlayer && _match.CurrentState == state;
    public float Energy => _aiPlayer.EnergySystem.Energy.CurrentAmount;
    public CreatureData NextCard => _aiPlayer.SelectedCreature;
    public bool CanSummon => _aiPlayer.CanSummon() && NextCard != null && Energy >= NextCard.EnergyCost;
    public IReadOnlyList<Creature> MyCreatures => _aiPlayer.ActiveCreatures;

    public List<Tile> FreeTiles()
    {
        var result = new List<Tile>();
        foreach (Tile tile in _board.GetData().Tiles)
            if (tile != null && tile.Data.IsPassable && tile.Occupant == null) result.Add(tile);
        return result;
    }

    public List<Tile> MoveTiles(Creature c)
    {
        var result = new List<Tile>();
        foreach (Vector2Int offset in c.ShowValidMovement())
        {
            Tile tile = TileAt(c.OccupyingTile.TileIndex + offset);
            if (tile != null && tile.Data.IsPassable && tile.Occupant == null) result.Add(tile);
        }
        return result;
    }

    public List<Tile> AttackTiles(Creature c)
    {
        var result = new List<Tile>();
        foreach (Vector2Int offset in c.ShowValidAttacks())
        {
            Tile tile = TileAt(c.OccupyingTile.TileIndex + offset);
            if (tile != null && tile.Occupant != null && tile.Occupant.Owner != _aiPlayer.Data) result.Add(tile);
        }
        return result;
    }

    Tile TileAt(Vector2Int index) =>
        _board.IsIndexInBounds(index) ? _board.GetData().Tiles[index.x, index.y] : null;

    /*
     * BoardController summons and moves onto whatever tile its selection box is over
     * since thats normally where the mouse is. PointAt moves
     * the box onto the tile the AI picked so those calls use it instead. The box snaps
     * back to the mouse next frame so PointAt always has to be called right before the
     * action. (Yes I know thats scuffed but it works)    
     */

    void PointAt(Tile tile) => _selectionBox.position = tile.transform.position + Vector3.up;

    public bool Summon(Tile tile)
    {
        PointAt(tile);
        return _boardController.PlaceCreature(NextCard) != null;
    }

    public bool Move(Creature c, Tile tile)
    {
        PointAt(tile);
        _boardController.MoveCreature(c, tile);
        return c.OccupyingTile == tile;
    }

    public bool Attack(Creature attacker, Tile target)
    {
        float before = Energy;
        _boardController.AttackCreature(attacker, target);
        return Energy < before;
    }

    public void NextPhase() => _match.RequestNextPhase();
}