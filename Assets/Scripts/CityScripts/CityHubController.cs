using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CityHubController : MonoBehaviour
{
    [SerializeField] private string mapSceneName = "MapScene";
    [SerializeField] private List<CityDefinition> cityDefinitions = new();
    [SerializeField] private TMP_Text cityNameText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button germanCourtButton;
    [SerializeField] private Button marketButton;
    [SerializeField] private Button churchButton;
    [SerializeField] private Button portButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button leaveCityButton;
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private InventoryPanelUI inventoryPanel;
    [SerializeField] private EventController eventController;

    private GameState gameState;
    private CityDefinition city;
    private bool isLoadingScene;

    private void Start()
    {
        gameState = GameState.Instance;
        if (!ValidateEntry())
        {
            SetButtonsInteractable(false);
            return;
        }

        city = cityDefinitions.Find(item => item != null && item.Location == gameState.CurrentLocation);
        if (city == null)
        {
            Debug.LogError($"{nameof(CityHubController)}: no city definition for {gameState.CurrentLocation}.", this);
            SetButtonsInteractable(false);
            return;
        }

        WireButtons();
        SetButtonsInteractable(true);
        RefreshHeader();
        ClosePanels();
    }

    private void Update()
    {
        if (leaveCityButton == null || gameState == null || isLoadingScene)
        {
            return;
        }

        bool eventOpen = eventController != null && eventController.IsEventOpen;
        bool marketOpen = marketPanel != null && marketPanel.activeSelf;
        bool dragging = inventoryPanel != null && inventoryPanel.IsDragging;
        if (inventoryButton != null)
        {
            inventoryButton.interactable = !eventOpen && !marketOpen;
        }

        leaveCityButton.interactable = !eventOpen && !marketOpen && !dragging;
        SetSightButtonsInteractable(!eventOpen && !marketOpen);
    }

    public void LeaveCity()
    {
        if (isLoadingScene || gameState == null || !CanLeaveCity())
        {
            return;
        }

        isLoadingScene = true;
        leaveCityButton.interactable = false;
        ClosePanels();

        if (!gameState.TryCompleteCurrentLocationStep())
        {
            isLoadingScene = false;
            Debug.LogError($"{nameof(CityHubController)}: current step is not a Location step.", this);
            return;
        }

        SceneManager.LoadScene(mapSceneName);
    }

    public void OpenGermanCourt()
    {
        if (!CanOpenPanel() || eventController == null)
        {
            return;
        }

        int visits = gameState.GetCityVisitCount(gameState.CurrentLocation);
        OpenCityEvent(visits > 1 ? city.ReturnVisitEvent : city.FirstVisitEvent);
    }

    public void OpenChurch()
    {
        if (!CanOpenPanel() || city.ChurchEvent == null)
        {
            return;
        }

        if (gameState.HasCompletedEvent(city.ChurchEvent.EventId))
        {
            SetStatus("Вы уже посетили это место.");
            return;
        }

        OpenCityEvent(city.ChurchEvent);
    }

    public void OpenMarket()
    {
        if (!CanOpenPanel() || marketPanel == null)
        {
            return;
        }

        ClosePanels();
        marketPanel.SetActive(true);
    }

    public void OpenInventory()
    {
        if (!CanOpenPanel() || inventoryPanel == null)
        {
            return;
        }

        ClosePanels();
        inventoryPanel.SetVisible(true);
    }

    public void ClosePanels()
    {
        marketPanel?.SetActive(false);
        inventoryPanel?.Hide();
    }

    private bool ValidateEntry()
    {
        if (gameState == null || !gameState.IsNewGameInitialized || !gameState.HasCurrentJourneyStep)
        {
            Debug.LogError($"{nameof(CityHubController)}: persistent initialized GameState is required.", this);
            return false;
        }

        JourneyStep step = gameState.CurrentJourneyStep;
        if (step.StepType != JourneyStepType.Location || step.DestinationLocation != gameState.CurrentLocation)
        {
            Debug.LogError($"{nameof(CityHubController)}: current route step is not the active city Location.", this);
            return false;
        }

        return gameState.Inventory != null;
    }

    private void WireButtons()
    {
        Wire(germanCourtButton, OpenGermanCourt);
        Wire(marketButton, OpenMarket);
        Wire(churchButton, OpenChurch);
        Wire(portButton, OpenInventory);
        Wire(inventoryButton, OpenInventory);
        Wire(leaveCityButton, LeaveCity);
    }

    private static void Wire(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void OpenCityEvent(EventDefinition definition)
    {
        if (definition == null)
        {
            SetStatus("Для этой точки пока нет события.");
            return;
        }

        if (definition.IsOneShot && gameState.HasCompletedEvent(definition.EventId))
        {
            SetStatus("Вы уже посетили это место.");
            return;
        }

        ClosePanels();
        eventController.OpenStandaloneEvent(definition, RefreshHeader);
    }

    private bool CanOpenPanel()
    {
        return !isLoadingScene && (eventController == null || !eventController.IsEventOpen);
    }

    private bool CanLeaveCity()
    {
        return (eventController == null || !eventController.IsEventOpen) &&
               (marketPanel == null || !marketPanel.activeSelf) &&
               (inventoryPanel == null || !inventoryPanel.IsDragging);
    }

    private void RefreshHeader()
    {
        if (cityNameText != null)
        {
            cityNameText.text = city.DisplayName;
        }

        if (dateText != null)
        {
            dateText.text = gameState.CurrentDate.ToString("dd MM yyyy");
        }
    }

    private void SetStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
        else
        {
            Debug.Log(text, this);
        }
    }

    private void SetButtonsInteractable(bool value)
    {
        SetSightButtonsInteractable(value);
        if (inventoryButton != null) inventoryButton.interactable = value;
        if (leaveCityButton != null) leaveCityButton.interactable = value;
    }

    private void SetSightButtonsInteractable(bool value)
    {
        if (germanCourtButton != null) germanCourtButton.interactable = value;
        if (marketButton != null) marketButton.interactable = value;
        if (churchButton != null) churchButton.interactable = value;
        if (portButton != null) portButton.interactable = value;
    }
}
