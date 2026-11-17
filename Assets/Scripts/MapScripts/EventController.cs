using UnityEngine;
using System;

public sealed class EventController : MonoBehaviour
{
    [SerializeField] private GameState gameState;
    [SerializeField] private JourneyController journeyController;
    [SerializeField] private EventPopupUI eventPopupUI;

    private EventDefinition activeEvent;
    private EventChoice appliedChoice;
    private bool isEventOpen;
    private bool hasAppliedChoice;
    private bool advanceJourneyOnClose;
    private Action standaloneCompleted;

    public bool IsEventOpen => isEventOpen;

    public void OpenEvent(EventDefinition eventDefinition)
    {
        OpenInternal(eventDefinition, true, null);
    }

    public void OpenStandaloneEvent(EventDefinition eventDefinition, Action completed = null)
    {
        OpenInternal(eventDefinition, false, completed);
    }

    private void OpenInternal(EventDefinition eventDefinition, bool advanceJourney, Action completed)
    {
        if (isEventOpen)
        {
            Debug.LogWarning($"{nameof(EventController)}: event '{activeEvent?.EventId}' is already open.", this);
            return;
        }

        ResolveReferences();

        if (gameState == null || eventPopupUI == null || (advanceJourney && journeyController == null))
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
        advanceJourneyOnClose = advanceJourney;
        standaloneCompleted = completed;

        eventPopupUI.Open(eventDefinition, gameState, OnChoiceSelected, OnContinuePressed,
            startWithDialogue: !advanceJourney);
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

        if (choice.RequiredItem != null && !gameState.HasItem(choice.RequiredItem, choice.RequiredItemQuantity))
        {
            Debug.LogWarning($"{nameof(EventController)}: missing required item for choice '{choice.ChoiceId}'.", this);
            return;
        }

        EventEffect effect = choice.Effect;
        if (effect == null)
        {
            Debug.LogError($"{nameof(EventController)}: choice '{choice.ChoiceId}' has no effect.", this);
            return;
        }

        if (!CanApplyEffect(choice, effect))
        {
            Debug.LogWarning($"{nameof(EventController)}: choice '{choice.ChoiceId}' cannot be applied.", this);
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

        ApplyInventoryEffect(choice, effect);

        hasAppliedChoice = true;
        appliedChoice = choice;
        eventPopupUI.ShowResult(choice.ResultText);
    }

    private bool CanApplyEffect(EventChoice choice, EventEffect effect)
    {
        if (effect.GoldDelta < 0 && gameState.Gold < Mathf.Abs(effect.GoldDelta))
        {
            return false;
        }

        if (choice.RequiredItem != null && !gameState.HasItem(choice.RequiredItem, choice.RequiredItemQuantity))
        {
            return false;
        }

        if (effect.RemoveAnyCargoQuantity > 0)
        {
            int totalCargo = 0;
            for (int i = 0; i < gameState.Inventory.SlotCount; i++)
            {
                totalCargo += gameState.Inventory.GetSlot(i)?.Quantity ?? 0;
            }

            if (totalCargo < effect.RemoveAnyCargoQuantity)
            {
                return false;
            }
        }

        for (int i = 0; i < effect.ItemChanges.Count; i++)
        {
            ItemDelta delta = effect.ItemChanges[i];
            if (delta == null || delta.Item == null || delta.QuantityDelta == 0)
            {
                continue;
            }

            if (delta.QuantityDelta < 0 && !gameState.HasItem(delta.Item, Mathf.Abs(delta.QuantityDelta)))
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyInventoryEffect(EventChoice choice, EventEffect effect)
    {
        if (choice.RequiredItem != null && choice.RequiredItemQuantity > 0)
        {
            gameState.TryRemoveItem(choice.RequiredItem, choice.RequiredItemQuantity);
        }

        if (effect.RemoveAnyCargoQuantity > 0)
        {
            gameState.TryRemoveAnyCargo(effect.RemoveAnyCargoQuantity);
        }

        for (int i = 0; i < effect.ItemChanges.Count; i++)
        {
            ItemDelta delta = effect.ItemChanges[i];
            if (delta == null || delta.Item == null || delta.QuantityDelta == 0)
            {
                continue;
            }

            if (delta.QuantityDelta > 0)
            {
                gameState.TryAddItem(delta.Item, delta.QuantityDelta);
            }
            else
            {
                gameState.TryRemoveItem(delta.Item, Mathf.Abs(delta.QuantityDelta));
            }
        }
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

        if (advanceJourneyOnClose)
        {
            gameState.AdvanceJourneyStep();
            if (shouldContinue)
            {
                journeyController.ContinueAfterEvent();
            }
        }
        else
        {
            Action completed = standaloneCompleted;
            standaloneCompleted = null;
            completed?.Invoke();
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
