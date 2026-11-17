using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public sealed class CityHubController : MonoBehaviour
{
    [SerializeField] private string mapSceneName = "MapScene";
    [SerializeField] private List<CityDefinition> cityDefinitions = new();
    private Button cityNameText;
    private Label dateText;
    private Label statusText;
    private Button germanCourtButton;
    private Button marketButton;
    private Button churchButton;
    private Button portButton;
    private Button inventoryButton;
    private Button leaveCityButton;
    [SerializeField] private GameObject marketPanel;
    [SerializeField] private InventoryPanelUI inventoryPanel;
    [SerializeField] private EventController eventController;

    private GameState gameState;
    private HanseaticHudView hud;
    private CityDefinition city;
    private bool isLoadingScene;

    private void Start()
    {
        BuildHud();
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

        BuildCityWorld();
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
            inventoryButton.SetEnabled(!eventOpen && !marketOpen);
        }

        leaveCityButton.SetEnabled(!eventOpen && !marketOpen && !dragging);
        SetSightButtonsInteractable(!eventOpen && !marketOpen);
    }

    public void LeaveCity()
    {
        if (isLoadingScene || gameState == null || !CanLeaveCity())
        {
            return;
        }

        isLoadingScene = true;
        leaveCityButton.SetEnabled(false);
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
        marketPanel.transform.SetAsLastSibling();
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

    private void BuildCityWorld()
    {
        Camera camera = Camera.main;
        if (city.WorldModel == null || camera == null)
            return;

        GameObject model = Instantiate(city.WorldModel, Vector3.zero, Quaternion.identity);
        model.name = city.DisplayName;
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
            bounds.Encapsulate(renderer.bounds);

        float extent = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        Vector3 direction = new Vector3(1, 1.08f, -1.4f).normalized;
        Vector3 target = bounds.center;
        camera.transform.SetPositionAndRotation(target + direction * extent * 2.8f, Quaternion.LookRotation(-direction));
        camera.orthographic = true;
        camera.orthographicSize = extent * 0.68f;
        camera.nearClipPlane = Mathf.Max(0.01f, extent * 0.01f);
        camera.farClipPlane = Mathf.Max(10, extent * 8);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.16f, 0.21f, 0.24f);
    }

    private void BuildHud()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            Transform canvas = root.name == "Canvas" ? root.transform : root.transform.Find("Canvas");
            Transform sights = canvas != null ? canvas.Find("CitySightsContainer") : null;
            if (sights != null) sights.gameObject.SetActive(false);
            foreach (Transform node in canvas != null ? canvas : root.transform)
            {
                if (node.name is "CityNameText" or "DateText" or "CityStatusText")
                    node.gameObject.SetActive(false);
            }
        }

        hud = new HanseaticHudView(transform, city: true);
        cityNameText = hud.Location;
        dateText = hud.Time;
        statusText = hud.Status;
        germanCourtButton = hud.CityAction("Немецкий двор");
        marketButton = hud.CityAction("Рынок");
        churchButton = hud.CityAction("Церковь");
        portButton = hud.CityAction("Порт");
        inventoryButton = hud.Inventory;
        leaveCityButton = hud.Leave;
    }

    private void OnDestroy() => hud?.Dispose();

    private void WireButtons()
    {
        germanCourtButton.clicked += OpenGermanCourt;
        marketButton.clicked += OpenMarket;
        churchButton.clicked += OpenChurch;
        portButton.clicked += OpenInventory;
        inventoryButton.clicked += OpenInventory;
        leaveCityButton.clicked += LeaveCity;
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
        if (inventoryButton != null) inventoryButton.SetEnabled(value);
        if (leaveCityButton != null) leaveCityButton.SetEnabled(value);
    }

    private void SetSightButtonsInteractable(bool value)
    {
        if (germanCourtButton != null) germanCourtButton.SetEnabled(value);
        if (marketButton != null) marketButton.SetEnabled(value);
        if (churchButton != null) churchButton.SetEnabled(value);
        if (portButton != null) portButton.SetEnabled(value);
    }
}
