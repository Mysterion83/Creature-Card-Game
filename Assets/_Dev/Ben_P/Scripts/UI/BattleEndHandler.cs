using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Takes the player back to Level Select when a match ends. If the
 * player won it marks the level complete and gives the gold reward through
 * ProfileManager. It waits a short moment then loads the menu.
 * MenuController sees ReturnToLevelSelect and opens Level Select instead of the main menu.
 */

public class BattleEndHandler : MonoBehaviour
{
    [SerializeField] private string _menuSceneName = "MainMenu_V2";
    [SerializeField] private float _returnDelay = 2f;

    private bool _ending;

    private void OnEnable() => MatchController.OnMatchEnded += HandleMatchEnded;
    private void OnDisable() => MatchController.OnMatchEnded -= HandleMatchEnded;

    private void HandleMatchEnded(Player winner)
    {
        if (_ending) return;
        _ending = true;

        bool playerWon = winner != null && !winner.Data.IsAI;
        ProfileManager profileManager = ProfileManager.Instance;

        if (profileManager == null)
        {
            Debug.LogWarning("BattleEndHandler: no ProfileManager (scene was started directly), so no rewards were given.");
        }
        else
        {
            if (playerWon)
            {
                if (profileManager.CurrentLevel == null)
                    Debug.LogWarning("BattleEndHandler: no current level set, so nothing was marked complete.");
                else
                    profileManager.CompleteCurrentLevel();
            }
            profileManager.ReturnToLevelSelect = true;
        }

        Debug.Log($"BattleEndHandler: {(playerWon ? "player won" : "player lost")}, returning to Level Select.");
        StartCoroutine(ReturnToMenu());
    }

    private IEnumerator ReturnToMenu()
    {
        yield return new WaitForSecondsRealtime(_returnDelay);

        if (!Application.CanStreamedLevelBeLoaded(_menuSceneName))
        {
            Debug.LogError($"BattleEndHandler: scene '{_menuSceneName}' can't be loaded. Is it in the Build Profiles scene list?");
            yield break;
        }
        SceneManager.LoadScene(_menuSceneName);
    }
}
