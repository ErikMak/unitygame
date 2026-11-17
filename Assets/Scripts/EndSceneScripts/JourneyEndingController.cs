using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static HanseaticDocument;

public sealed class JourneyEndingController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mapSceneName = "MapScene";
    private HanseaticDocument document;

    private void Start()
    {
        document = new HanseaticDocument(transform, "ServicePanel", 20);
        VisualElement root = document.Root;
        Show(root.Q("body"), false);
        VisualElement actions = root.Q("actions");
        actions.Add(ServiceAction("Новая игра", RestartGame));
        actions.Add(ServiceAction("Главное меню", ReturnToMainMenu));
        actions.Add(ServiceAction("Выйти", QuitGame));
        DisplayEnding();
    }

    private void OnDestroy() => document?.Dispose();

    public void DisplayEnding()
    {
        if (document == null) return;
        GameState state = GameState.Instance;
        bool available = state != null && state.IsNewGameInitialized;
        bool success = available && state.IsGameFinished && state.Gold >= state.RequiredGoldForSuccess &&
            state.DaysElapsed <= state.MaximumDaysForSuccess;
        document.Root.Q<Label>("title").text = success ? "Путешествие завершено" : "Путешествие окончено";
        document.Root.Q<Label>("summary").text = available
            ? $"Золото: {state.StartingGold} → {state.Gold}\nДней в пути: {state.DaysElapsed}"
            : "Нет данных о путешествии.";
    }

    public void RestartGame()
    {
        GameState.Instance?.StartNewGame();
        SceneManager.LoadScene(mapSceneName);
    }

    public void ReturnToMainMenu() => SceneManager.LoadScene(mainMenuSceneName);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
