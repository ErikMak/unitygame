using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public sealed class GameHUD : MonoBehaviour
{
    [SerializeField] private InventoryPanelUI inventoryPanel;
    [SerializeField] private EventController eventController;

    private GameState gameState;
    private HanseaticHudView hud;

    private void Awake()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        eventController ??= FindAnyObjectByType<EventController>();

        Transform oldBar = transform.Find("TopBar");
        if (oldBar != null)
            oldBar.gameObject.SetActive(false);
        if (gameObject.scene.name != "MapScene")
            return;

        hud = new HanseaticHudView(transform, city: false);
        hud.Inventory.clicked += ToggleInventory;

        if (inventoryPanel != null)
        {
            inventoryPanel.Hide();
        }
    }

    private void OnDestroy() => hud?.Dispose();

    private void OnEnable()
    {
        if (hud == null)
            return;

        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        if (hud != null)
            Unsubscribe();
    }

    private void Update()
    {
        if (hud == null)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.iKey.wasPressedThisFrame && !IsEventOpen() && !hud.IsMenuOpen)
        {
            ToggleInventory();
        }

        if (keyboard.escapeKey.wasPressedThisFrame && inventoryPanel != null && inventoryPanel.IsOpen)
        {
            inventoryPanel.Hide();
        }
    }

    private void Subscribe()
    {
        if (gameState == null)
        {
            return;
        }

        gameState.OnGoldChanged += HandleGoldChanged;
        gameState.OnDaysChanged += HandleDaysChanged;
        gameState.OnInventoryChanged += RefreshSummary;
        gameState.OnLocationChanged += HandleLocationChanged;
        gameState.OnJourneyStepChanged += HandleJourneyStepChanged;
    }

    private void Unsubscribe()
    {
        if (gameState == null)
        {
            return;
        }

        gameState.OnGoldChanged -= HandleGoldChanged;
        gameState.OnDaysChanged -= HandleDaysChanged;
        gameState.OnInventoryChanged -= RefreshSummary;
        gameState.OnLocationChanged -= HandleLocationChanged;
        gameState.OnJourneyStepChanged -= HandleJourneyStepChanged;
    }

    public void RefreshAll()
    {
        RefreshJourney();
        RefreshSummary();
    }

    private void HandleGoldChanged(int value) => RefreshSummary();
    private void HandleDaysChanged(int value) => RefreshSummary();
    private void HandleLocationChanged(LocationId location) => RefreshJourney();
    private void HandleJourneyStepChanged(int index, JourneyStep step) => RefreshJourney();

    private void RefreshSummary()
    {
        if (hud == null || gameState == null) return;
        hud.Time.text = $"День {gameState.DaysElapsed} из {gameState.MaximumDaysForSuccess}";
        hud.Summary.text = $"Золото: {gameState.Gold}\nГруз: {gameState.GetOccupiedSlotCount()} / {gameState.InventorySlotCount}\nЦель: {gameState.RequiredGoldForSuccess} золота";
    }

    private void RefreshJourney()
    {
        JourneyStep step = gameState != null ? gameState.CurrentJourneyStep : null;
        if (hud == null || step == null) return;
        hud.Location.text = step.StepType switch
        {
            JourneyStepType.Travel => $"{GetLocationName(step.StartPoint)} → {GetLocationName(step.EndPoint)}",
            JourneyStepType.Location => GetLocationName(step.DestinationLocation),
            JourneyStepType.Ending => "Возвращение к Балтике",
            _ => step.DisplayName
        };
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null || IsEventOpen())
        {
            return;
        }

        inventoryPanel.SetVisible(!inventoryPanel.IsOpen);
    }

    private bool IsEventOpen()
    {
        eventController ??= FindAnyObjectByType<EventController>();
        return eventController != null && eventController.IsEventOpen;
    }

    private static string GetLocationName(MapPointId id)
    {
        return id switch
        {
            MapPointId.NevaMouth => "Устье Невы",
            MapPointId.Oreshek => "Орешек",
            MapPointId.Ladoga => "Старая Ладога",
            MapPointId.Novgorod => "Великий Новгород",
            MapPointId.Torzhok => "Торжок",
            MapPointId.BalticReturn => "Балтика",
            _ => id.ToString()
        };
    }

    private static string GetLocationName(LocationId id)
    {
        return id switch
        {
            LocationId.NevaMouth => "Устье Невы",
            LocationId.Oreshek => "Орешек",
            LocationId.Ladoga => "Старая Ладога",
            LocationId.Novgorod => "Великий Новгород",
            LocationId.Torzhok => "Торжок",
            LocationId.BalticReturn => "Балтика",
            _ => id.ToString()
        };
    }
}
