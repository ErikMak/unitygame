using System;
using UnityEngine;

[Serializable]
public sealed class EventChoice
{
    [SerializeField] private string choiceId;
    [SerializeField] private string buttonText;
    [SerializeField] private string resultText;
    [SerializeField] private EventEffect effect = new();
    [SerializeField] private bool isAvailable = true;
    [SerializeField, Min(0)] private int requiredGold;
    [SerializeField] private GoodItem requiredItem;
    [SerializeField, Min(0)] private int requiredItemQuantity;

    public string ChoiceId
    {
        get => choiceId;
        set => choiceId = value;
    }

    public string ButtonText
    {
        get => buttonText;
        set => buttonText = value;
    }

    public string ResultText
    {
        get => resultText;
        set => resultText = value;
    }

    public EventEffect Effect => effect;

    public bool IsAvailable
    {
        get => isAvailable;
        set => isAvailable = value;
    }

    public int RequiredGold
    {
        get => requiredGold;
        set => requiredGold = Mathf.Max(0, value);
    }

    public GoodItem RequiredItem
    {
        get => requiredItem;
        set => requiredItem = value;
    }

    public int RequiredItemQuantity
    {
        get => requiredItemQuantity;
        set => requiredItemQuantity = Mathf.Max(0, value);
    }

    public void Validate()
    {
        requiredGold = Mathf.Max(0, requiredGold);
        requiredItemQuantity = Mathf.Max(0, requiredItemQuantity);
        effect ??= new EventEffect();
        effect.ClampValues();
    }
}
