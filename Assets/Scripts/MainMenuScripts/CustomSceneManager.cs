using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CustomSceneManager : MonoBehaviour
{
    [Header("Main Menu Containers")]
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject settingsContainer;

    [Header("Scene Transition")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [SerializeField, Min(0.01f)]
    private float fadeDuration = 0.6f;

    private bool isSceneTransitionRunning;

    private void Awake()
    {
        ShowMainMenu();
        ResetFadeOverlay();
    }

    private void ResetFadeOverlay()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.interactable = false;
        fadeCanvasGroup.blocksRaycasts = false;
    }
    /// <summary>
    /// Открывает меню настроек.
    /// </summary>
    public void ShowSettings()
    {
        SetContainerState(
            showMainContainer: false,
            showSettingsContainer: true);
    }

    /// <summary>
    /// Закрывает настройки и возвращает главное меню.
    /// </summary>
    public void ShowMainMenu()
    {
        SetContainerState(
            showMainContainer: true,
            showSettingsContainer: false);
    }

    private void SetContainerState(
        bool showMainContainer,
        bool showSettingsContainer)
    {
        if (mainContainer != null)
        {
            mainContainer.SetActive(showMainContainer);
        }
        else
        {
            Debug.LogWarning(
                "SceneManager: MainContainer не назначен в Inspector.");
        }

        if (settingsContainer != null)
        {
            settingsContainer.SetActive(showSettingsContainer);
        }
        else
        {
            Debug.LogWarning(
                "SceneManager: SettingsContainer не назначен в Inspector.");
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (isSceneTransitionRunning)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "SceneManager: имя загружаемой сцены не указано.");
            return;
        }

        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        isSceneTransitionRunning = true;

        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning(
                "SceneManager: Fade Canvas Group не назначен. " +
                "Сцена будет загружена без затемнения.");

            yield return LoadSceneAsync(sceneName);
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.interactable = true;
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsedTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsedTime / fadeDuration);

            fadeCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                1f,
                progress);

            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        yield return LoadSceneAsync(sceneName);
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation loadingOperation =
            SceneManager.LoadSceneAsync(sceneName);

        if (loadingOperation == null)
        {
            Debug.LogError(
                $"SceneManager: не удалось начать загрузку " +
                $"сцены \"{sceneName}\".");

            isSceneTransitionRunning = false;
            yield break;
        }

        while (!loadingOperation.isDone)
            yield return null;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadMapScene()
    {
       SceneManager.LoadScene("MapScene");
    }
}