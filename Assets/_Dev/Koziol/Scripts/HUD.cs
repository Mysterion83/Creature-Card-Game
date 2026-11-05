using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI EnergyCounter;
    [SerializeField] private TextMeshProUGUI ObjectiveCounter;
    [SerializeField] private TextMeshProUGUI TurnText;
    [SerializeField] private Image SummoningPhase;
    [SerializeField] private Image CombatPhase;
    [SerializeField] private Color DefaultPhaseColour;
    [SerializeField] private Color ActivePhaseColour;

    [SerializeField] private GameObject TileInfo;
    [SerializeField] private TextMeshProUGUI TileInfoText;

    public void UpdatePhase(bool isSummoningPhase)
    {
        if (isSummoningPhase)
        {
            SummoningPhase.color = ActivePhaseColour;
            CombatPhase.color = DefaultPhaseColour;
        }
        else
        {
            SummoningPhase.color = DefaultPhaseColour;
            CombatPhase.color = ActivePhaseColour;
        }
    }

    public void SetTileInfoData(Tile tile, Vector2 mousePosition)
    {
        if (tile == null)
        {
            TileInfo.SetActive(false);
            return;
        }
        TileInfo.SetActive(true);
        string tileInfo = $"Tile: {tile.Data.Name}\n" +
            $"Element: {tile.Data.Element}\n" +
            $"Elemental Resonance: {tile.Data.EffectStrength}\n";

        Creature occupant = tile.Occupant;

        if (occupant != null)
        {
            tileInfo += $"Occupant: {occupant.Data.name}\n" +
                $"Health: {FormatValue((int)occupant.CurrentHealth, (int)occupant.Data.MaxHealth)}\n" +
                $"Attack: {occupant.Data.Attack} {occupant.Data.Defense}\n";
        }
        else
        {
            tileInfo += "Occupant: None\n";
        }
        TileInfoText.text = tileInfo;
    }

    public void UpdateTurn(PlayerData player)
    {
        TurnText.text = $"Turn: {player.Name}";
    }

    public void UpdateEnergyCounter(int currentEnergy, int maxEnergy)
    {
        EnergyCounter.text = FormatValue(currentEnergy, maxEnergy);
    }
    public void UpdateObjectiveCounter(int currentObjectives, int maxObjectives)
    {
        ObjectiveCounter.text = FormatValue(currentObjectives, maxObjectives);
    }

    static string FormatValue(int value, int maxValue)
    {
        return $"{value}/{maxValue}";
    }
}
