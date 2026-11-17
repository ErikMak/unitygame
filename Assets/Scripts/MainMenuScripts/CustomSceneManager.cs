using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CustomSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject mainContainer;
    [SerializeField] private GameObject settingsContainer;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.6f;

    private HanseaticDocument document;
    private HanseaticMenuView view;
    private bool loading;

    private void Awake()
    {
        // Existing settings components still read values from serialized controls during migration.
        Transform legacyCanvas = mainContainer != null ? mainContainer.transform.parent : null;
        if (legacyCanvas != null)
            legacyCanvas.gameObject.SetActive(false);
        if (settingsContainer != null)
            settingsContainer.SetActive(false);

        document = new HanseaticDocument(transform, "MainMenu", 0);
        view = new HanseaticMenuView(document.Root, this,
            FindAnyObjectByType<GraphicsSettingsUI>(FindObjectsInactive.Include),
            FindAnyObjectByType<AudioSettingsUI>(FindObjectsInactive.Include));
        ShowMainMenu();
    }

    private void OnDestroy() => document?.Dispose();
    public void ShowSettings() => view?.SetPage(true);
    public void ShowMainMenu() => view?.SetPage(false);
    public void LoadMapScene() => LoadSceneWithFade("MapScene");

    public void LoadSceneWithFade(string sceneName)
    {
        if (loading || string.IsNullOrWhiteSpace(sceneName))
            return;
        loading = true;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        VisualElement fade = view.Fade;
        document.Root.Q("menu").SetEnabled(false);
        document.Root.Q("settings").SetEnabled(false);
        HanseaticDocument.Show(fade, true);
        for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
        {
            fade.style.opacity = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        fade.style.opacity = 1f;
        yield return SceneManager.LoadSceneAsync(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
