using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("Parts")]
    [SerializeField] CanvasGroup group;
    [SerializeField] Slider progressBar;
    [SerializeField] TMP_Text titleText;
    [SerializeField] Image artImage;

    [Header("Settings")]
    [SerializeField] string defaultTitle = "Loading...";
    [SerializeField] float fadeTime = 0.3f;
    [SerializeField] float minimumTime = 1f;

    bool loading;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        group.alpha = 0f;
        group.blocksRaycasts = false;
    }

    public static void Load(string sceneName, string title = null, Sprite art = null)
    {
        if (Instance == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }
        if (Instance.loading) return;
        Instance.StartCoroutine(Instance.LoadRoutine(sceneName, title, art));
    }

    IEnumerator LoadRoutine(string sceneName, string title, Sprite art)
    {
        loading = true;

        if (titleText != null) titleText.text = string.IsNullOrEmpty(title) ? defaultTitle : title;
        if (artImage != null)
        {
            artImage.sprite = art;
            artImage.enabled = art != null;
        }
        if (progressBar != null) progressBar.value = 0f;

        group.blocksRaycasts = true;
        yield return Fade(1f);

        float startTime = Time.unscaledTime;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogError($"LoadingScreen: scene '{sceneName}' can't be loaded. Is it in the Build Profiles scene list?");
            yield return Fade(0f);
            group.blocksRaycasts = false;
            loading = false;
            yield break;
        }
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f || Time.unscaledTime - startTime < minimumTime)
        {
            float loaded = Mathf.Clamp01(operation.progress / 0.9f);
            float waited = minimumTime > 0f ? Mathf.Clamp01((Time.unscaledTime - startTime) / minimumTime) : 1f;
            if (progressBar != null) progressBar.value = Mathf.Min(loaded, waited);
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;

        yield return Fade(0f);
        group.blocksRaycasts = false;
        loading = false;
    }

    IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float time = 0f;
        while (time < fadeTime)
        {
            time += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, time / fadeTime);
            yield return null;
        }
        group.alpha = target;
    }
}
