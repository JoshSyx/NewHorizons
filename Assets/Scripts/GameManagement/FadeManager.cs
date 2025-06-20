using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instance;
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 0.5f;
    private bool isFading = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
                if (fadeCanvasGroup == null)
                {
                    Debug.LogError("FadeManager: No CanvasGroup found in children.");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f; // Ensure time is running in new scene

        if (fadeCanvasGroup == null)
        {
            fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        StartCoroutine(Fade(1, 0)); // Fade in from black
    }

    public void LoadScene(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoad(sceneName));
        }
    }

    public void ReloadScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(SceneManager.GetSceneByBuildIndex(nextIndex).name);
        }
        else
        {
            Debug.LogWarning("FadeManager: No next scene in build settings.");
        }
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isFading = true;
        yield return StartCoroutine(Fade(0, 1)); // Fade to black
        SceneManager.LoadScene(sceneName);
        // Fade back in happens in OnSceneLoaded
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("FadeManager: fadeCanvasGroup is null.");
            yield break;
        }

        float elapsed = 0f;
        fadeCanvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
        fadeCanvasGroup.blocksRaycasts = endAlpha != 0;
        isFading = false;
    }
}
