using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class JourneyController : MonoBehaviour
{
    private const float PlayerUnitHeight = 0.20f;
    private const float DefaultTravelDuration = 1.00f;
    private const float NextStepDelaySeconds = 0.25f;

    [SerializeField] private GameState gameState;
    [SerializeField] private MapPointRegistry pointRegistry;
    [SerializeField] private JourneyPathRegistry pathRegistry;
    [SerializeField] private EventController eventController;
    [SerializeField] private Transform playerUnit;

    [SerializeField, Min(0.001f)] private float waypointArrivalDistance = 0.05f;
    [SerializeField, Min(0f)] private float rotationSpeedDegrees = 240f;
    [SerializeField, Min(0f)] private float startDelaySeconds = 0.50f;
    [SerializeField] private bool autoStart = true;

    [SerializeField] private JourneyControllerState state = JourneyControllerState.Initializing;

    private Coroutine processingCoroutine;
    private Coroutine eventContinuationCoroutine;
    private int completedStepIndex = -1;

    public JourneyControllerState State => state;

    private void Start()
    {
        StartProcessing();
    }

    public void StartProcessing()
    {
        if (processingCoroutine != null)
        {
            StopCoroutine(processingCoroutine);
        }

        processingCoroutine = StartCoroutine(InitializeAndRun());
    }

    public void ContinueAfterEvent()
    {
        if (state != JourneyControllerState.StoppedAtEvent)
        {
            return;
        }

        if (eventContinuationCoroutine != null)
        {
            return;
        }

        state = JourneyControllerState.Waiting;
        eventContinuationCoroutine = StartCoroutine(ContinueAfterEventRoutine());
    }

    private IEnumerator InitializeAndRun()
    {
        state = JourneyControllerState.Initializing;
        yield return null;

        if (!ResolveReferences())
        {
            state = JourneyControllerState.Error;
            processingCoroutine = null;
            yield break;
        }

        if (gameState.Route == null || gameState.Route.Steps == null || gameState.Route.Steps.Count == 0)
        {
            Debug.LogError($"{nameof(JourneyController)}: GameState has no route assigned.", this);
            state = JourneyControllerState.Error;
            processingCoroutine = null;
            yield break;
        }

        if (!gameState.IsNewGameInitialized)
        {
            gameState.StartNewGame();
        }

        if (!gameState.HasCurrentJourneyStep)
        {
            Debug.LogError($"{nameof(JourneyController)}: GameState has no current journey step.", this);
            state = JourneyControllerState.Error;
            processingCoroutine = null;
            yield break;
        }

        if (!PlacePlayerAtCurrentRoutePosition())
        {
            state = JourneyControllerState.Error;
            processingCoroutine = null;
            yield break;
        }

        state = JourneyControllerState.Waiting;

        if (startDelaySeconds > 0f)
        {
            yield return WaitUnscaled(startDelaySeconds);
        }

        if (autoStart)
        {
            yield return ProcessCurrentStep();
        }

        processingCoroutine = null;
    }

    private bool ResolveReferences()
    {
        gameState = GameState.Instance != null ? GameState.Instance : gameState;

        if (gameState == null)
        {
            gameState = FindAnyObjectByType<GameState>();
        }

        if (gameState == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: GameState reference is missing.", this);
            return false;
        }

        if (pointRegistry == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: MapPointRegistry reference is missing.", this);
            return false;
        }

        if (pathRegistry == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: JourneyPathRegistry reference is missing.", this);
            return false;
        }

        if (eventController == null)
        {
            eventController = FindAnyObjectByType<EventController>();
        }

        if (playerUnit == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: PlayerUnit reference is missing.", this);
            return false;
        }

        return true;
    }

    private bool PlacePlayerAtCurrentRoutePosition()
    {
        MapPointId pointId = GetRestorePointForCurrentStep();
        return PlacePlayerAtMapPoint(pointId);
    }

    private bool PlacePlayerAtMapPoint(MapPointId pointId)
    {
        if (!pointRegistry.TryGetPoint(pointId, out Transform point))
        {
            Debug.LogError($"{nameof(JourneyController)}: unable to restore PlayerUnit at map point '{pointId}'.", this);
            return false;
        }

        SetPlayerPosition(point.position);
        return true;
    }

    private MapPointId GetRestorePointForCurrentStep()
    {
        JourneyStep step = gameState.CurrentJourneyStep;
        if (step == null)
        {
            return MapPointId.NevaMouth;
        }

        return step.StepType switch
        {
            JourneyStepType.Travel => step.StartPoint,
            JourneyStepType.Location => step.EndPoint,
            JourneyStepType.Event => step.StartPoint,
            JourneyStepType.Ending => step.EndPoint,
            _ => step.StartPoint
        };
    }

    private IEnumerator ProcessCurrentStep()
    {
        if (!gameState.HasCurrentJourneyStep)
        {
            state = JourneyControllerState.Finished;
            yield break;
        }

        JourneyStep step = gameState.CurrentJourneyStep;
        switch (step.StepType)
        {
            case JourneyStepType.Travel:
                yield return ProcessTravelStep(step, gameState.CurrentJourneyStepIndex);
                break;
            case JourneyStepType.Location:
                PlacePlayerAtMapPoint(step.EndPoint);
                state = JourneyControllerState.StoppedAtLocation;
                Debug.Log($"{nameof(JourneyController)}: stopped at location {step.DestinationLocation}.", this);
                break;
            case JourneyStepType.Event:
                HandleEventStep(step);
                break;
            case JourneyStepType.Ending:
                PlacePlayerAtMapPoint(step.EndPoint);
                state = JourneyControllerState.Finished;
                gameState.MarkGameFinished();
                break;
            default:
                state = JourneyControllerState.Error;
                Debug.LogError($"{nameof(JourneyController)}: unsupported JourneyStepType '{step.StepType}'.", this);
                break;
        }
    }

    private IEnumerator ContinueAfterEventRoutine()
    {
        yield return WaitUnscaled(NextStepDelaySeconds);
        yield return ProcessCurrentStep();
        eventContinuationCoroutine = null;
    }

    private void HandleEventStep(JourneyStep step)
    {
        state = JourneyControllerState.StoppedAtEvent;
        processingCoroutine = null;

        if (step.Event == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: event step '{step.Id}' has no EventDefinition.", this);
            state = JourneyControllerState.Error;
            return;
        }

        if (gameState.HasCompletedEvent(step.Event.EventId) && step.Event.IsOneShot)
        {
            gameState.AdvanceJourneyStep();
            ContinueAfterEvent();
            return;
        }

        if (eventController == null)
        {
            Debug.LogError($"{nameof(JourneyController)}: EventController reference is missing.", this);
            state = JourneyControllerState.Error;
            return;
        }

        eventController.OpenEvent(step.Event);
    }

    private IEnumerator ProcessTravelStep(JourneyStep step, int stepIndex)
    {
        if (!pathRegistry.TryGetPath(step.StartPoint, step.EndPoint, out IReadOnlyList<Transform> waypoints))
        {
            Debug.LogError($"{nameof(JourneyController)}: no path for {step.StartPoint} -> {step.EndPoint}.", this);
            state = JourneyControllerState.Error;
            yield break;
        }

        if (waypoints.Count < 2)
        {
            Debug.LogError($"{nameof(JourneyController)}: path {step.StartPoint} -> {step.EndPoint} must contain at least two waypoints.", this);
            state = JourneyControllerState.Error;
            yield break;
        }

        float duration = step.TravelDuration;
        if (duration <= 0f)
        {
            duration = DefaultTravelDuration;
            Debug.LogWarning($"{nameof(JourneyController)}: step '{step.Id}' has non-positive TravelDuration. Using {DefaultTravelDuration:0.00} second.", this);
        }

        state = JourneyControllerState.Moving;
        gameState.SetJourneyProgress(0f);
        Vector3 originalScale = playerUnit.localScale;
        float totalDistance = CalculatePathDistance(waypoints);
        float traveledDistance = 0f;

        if (totalDistance <= Mathf.Epsilon)
        {
            SetPlayerPosition(waypoints[^1].position);
            gameState.SetJourneyProgress(1f);
        }
        else
        {
            float movementSpeed = totalDistance / duration;
            for (int i = 1; i < waypoints.Count; i++)
            {
                Vector3 start = WithPlayerHeight(playerUnit.position);
                Vector3 target = WithPlayerHeight(waypoints[i].position);
                float segmentDistance = Vector3.Distance(start, target);
                yield return MoveToWaypoint(waypoints[i], movementSpeed, traveledDistance, totalDistance);
                traveledDistance += segmentDistance;
                gameState.SetJourneyProgress(Mathf.Clamp01(traveledDistance / totalDistance));
                playerUnit.localScale = originalScale;
            }
        }

        if (completedStepIndex != stepIndex)
        {
            completedStepIndex = stepIndex;
            CompleteTravelStep(step);
        }

        yield return WaitUnscaled(NextStepDelaySeconds);
        gameState.SetJourneyProgress(1f);
        yield return ProcessCurrentStep();
    }

    private IEnumerator MoveToWaypoint(Transform waypoint, float movementSpeed, float completedDistance, float totalDistance)
    {
        while (playerUnit != null && waypoint != null)
        {
            Vector3 targetPosition = WithPlayerHeight(waypoint.position);
            Vector3 currentPosition = WithPlayerHeight(playerUnit.position);
            Vector3 delta = targetPosition - currentPosition;
            Vector3 flatDirection = new Vector3(delta.x, 0f, delta.z);

            if (flatDirection.magnitude <= waypointArrivalDistance)
            {
                yield break;
            }

            RotatePlayerTowards(flatDirection);
            Vector3 nextPosition = Vector3.MoveTowards(currentPosition, targetPosition, movementSpeed * Time.unscaledDeltaTime);
            completedDistance += Vector3.Distance(currentPosition, nextPosition);
            SetPlayerPosition(nextPosition);
            gameState.SetJourneyProgress(totalDistance > 0f ? completedDistance / totalDistance : 1f);
            yield return null;
        }
    }

    private void RotatePlayerTowards(Vector3 flatDirection)
    {
        if (flatDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(flatDirection.normalized, Vector3.up);
        Quaternion yOnlyTarget = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        Quaternion yOnlyCurrent = Quaternion.Euler(0f, playerUnit.eulerAngles.y, 0f);
        playerUnit.rotation = Quaternion.RotateTowards(
            yOnlyCurrent,
            yOnlyTarget,
            rotationSpeedDegrees * Time.unscaledDeltaTime);
    }

    private void CompleteTravelStep(JourneyStep step)
    {
        if (pointRegistry.TryGetPoint(step.EndPoint, out Transform endPoint))
        {
            SetPlayerPosition(endPoint.position);
        }

        gameState.AddDays(step.DaysCost);

        if (TryMapPointToLocation(step.EndPoint, out LocationId locationId))
        {
            gameState.SetCurrentLocation(locationId);
        }

        gameState.AdvanceJourneyStep();
    }

    private static bool TryMapPointToLocation(MapPointId pointId, out LocationId locationId)
    {
        return System.Enum.TryParse(pointId.ToString(), out locationId);
    }

    private static float CalculatePathDistance(IReadOnlyList<Transform> waypoints)
    {
        float distance = 0f;
        for (int i = 1; i < waypoints.Count; i++)
        {
            if (waypoints[i - 1] == null || waypoints[i] == null)
            {
                continue;
            }

            distance += Vector3.Distance(WithPlayerHeight(waypoints[i - 1].position), WithPlayerHeight(waypoints[i].position));
        }

        return distance;
    }

    private void SetPlayerPosition(Vector3 position)
    {
        playerUnit.position = WithPlayerHeight(position);
        playerUnit.rotation = Quaternion.Euler(0f, playerUnit.eulerAngles.y, 0f);
    }

    private static Vector3 WithPlayerHeight(Vector3 position)
    {
        position.y = PlayerUnitHeight;
        return position;
    }

    private static IEnumerator WaitUnscaled(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}

public enum JourneyControllerState
{
    Initializing,
    Waiting,
    Moving,
    StoppedAtLocation,
    StoppedAtEvent,
    Finished,
    Error
}
