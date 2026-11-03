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
    readonly Player aiPlayer;                   
    readonly MatchController match;             
    readonly BoardController boardController;    
    readonly Board board;                        
    readonly Transform selectionBox;             
        
    public BattleAdapter(Player aiPlayer, Transform selectionBox)
    {
        this.aiPlayer = aiPlayer;
        this.selectionBox = selectionBox;
        match = Object.FindAnyObjectByType<MatchController>();
        boardController = Object.FindAnyObjectByType<BoardController>();
        board = boardController.GetComponent<Board>();
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
        match.IsMatchActive && match.CurrentPlayer == aiPlayer && match.CurrentState == state;       
    public float Energy => aiPlayer.EnergySystem.Energy.CurrentAmount;
    public CreatureData NextCard => aiPlayer.SelectedCreature;        
    public bool CanSummon => aiPlayer.CanSummon() && NextCard != null && Energy >= NextCard.EnergyCost;     
    public IReadOnlyList<Creature> MyCreatures => aiPlayer.ActiveCreatures;

   public List<Tile> FreeTiles()
    {
        var result = new List<Tile>();
        foreach (Tile tile in board.GetData().Tiles)
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
            if (tile != null && tile.Occupant != null && tile.Occupant.Owner != aiPlayer.Data) result.Add(tile);
        }
        return result;
    }
   
    Tile TileAt(Vector2Int index) =>
        board.IsIndexInBounds(index) ? board.GetData().Tiles[index.x, index.y] : null;

    /*
     * BoardController summons and moves onto whatever tile its selection box is over
     * since thats normally where the mouse is. PointAt moves
     * the box onto the tile the AI picked so those calls use it instead. The box snaps
     * back to the mouse next frame so PointAt always has to be called right before the
     * action. (Yes I know thats scuffed but it works)    
     */

    void PointAt(Tile tile) => selectionBox.position = tile.transform.position + Vector3.up;
        
    public bool Summon(Tile tile)
    {
        PointAt(tile);
        return boardController.PlaceCreature(NextCard) != null;
    }
        
    public bool Move(Creature c, Tile tile)
    {
        PointAt(tile);
        boardController.MoveCreature(c, tile);
        return c.OccupyingTile == tile;
    }
     
    public bool Attack(Creature attacker, Tile target)
    {
        float before = Energy;
        boardController.AttackCreature(attacker, target);
        return Energy < before;
    }
      
    public void NextPhase() => match.RequestNextPhase();
}