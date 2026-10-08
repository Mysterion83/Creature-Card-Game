using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start() => ShowMain();
       
    public void ShowMain() => Show(mainPanel);
    public void ShowLevelSelect() => Show(levelSelectPanel);
    public void ShowSettings() => Show(settingsPanel);

    public void LoadLevel(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' can't be loaded.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // stop Play mode
#else
        Application.Quit();
#endif
    }

    private void Show(GameObject panel)
    {
        mainPanel.SetActive(panel == mainPanel);
        levelSelectPanel.SetActive(panel == levelSelectPanel);
        settingsPanel.SetActive(panel == settingsPanel);
    }
}