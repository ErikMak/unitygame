using System;
using UnityEngine;

[Serializable]
public class JourneyStep
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private JourneyStepType stepType;
    [SerializeField] private MapPointId startPoint;
    [SerializeField] private MapPointId endPoint;
    [SerializeField] private LocationId destinationLocation;
    [SerializeField, Min(0f)] private float travelDuration;
    [SerializeField, Min(0)] private int daysCost;
    [SerializeField] private bool isReturnRoute;

    public string Id
    {
        get => id;
        set => id = value;
    }

    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    public JourneyStepType StepType
    {
        get => stepType;
        set => stepType = value;
    }

    public MapPointId StartPoint
    {
        get => startPoint;
        set => startPoint = value;
    }

    public MapPointId EndPoint
    {
        get => endPoint;
        set => endPoint = value;
    }

    public LocationId DestinationLocation
    {
        get => destinationLocation;
        set => destinationLocation = value;
    }

    public float TravelDuration
    {
        get => travelDuration;
        set => travelDuration = Mathf.Max(0f, value);
    }

    public int DaysCost
    {
        get => daysCost;
        set => daysCost = Mathf.Max(0, value);
    }

    public bool IsReturnRoute
    {
        get => isReturnRoute;
        set => isReturnRoute = value;
    }

    public void ClampNonNegativeValues()
    {
        travelDuration = Mathf.Max(0f, travelDuration);
        daysCost = Mathf.Max(0, daysCost);
    }
}
