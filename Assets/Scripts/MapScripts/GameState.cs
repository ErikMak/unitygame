using UnityEngine;
using System.Collections.Generic;
using System;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [SerializeField, Min(0)] private int startingGold = 100;
    [SerializeField] private GameRulesDefinition rules;
    [SerializeField] private JourneyRouteDefinition route;
    [SerializeField] private InventoryModel inventory = new();

    private int gold;
    private int currentJourneyStepIndex;
    private int daysElapsed;
    private LocationId currentLocation;
    private bool isNewGameInitialized;
    private bool isGameFinished;
    [SerializeField] private List<string> completedEventIds = new();
    private readonly HashSet<string> completedEventLookup = new();

    public event Action<int> OnGoldChanged;
    public event Action<int> OnDaysChanged;
    public event Action OnInventoryChanged;
    public event Action<LocationId> OnLocationChanged;
    public event Action<int, JourneyStep> OnJourneyStepChanged;
    public event Action<float> OnJourneyProgressChanged;

    public int StartingGold => rules != null ? rules.StartingGold : startingGold;
    public int Gold => gold;
    public int CurrentJourneyStepIndex => currentJourneyStepIndex;
    public int DaysElapsed => daysElapsed;
    public LocationId CurrentLocation => currentLocation;
    public bool IsNewGameInitialized => isNewGameInitialized;
    public bool IsGameFinished => isGameFinished;
    public JourneyRouteDefinition Route => route;
    public GameRulesDefinition Rules => rules;
    public InventoryModel Inventory => inventory;
    public int InventorySlotCount => rules != null ? rules.InventorySlotCount : 8;
    public int MaxStackPerSlot => rules != null ? rules.MaxStackPerSlot : 5;
    public int RequiredGoldForSuccess => rules != null ? rules.RequiredGoldForSuccess : 160;
    public int MaximumDaysForSuccess => rules != null ? rules.MaximumDaysForSuccess : 18;

    public bool HasCurrentJourneyStep =>
        route != null &&
        route.Steps != null &&
        currentJourneyStepIndex >= 0 &&
        currentJourneyStepIndex < route.Steps.Count;

    public JourneyStep CurrentJourneyStep =>
        HasCurrentJourneyStep ? route.Steps[currentJourneyStepIndex] : null;

    public bool IsLastJourneyStep =>
        HasCurrentJourneyStep && currentJourneyStepIndex == route.Steps.Count - 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }

            return;
        }

        Instance = this;

        if (Application.isPlaying)
        {
            DontDestroyOnLoad(gameObject);
        }

        ClampState();
        RebuildCompletedEventLookup();
    }

    private void OnValidate()
    {
        startingGold = Mathf.Max(0, startingGold);
        inventory ??= new InventoryModel();
        inventory.EnsureSlotCount(InventorySlotCount);
    }

    public void ConfigureRules(GameRulesDefinition rulesDefinition)
    {
        rules = rulesDefinition;
        ClampState();
    }

    public void ConfigureRoute(JourneyRouteDefinition routeDefinition)
    {
        route = routeDefinition;

        if (route == null || route.Steps == null || route.Steps.Count == 0)
        {
            currentJourneyStepIndex = 0;
            return;
        }

        currentJourneyStepIndex = Mathf.Clamp(currentJourneyStepIndex, 0, route.Steps.Count - 1);
    }

    public void StartNewGame()
    {
        gold = Mathf.Max(0, StartingGold);
        currentJourneyStepIndex = 0;
        daysElapsed = 0;
        currentLocation = LocationId.NevaMouth;
        isGameFinished = false;
        isNewGameInitialized = true;
        completedEventIds.Clear();
        completedEventLookup.Clear();
        inventory.EnsureSlotCount(InventorySlotCount);
        inventory.ClearInventory();

        ClampState();
        RaiseAllStateChanged();
    }

    public bool AdvanceJourneyStep()
    {
        if (route == null || route.Steps == null || route.Steps.Count == 0)
        {
            currentJourneyStepIndex = 0;
            return false;
        }

        if (currentJourneyStepIndex < 0)
        {
            currentJourneyStepIndex = 0;
            return true;
        }

        if (currentJourneyStepIndex >= route.Steps.Count - 1)
        {
            currentJourneyStepIndex = route.Steps.Count - 1;
            return false;
        }

        currentJourneyStepIndex++;
        OnJourneyStepChanged?.Invoke(currentJourneyStepIndex, CurrentJourneyStep);
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gold += amount;
        OnGoldChanged?.Invoke(gold);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || amount > gold)
        {
            return false;
        }

        gold -= amount;
        OnGoldChanged?.Invoke(gold);
        return true;
    }

    public void AddDays(int days)
    {
        if (days <= 0)
        {
            return;
        }

        daysElapsed += days;
        OnDaysChanged?.Invoke(daysElapsed);
    }

    public void SetCurrentLocation(LocationId location)
    {
        if (currentLocation == location)
        {
            return;
        }

        currentLocation = location;
        OnLocationChanged?.Invoke(currentLocation);
    }

    public void MarkGameFinished()
    {
        isGameFinished = true;
    }

    public bool TryAddItem(GoodItem item, int quantity)
    {
        InventoryService service = new(inventory);
        bool result = service.TryAddItem(item, quantity);
        if (result)
        {
            OnInventoryChanged?.Invoke();
        }

        return result;
    }

    public bool TryRemoveItem(GoodItem item, int quantity)
    {
        InventoryService service = new(inventory);
        bool result = service.TryRemoveItem(item, quantity);
        if (result)
        {
            OnInventoryChanged?.Invoke();
        }

        return result;
    }

    public bool TryRemoveAnyCargo(int quantity)
    {
        InventoryService service = new(inventory);
        bool result = service.TryRemoveAnyCargo(quantity);
        if (result && quantity > 0)
        {
            OnInventoryChanged?.Invoke();
        }

        return result;
    }

    public bool MoveOrMergeSlot(int sourceIndex, int targetIndex)
    {
        InventoryService service = new(inventory);
        bool result = service.MoveOrMergeSlot(sourceIndex, targetIndex);
        if (result)
        {
            OnInventoryChanged?.Invoke();
        }

        return result;
    }

    public int GetOccupiedSlotCount()
    {
        return new InventoryService(inventory).GetOccupiedSlotCount();
    }

    public bool HasItem(GoodItem item, int quantity)
    {
        return new InventoryService(inventory).HasItem(item, quantity);
    }

    public int GetItemQuantity(GoodItem item)
    {
        return new InventoryService(inventory).GetItemQuantity(item);
    }

    public void SetJourneyProgress(float progress)
    {
        OnJourneyProgressChanged?.Invoke(Mathf.Clamp01(progress));
    }

    public bool HasCompletedEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return false;
        }

        RebuildCompletedEventLookup();
        return completedEventLookup.Contains(eventId);
    }

    public void RegisterCompletedEvent(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
        {
            return;
        }

        RebuildCompletedEventLookup();
        if (!completedEventLookup.Add(eventId))
        {
            return;
        }

        completedEventIds.Add(eventId);
    }

    private void ClampState()
    {
        startingGold = Mathf.Max(0, startingGold);
        gold = Mathf.Max(0, gold);
        daysElapsed = Mathf.Max(0, daysElapsed);
        inventory ??= new InventoryModel();
        inventory.EnsureSlotCount(InventorySlotCount);

        if (route == null || route.Steps == null || route.Steps.Count == 0)
        {
            currentJourneyStepIndex = Mathf.Max(0, currentJourneyStepIndex);
            return;
        }

        currentJourneyStepIndex = Mathf.Clamp(currentJourneyStepIndex, 0, route.Steps.Count - 1);
    }

    private void RebuildCompletedEventLookup()
    {
        completedEventIds ??= new List<string>();
        completedEventLookup.Clear();

        for (int i = completedEventIds.Count - 1; i >= 0; i--)
        {
            string eventId = completedEventIds[i];
            if (string.IsNullOrWhiteSpace(eventId) || !completedEventLookup.Add(eventId))
            {
                completedEventIds.RemoveAt(i);
            }
        }
    }

    private void RaiseAllStateChanged()
    {
        OnGoldChanged?.Invoke(gold);
        OnDaysChanged?.Invoke(daysElapsed);
        OnInventoryChanged?.Invoke();
        OnLocationChanged?.Invoke(currentLocation);
        OnJourneyStepChanged?.Invoke(currentJourneyStepIndex, CurrentJourneyStep);
        OnJourneyProgressChanged?.Invoke(CurrentJourneyStep?.StepType == JourneyStepType.Travel ? 0f : 1f);
    }
}
