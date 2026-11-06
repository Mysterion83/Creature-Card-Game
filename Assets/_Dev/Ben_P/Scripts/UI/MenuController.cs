using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject levelSelectPanel;

    readonly Stack<GameObject> history = new();
    GameObject current;

    void Start()
    {       
        var pm = ProfileManager.Instance;
        if (pm != null && pm.ReturnToLevelSelect)
        {
            pm.ReturnToLevelSelect = false;
            OpenFresh(levelSelectPanel);
        }
        else
        {
            OpenFresh(mainPanel);
        }
    }

    // Navigation 
    public void Open(GameObject panel)
    {
        if (current != null)
        {
            current.SetActive(false);
            history.Push(current);
        }
        current = panel;
        current.SetActive(true);
    }

    public void OpenFresh(GameObject panel)   // forward step that wipes Back history
    {
        history.Clear();
        if (current != null) current.SetActive(false);
        current = panel;
        current.SetActive(true);
    }

    public void Back()
    {
        if (history.Count == 0) return;
        current.SetActive(false);
        current = history.Pop();
        current.SetActive(true);
    }

    public void ShowMain() => OpenFresh(mainPanel);

    // Scenes and quitting
    public void LoadLevel(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' can't be loaded. Is it in the Build Profiles scene list?");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}