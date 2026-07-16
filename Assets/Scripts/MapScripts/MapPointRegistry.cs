using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapPointRegistry : MonoBehaviour
{
    [SerializeField] private List<MapPointBinding> points = new();

    public IReadOnlyList<MapPointBinding> Points => points;

    public bool TryGetPoint(MapPointId pointId, out Transform point)
    {
        for (int i = 0; i < points.Count; i++)
        {
            MapPointBinding binding = points[i];
            if (binding.PointId != pointId)
            {
                continue;
            }

            point = binding.PointTransform;
            if (point == null)
            {
                Debug.LogError($"{nameof(MapPointRegistry)}: Map point '{pointId}' has no Transform assigned.", this);
                return false;
            }

            return true;
        }

        point = null;
        return false;
    }

    private void OnValidate()
    {
        points ??= new List<MapPointBinding>();

        HashSet<MapPointId> usedIds = new();
        for (int i = points.Count - 1; i >= 0; i--)
        {
            MapPointBinding binding = points[i];
            if (!usedIds.Add(binding.PointId))
            {
                Debug.LogError($"{nameof(MapPointRegistry)}: duplicate MapPointId '{binding.PointId}' removed.", this);
                points.RemoveAt(i);
                continue;
            }

            if (binding.PointTransform == null)
            {
                Debug.LogError($"{nameof(MapPointRegistry)}: Map point '{binding.PointId}' has an empty Transform reference.", this);
            }
        }
    }
}

[Serializable]
public sealed class MapPointBinding
{
    [SerializeField] private MapPointId pointId;
    [SerializeField] private Transform pointTransform;

    public MapPointId PointId
    {
        get => pointId;
        set => pointId = value;
    }

    public Transform PointTransform
    {
        get => pointTransform;
        set => pointTransform = value;
    }
}
