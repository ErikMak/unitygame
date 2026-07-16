using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "JourneyRouteDefinition", menuName = "Hanseatic/Journey Route Definition")]
public class JourneyRouteDefinition : ScriptableObject
{
    [SerializeField] private string routeId;
    [SerializeField] private string displayName;
    [SerializeField] private List<JourneyStep> steps = new();

    public string RouteId
    {
        get => routeId;
        set => routeId = value;
    }

    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    public List<JourneyStep> Steps => steps;

    private void OnValidate()
    {
        ValidateData();
    }

    public bool ValidateData()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(routeId))
        {
            Debug.LogWarning($"{name}: RouteId is empty.", this);
            isValid = false;
        }

        steps ??= new List<JourneyStep>();

        HashSet<string> stepIds = new();
        MapPointId? previousTravelEndPoint = null;

        for (int i = 0; i < steps.Count; i++)
        {
            JourneyStep step = steps[i];
            if (step == null)
            {
                Debug.LogWarning($"{name}: Step at index {i} is null.", this);
                isValid = false;
                previousTravelEndPoint = null;
                continue;
            }

            step.ClampNonNegativeValues();

            if (string.IsNullOrWhiteSpace(step.Id))
            {
                Debug.LogWarning($"{name}: Step at index {i} has empty Id.", this);
                isValid = false;
            }
            else if (!stepIds.Add(step.Id))
            {
                Debug.LogWarning($"{name}: Step Id '{step.Id}' is duplicated.", this);
                isValid = false;
            }

            if (step.StepType == JourneyStepType.Travel)
            {
                if (step.StartPoint == step.EndPoint)
                {
                    Debug.LogWarning($"{name}: Travel step '{step.Id}' has identical StartPoint and EndPoint.", this);
                    isValid = false;
                }

                if (previousTravelEndPoint.HasValue && previousTravelEndPoint.Value != step.StartPoint)
                {
                    Debug.LogWarning(
                        $"{name}: Travel step '{step.Id}' starts at {step.StartPoint}, but previous travel ended at {previousTravelEndPoint.Value}.",
                        this);
                    isValid = false;
                }

                previousTravelEndPoint = step.EndPoint;
            }
            else
            {
                previousTravelEndPoint = null;
            }
        }

        return isValid;
    }
}
