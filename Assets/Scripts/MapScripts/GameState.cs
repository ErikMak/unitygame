using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    [SerializeField, Min(0)] private int startingGold = 100;
    [SerializeField] private JourneyRouteDefinition route;

    private int gold;
    private int currentJourneyStepIndex;
    private int daysElapsed;
    private LocationId currentLocation;
    private bool isNewGameInitialized;
    private bool isGameFinished;

    public int StartingGold => startingGold;
    public int Gold => gold;
    public int CurrentJourneyStepIndex => currentJourneyStepIndex;
    public int DaysElapsed => daysElapsed;
    public LocationId CurrentLocation => currentLocation;
    public bool IsNewGameInitialized => isNewGameInitialized;
    public bool IsGameFinished => isGameFinished;
    public JourneyRouteDefinition Route => route;

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
    }

    private void OnValidate()
    {
        startingGold = Mathf.Max(0, startingGold);
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
        gold = Mathf.Max(0, startingGold);
        currentJourneyStepIndex = 0;
        daysElapsed = 0;
        currentLocation = LocationId.NevaMouth;
        isGameFinished = false;
        isNewGameInitialized = true;

        ClampState();
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
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        gold += amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (amount < 0 || amount > gold)
        {
            return false;
        }

        gold -= amount;
        return true;
    }

    public void AddDays(int days)
    {
        if (days <= 0)
        {
            return;
        }

        daysElapsed += days;
    }

    public void SetCurrentLocation(LocationId location)
    {
        currentLocation = location;
    }

    public void MarkGameFinished()
    {
        isGameFinished = true;
    }

    private void ClampState()
    {
        startingGold = Mathf.Max(0, startingGold);
        gold = Mathf.Max(0, gold);
        daysElapsed = Mathf.Max(0, daysElapsed);

        if (route == null || route.Steps == null || route.Steps.Count == 0)
        {
            currentJourneyStepIndex = Mathf.Max(0, currentJourneyStepIndex);
            return;
        }

        currentJourneyStepIndex = Mathf.Clamp(currentJourneyStepIndex, 0, route.Steps.Count - 1);
    }
}
