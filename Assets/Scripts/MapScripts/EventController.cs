using UnityEngine;

public sealed class EventController : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private JourneyController journeyController;
    [SerializeField] private EventPopupUI eventPopupUI;

    private EventDefinition activeEvent;
    private EventChoice appliedChoice;
    private bool isEventOpen;
    private bool hasAppliedChoice;

    public bool IsEventOpen => isEventOpen;

    public void OpenEvent(EventDefinition eventDefinition)
    {
        if (isEventOpen)
        {
            Debug.LogWarning($"{nameof(EventController)}: event '{activeEvent?.EventId}' is already open.", this);
            return;
        }

        ResolveReferences();

        if (gameState == null || journeyController == null || eventPopupUI == null)
        {
            Debug.LogError($"{nameof(EventController)}: missing required references.", this);
            return;
        }

        if (eventDefinition == null)
        {
            Debug.LogError($"{nameof(EventController)}: EventDefinition is missing.", this);
            return;
        }

        if (eventDefinition.IsOneShot && gameState.HasCompletedEvent(eventDefinition.EventId))
        {
            Debug.LogWarning($"{nameof(EventController)}: one-shot event '{eventDefinition.EventId}' is already completed.", this);
            return;
        }

        activeEvent = eventDefinition;
        appliedChoice = null;
        hasAppliedChoice = false;
        isEventOpen = true;

        eventPopupUI.Open(eventDefinition, gameState, OnChoiceSelected, OnContinuePressed);
    }

    public void SelectChoice(EventChoice choice)
    {
        OnChoiceSelected(choice);
    }

    private void OnChoiceSelected(EventChoice choice)
    {
        if (!isEventOpen || activeEvent == null)
        {
            return;
        }

        if (hasAppliedChoice)
        {
            return;
        }

        if (choice == null)
        {
            Debug.LogError($"{nameof(EventController)}: selected choice is null.", this);
            return;
        }

        if (!choice.IsAvailable)
        {
            Debug.LogWarning($"{nameof(EventController)}: choice '{choice.ChoiceId}' is unavailable.", this);
            return;
        }

        if (gameState.Gold < choice.RequiredGold)
        {
            Debug.LogWarning($"{nameof(EventController)}: not enough gold for choice '{choice.ChoiceId}'.", this);
            return;
        }

        EventEffect effect = choice.Effect;
        if (effect == null)
        {
            Debug.LogError($"{nameof(EventController)}: choice '{choice.ChoiceId}' has no effect.", this);
            return;
        }

        if (effect.GoldDelta < 0 && !gameState.TrySpendGold(Mathf.Abs(effect.GoldDelta)))
        {
            Debug.LogWarning($"{nameof(EventController)}: failed to spend gold for choice '{choice.ChoiceId}'.", this);
            return;
        }

        if (effect.GoldDelta > 0)
        {
            gameState.AddGold(effect.GoldDelta);
        }

        if (effect.DaysDelta > 0)
        {
            gameState.AddDays(effect.DaysDelta);
        }

        hasAppliedChoice = true;
        appliedChoice = choice;
        eventPopupUI.ShowResult(choice.ResultText);
    }

    private void OnContinuePressed()
    {
        if (!isEventOpen || activeEvent == null || !hasAppliedChoice)
        {
            return;
        }

        EventEffect effect = appliedChoice?.Effect;
        if (activeEvent.IsOneShot)
        {
            gameState.RegisterCompletedEvent(activeEvent.EventId);
        }

        eventPopupUI.Close();

        bool shouldContinue = effect == null || effect.ContinueJourney;
        isEventOpen = false;
        activeEvent = null;
        appliedChoice = null;
        hasAppliedChoice = false;

        gameState.AdvanceJourneyStep();

        if (shouldContinue)
        {
            journeyController.ContinueAfterEvent();
        }
    }

    private void ResolveReferences()
    {
        gameState = GameState.Instance != null ? GameState.Instance : gameState;
        gameState ??= FindAnyObjectByType<GameState>();
        journeyController ??= FindAnyObjectByType<JourneyController>();
        eventPopupUI ??= FindAnyObjectByType<EventPopupUI>(FindObjectsInactive.Include);
    }
}
