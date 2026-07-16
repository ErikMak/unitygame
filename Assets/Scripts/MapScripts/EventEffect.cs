using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EventEffect
{
    [SerializeField] private int goldDelta;
    [SerializeField] private int daysDelta;
    [SerializeField] private bool continueJourney = true;
    [SerializeField] private bool finishEvent = true;
    [SerializeField] private List<ItemDelta> itemChanges = new();
    [SerializeField, Min(0)] private int removeAnyCargoQuantity;

    public int GoldDelta
    {
        get => goldDelta;
        set => goldDelta = value;
    }

    public int DaysDelta
    {
        get => daysDelta;
        set => daysDelta = Mathf.Max(0, value);
    }

    public bool ContinueJourney
    {
        get => continueJourney;
        set => continueJourney = value;
    }

    public bool FinishEvent
    {
        get => finishEvent;
        set => finishEvent = value;
    }

    public List<ItemDelta> ItemChanges => itemChanges;

    public int RemoveAnyCargoQuantity
    {
        get => removeAnyCargoQuantity;
        set => removeAnyCargoQuantity = Mathf.Max(0, value);
    }

    public void ClampValues()
    {
        daysDelta = Mathf.Max(0, daysDelta);
        removeAnyCargoQuantity = Mathf.Max(0, removeAnyCargoQuantity);
        itemChanges ??= new List<ItemDelta>();
    }
}

[Serializable]
public sealed class ItemDelta
{
    [SerializeField] private GoodItem item;
    [SerializeField] private int quantityDelta;

    public GoodItem Item
    {
        get => item;
        set => item = value;
    }

    public int QuantityDelta
    {
        get => quantityDelta;
        set => quantityDelta = value;
    }
}
