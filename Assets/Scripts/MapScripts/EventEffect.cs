using System;
using UnityEngine;

[Serializable]
public sealed class EventEffect
{
    [SerializeField] private int goldDelta;
    [SerializeField] private int daysDelta;
    [SerializeField] private bool continueJourney = true;
    [SerializeField] private bool finishEvent = true;

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

    public void ClampValues()
    {
        daysDelta = Mathf.Max(0, daysDelta);
    }
}
