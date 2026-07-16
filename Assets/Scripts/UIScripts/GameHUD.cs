using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text goldValue;
    [SerializeField] private TMP_Text daysValue;
    [SerializeField] private TMP_Text journeyStatusText;
    [SerializeField] private Image journeyProgressFill;
    [SerializeField] private TMP_Text goalValue;
    [SerializeField] private TMP_Text cargoValue;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private InventoryPanelUI inventoryPanel;
    [SerializeField] private EventController eventController;

    private GameState gameState;

    private void Awake()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        eventController ??= FindAnyObjectByType<EventController>();

        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveAllListeners();
            inventoryButton.onClick.AddListener(ToggleInventory);
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.Hide();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.iKey.wasPressedThisFrame && !IsEventOpen())
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
        gameState.OnInventoryChanged += RefreshCargo;
        gameState.OnLocationChanged += HandleLocationChanged;
        gameState.OnJourneyStepChanged += HandleJourneyStepChanged;
        gameState.OnJourneyProgressChanged += RefreshProgress;
    }

    private void Unsubscribe()
    {
        if (gameState == null)
        {
            return;
        }

        gameState.OnGoldChanged -= HandleGoldChanged;
        gameState.OnDaysChanged -= HandleDaysChanged;
        gameState.OnInventoryChanged -= RefreshCargo;
        gameState.OnLocationChanged -= HandleLocationChanged;
        gameState.OnJourneyStepChanged -= HandleJourneyStepChanged;
        gameState.OnJourneyProgressChanged -= RefreshProgress;
    }

    public void RefreshAll()
    {
        RefreshGold();
        RefreshDays();
        RefreshGoal();
        RefreshCargo();
        RefreshJourney();
        RefreshProgress(gameState != null && gameState.CurrentJourneyStep?.StepType == JourneyStepType.Travel ? 0f : 1f);
    }

    private void RefreshGold()
    {
        if (goldValue != null && gameState != null)
        {
            goldValue.text = $"Золото: {gameState.Gold}";
        }
    }

    private void HandleGoldChanged(int value)
    {
        RefreshGold();
    }

    private void HandleDaysChanged(int value)
    {
        RefreshDays();
    }

    private void HandleLocationChanged(LocationId location)
    {
        RefreshJourney();
    }

    private void HandleJourneyStepChanged(int index, JourneyStep step)
    {
        RefreshJourney();
    }

    private void RefreshDays()
    {
        if (daysValue != null && gameState != null)
        {
            daysValue.text = $"День: {gameState.DaysElapsed} / {gameState.MaximumDaysForSuccess}";
        }
    }

    private void RefreshGoal()
    {
        if (goalValue != null && gameState != null)
        {
            goalValue.text = $"Цель: {gameState.RequiredGoldForSuccess} золота";
        }
    }

    private void RefreshCargo()
    {
        if (cargoValue != null && gameState != null)
        {
            cargoValue.text = $"Груз: {gameState.GetOccupiedSlotCount()} / {gameState.InventorySlotCount}";
        }
    }

    private void RefreshJourney()
    {
        if (journeyStatusText == null || gameState == null || gameState.CurrentJourneyStep == null)
        {
            return;
        }

        JourneyStep step = gameState.CurrentJourneyStep;
        journeyStatusText.text = step.StepType switch
        {
            JourneyStepType.Travel => $"{GetLocationName(step.StartPoint)} -> {GetLocationName(step.EndPoint)}",
            JourneyStepType.Event => $"Событие: {step.DisplayName}",
            JourneyStepType.Location => $"Локация: {GetLocationName(step.DestinationLocation)}",
            JourneyStepType.Ending => "Возвращение к Балтике",
            _ => step.DisplayName
        };
    }

    private void RefreshProgress(float progress)
    {
        if (journeyProgressFill != null)
        {
            journeyProgressFill.fillAmount = Mathf.Clamp01(progress);
        }
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
