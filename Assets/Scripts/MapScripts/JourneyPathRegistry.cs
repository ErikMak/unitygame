using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class JourneyPathRegistry : MonoBehaviour
{
    [SerializeField] private List<JourneyPathSegment> segments = new();

    public IReadOnlyList<JourneyPathSegment> Segments => segments;

    public bool TryGetPath(MapPointId startPoint, MapPointId endPoint, out IReadOnlyList<Transform> waypoints)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            JourneyPathSegment segment = segments[i];
            if (segment.StartPoint != startPoint || segment.EndPoint != endPoint)
            {
                continue;
            }

            if (segment.Waypoints == null || segment.Waypoints.Count == 0)
            {
                Debug.LogError($"{nameof(JourneyPathRegistry)}: path {startPoint} -> {endPoint} has no waypoints.", this);
                waypoints = null;
                return false;
            }

            for (int waypointIndex = 0; waypointIndex < segment.Waypoints.Count; waypointIndex++)
            {
                if (segment.Waypoints[waypointIndex] == null)
                {
                    Debug.LogError(
                        $"{nameof(JourneyPathRegistry)}: path {startPoint} -> {endPoint} has an empty waypoint at index {waypointIndex}.",
                        this);
                    waypoints = null;
                    return false;
                }
            }

            waypoints = segment.Waypoints;
            return true;
        }

        waypoints = null;
        return false;
    }

    private void OnValidate()
    {
        segments ??= new List<JourneyPathSegment>();

        HashSet<string> usedSegments = new();
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            JourneyPathSegment segment = segments[i];
            string key = $"{segment.StartPoint}->{segment.EndPoint}";
            if (!usedSegments.Add(key))
            {
                Debug.LogError($"{nameof(JourneyPathRegistry)}: duplicate path '{key}' removed.", this);
                segments.RemoveAt(i);
                continue;
            }

            if (segment.Waypoints == null || segment.Waypoints.Count == 0)
            {
                Debug.LogError($"{nameof(JourneyPathRegistry)}: path '{key}' has no waypoints.", this);
                continue;
            }

            for (int waypointIndex = 0; waypointIndex < segment.Waypoints.Count; waypointIndex++)
            {
                if (segment.Waypoints[waypointIndex] == null)
                {
                    Debug.LogError($"{nameof(JourneyPathRegistry)}: path '{key}' has an empty waypoint at index {waypointIndex}.", this);
                }
            }
        }
    }
}

[Serializable]
public sealed class JourneyPathSegment
{
    [SerializeField] private MapPointId startPoint;
    [SerializeField] private MapPointId endPoint;
    [SerializeField] private List<Transform> waypoints = new();

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

    public List<Transform> Waypoints => waypoints;
}
