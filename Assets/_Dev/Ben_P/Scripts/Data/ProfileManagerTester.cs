using UnityEngine;

public class ProfileManagerTester : MonoBehaviour
{
    [SerializeField] StarterDeck testStarter;
    [SerializeField] LevelData firstLevel;    
    [SerializeField] LevelData secondLevel;   
    [SerializeField] int slot = 0;

    ProfileManager PM => ProfileManager.Instance;

    [ContextMenu("1 New Game")] void T1() { PM.NewGame(slot, testStarter); Print(); }
    [ContextMenu("2 Load Game")] void T2() { Debug.Log($"Loaded: {PM.LoadGame(slot)}"); Print(); }
    [ContextMenu("3 Add 100 Coins")] void T3() { PM.AddCoins(100); Print(); }
    [ContextMenu("4 Spend 1000")] void T4() { Debug.Log($"Spent: {PM.TrySpend(1000)}"); Print(); }
    [ContextMenu("5 Check Unlocks")] void T5() => Debug.Log($"Level 2 unlocked: {PM.IsUnlocked(secondLevel)}");
    [ContextMenu("6 Win Level 1")] void T6() { PM.DebugSetCurrentLevel(firstLevel); PM.CompleteCurrentLevel(); Print(); }
    [ContextMenu("7 Reroll Shop")] void T7() { PM.RerollShop(); Print(); }

    void Print() => Debug.Log(JsonUtility.ToJson(PM.Profile, true));
}