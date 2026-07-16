using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class JourneyEndingController : MonoBehaviour
{
    [SerializeField] private TMP_Text resultTitleText;
    [SerializeField] private TMP_Text narrativeText;
    [SerializeField] private TMP_Text startingGoldValueText;
    [SerializeField] private TMP_Text finalGoldValueText;
    [SerializeField] private TMP_Text daysValueText;
    [SerializeField] private TMP_Text routeValueText;
    [SerializeField] private TMP_Text locationValueText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mapSceneName = "MapScene";

    private void Start()
    {
        WireButton(restartButton, RestartGame);
        WireButton(mainMenuButton, ReturnToMainMenu);
        WireButton(quitButton, QuitGame);
        DisplayEnding();
    }

    public void DisplayEnding()
    {
        GameState state = GameState.Instance;
        if (state == null || !state.IsNewGameInitialized)
        {
            SetText(resultTitleText, "Нет данных о путешествии");
            SetText(narrativeText, "Финальная сцена открыта без активного игрового состояния.");
            return;
        }

        bool success = state.IsGameFinished &&
                       state.Gold >= state.RequiredGoldForSuccess &&
                       state.DaysElapsed <= state.MaximumDaysForSuccess;

        SetText(resultTitleText, success ? "Путешествие завершено" : "Путешествие окончено");
        SetText(narrativeText, success
            ? "Ладья вернулась к Балтике в срок и с достаточной выручкой."
            : "Ладья вернулась к Балтике, но условия торгового поручения выполнены не полностью.");
        SetText(startingGoldValueText, state.StartingGold.ToString());
        SetText(finalGoldValueText, state.Gold.ToString());
        SetText(daysValueText, state.DaysElapsed.ToString());
        SetText(routeValueText, $"{state.CurrentJourneyStepIndex + 1} / {state.Route?.Steps?.Count ?? 0}");
        SetText(locationValueText, state.CurrentLocation.ToString());
    }

    public void RestartGame()
    {
        GameState.Instance?.StartNewGame();
        SceneManager.LoadScene(mapSceneName);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }
}
