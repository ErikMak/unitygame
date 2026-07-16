using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class EndingScreenController : MonoBehaviour
{
    /*
    [Header("Result Rules")]
    [Tooltip("Минимальное количество золота для успешного финала.")]
    [SerializeField, Min(0)]
    private int minimumGoldForSuccess = 0;

    [Tooltip("Учитывать ли количество дней при определении результата.")]
    [SerializeField]
    private bool useDaysLimit = false;

    [Tooltip("Максимальное количество дней для успешного финала.")]
    [SerializeField, Min(0)]
    private int maximumDaysForSuccess = 30;

    [Header("Result Text")]
    [SerializeField]
    private TMP_Text resultTitleText;

    [SerializeField]
    private TMP_Text narrativeText;

    [SerializeField]
    private string successTitle = "Путешествие завершено";

    [SerializeField]
    private string failureTitle = "Поручение не выполнено";

    [SerializeField, TextArea(3, 8)]
    private string successNarrative =
        "Ладья вернулась к берегам Балтики до окончания навигации. " +
        "Торговое поручение исполнено, а полученная прибыль доказала, " +
        "что ученик достоин доверия Немецкого двора.";

    [SerializeField, TextArea(3, 8)]
    private string failureNarrative =
        "Путь подошёл к концу, но поручение не принесло ожидаемого результата. " +
        "Недостаток золота или слишком долгая дорога не позволили завершить " +
        "торговую экспедицию успешно.";

    [Header("Statistics Values")]
    [SerializeField]
    private TMP_Text startingGoldValueText;

    [SerializeField]
    private TMP_Text finalGoldValueText;

    [SerializeField]
    private TMP_Text profitValueText;

    [SerializeField]
    private TMP_Text daysValueText;

    [SerializeField]
    private TMP_Text routeValueText;

    [SerializeField]
    private TMP_Text locationValueText;

    [SerializeField]
    private TMP_Text resultValueText;

    [Header("Optional Result Objects")]
    [SerializeField]
    private GameObject successDecoration;

    [SerializeField]
    private GameObject failureDecoration;

    [Header("Scene Names")]
    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    [SerializeField]
    private string mapSceneName = "MapScene";

    private void Start()
    {
        DisplayEnding();
    }

    public void DisplayEnding()
    {
        GameState gameState = GameState.Instance;

        if (gameState == null)
        {
            ShowMissingData(
                "GameState не найден. Финальная сцена была запущена без игрового состояния.");

            return;
        }

        if (!gameState.IsNewGameInitialized)
        {
            ShowMissingData(
                "Новое прохождение не было инициализировано.");

            return;
        }

        int totalRouteSteps = GetTotalRouteSteps(gameState);

        int completedRouteSteps = GetCompletedRouteSteps(
            gameState,
            totalRouteSteps);

        int profit = gameState.Gold - gameState.StartingGold;

        bool isSuccess = CalculateSuccess(
            gameState,
            totalRouteSteps);

        ApplyResultText(isSuccess);
        ApplyStatistics(
            gameState,
            completedRouteSteps,
            totalRouteSteps,
            profit,
            isSuccess);

        SetResultDecorations(isSuccess);
    }

    private bool CalculateSuccess(
        GameState gameState,
        int totalRouteSteps)
    {
        bool routeCompleted =
            totalRouteSteps <= 0 ||
            gameState.CurrentJourneyStepIndex >= totalRouteSteps - 1;

        bool hasEnoughGold =
            gameState.Gold >= minimumGoldForSuccess;

        bool completedInTime =
            !useDaysLimit ||
            gameState.DaysElapsed <= maximumDaysForSuccess;

        return
            gameState.IsGameFinished &&
            routeCompleted &&
            hasEnoughGold &&
            completedInTime;
    }

    private int GetTotalRouteSteps(GameState gameState)
    {
        if (gameState.Route == null ||
            gameState.Route.Steps == null)
        {
            return 0;
        }

        return gameState.Route.Steps.Count;
    }

    private int GetCompletedRouteSteps(
        GameState gameState,
        int totalRouteSteps)
    {
        if (totalRouteSteps <= 0)
            return 0;

        return Mathf.Clamp(
            gameState.CurrentJourneyStepIndex + 1,
            0,
            totalRouteSteps);
    }

    private void ApplyResultText(bool isSuccess)
    {
        if (resultTitleText != null)
        {
            resultTitleText.text =
                isSuccess ? successTitle : failureTitle;
        }

        if (narrativeText != null)
        {
            narrativeText.text =
                isSuccess
                    ? successNarrative
                    : failureNarrative;
        }
    }

    private void ApplyStatistics(
        GameState gameState,
        int completedRouteSteps,
        int totalRouteSteps,
        int profit,
        bool isSuccess)
    {
        SetText(
            startingGoldValueText,
            gameState.StartingGold.ToString());

        SetText(
            finalGoldValueText,
            gameState.Gold.ToString());

        SetText(
            profitValueText,
            FormatSignedNumber(profit));

        SetText(
            daysValueText,
            gameState.DaysElapsed.ToString());

        string routeProgress = totalRouteSteps > 0
            ? $"{completedRouteSteps} / {totalRouteSteps}"
            : "Нет данных";

        SetText(
            routeValueText,
            routeProgress);

        SetText(
            locationValueText,
            gameState.CurrentLocation.ToString());

        SetText(
            resultValueText,
            isSuccess ? "Успех" : "Неудача");
    }

    private void SetResultDecorations(bool isSuccess)
    {
        if (successDecoration != null)
            successDecoration.SetActive(isSuccess);

        if (failureDecoration != null)
            failureDecoration.SetActive(!isSuccess);
    }

    private void ShowMissingData(string reason)
    {
        SetText(
            resultTitleText,
            "Нет данных о прохождении");

        SetText(
            narrativeText,
            reason);

        SetText(startingGoldValueText, "—");
        SetText(finalGoldValueText, "—");
        SetText(profitValueText, "—");
        SetText(daysValueText, "—");
        SetText(routeValueText, "—");
        SetText(locationValueText, "—");
        SetText(resultValueText, "—");

        if (successDecoration != null)
            successDecoration.SetActive(false);

        if (failureDecoration != null)
            failureDecoration.SetActive(false);

        Debug.LogWarning(
            $"EndingScreenController: {reason}");
    }

    private static void SetText(
        TMP_Text target,
        string value)
    {
        if (target != null)
            target.text = value;
    }

    private static string FormatSignedNumber(int value)
    {
        if (value > 0)
            return $"+{value}";

        return value.ToString();
    }

    public void RestartGame()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.StartNewGame();
        }

        UnitySceneManager.LoadScene(mapSceneName);
    }

    */

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuSceneName");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}