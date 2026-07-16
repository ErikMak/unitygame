using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EventDefinition", menuName = "Hanseatic/Events/Event Definition")]
public sealed class EventDefinition : ScriptableObject
{
    [SerializeField] private string eventId;
    [SerializeField] private string title;
    [SerializeField, TextArea(3, 8)] private string description;
    [SerializeField] private Sprite illustration;
    [SerializeField] private List<EventChoice> choices = new();
    [SerializeField] private bool isOneShot = true;
    [SerializeField] private bool showResultAfterChoice = true;

    public string EventId
    {
        get => eventId;
        set => eventId = value;
    }

    public string Title
    {
        get => title;
        set => title = value;
    }

    public string Description
    {
        get => description;
        set => description = value;
    }

    public Sprite Illustration
    {
        get => illustration;
        set => illustration = value;
    }

    public List<EventChoice> Choices => choices;
    public bool IsOneShot => isOneShot;
    public bool ShowResultAfterChoice => showResultAfterChoice;

    private void OnValidate()
    {
        ValidateData();
    }

    public bool ValidateData()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(eventId))
        {
            Debug.LogError($"{name}: EventId is required.", this);
            isValid = false;
        }

        choices ??= new List<EventChoice>();
        if (choices.Count < 4)
        {
            Debug.LogError($"{name}: EventDefinition must contain at least 4 choices.", this);
            isValid = false;
        }

        HashSet<string> choiceIds = new();
        for (int i = 0; i < choices.Count; i++)
        {
            EventChoice choice = choices[i];
            if (choice == null)
            {
                Debug.LogError($"{name}: choice at index {i} is null.", this);
                isValid = false;
                continue;
            }

            choice.Validate();

            if (string.IsNullOrWhiteSpace(choice.ChoiceId))
            {
                Debug.LogError($"{name}: choice at index {i} has empty ChoiceId.", this);
                isValid = false;
            }
            else if (!choiceIds.Add(choice.ChoiceId))
            {
                Debug.LogError($"{name}: duplicate ChoiceId '{choice.ChoiceId}'.", this);
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(choice.ButtonText))
            {
                Debug.LogError($"{name}: choice '{choice.ChoiceId}' has empty ButtonText.", this);
                isValid = false;
            }
        }

        return isValid;
    }
}
