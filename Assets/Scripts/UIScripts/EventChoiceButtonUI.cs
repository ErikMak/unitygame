using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EventChoiceButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    private EventChoice choice;
    private Action<EventChoice> onSelected;

    public void Initialize(EventChoice eventChoice, int currentGold, Action<EventChoice> selectedCallback)
    {
        choice = eventChoice;
        onSelected = selectedCallback;

        button ??= GetComponent<Button>();
        buttonText ??= GetComponentInChildren<TMP_Text>(true);

        bool hasChoice = choice != null;
        bool enoughGold = hasChoice && currentGold >= choice.RequiredGold;
        bool isEnabled = hasChoice && choice.IsAvailable && enoughGold;

        if (buttonText != null)
        {
            if (!hasChoice)
            {
                buttonText.text = "Недоступный вариант";
            }
            else if (!choice.IsAvailable)
            {
                buttonText.text = $"{choice.ButtonText}\nНедоступно";
            }
            else if (!enoughGold)
            {
                buttonText.text = $"{choice.ButtonText}\nНедостаточно золота: требуется {choice.RequiredGold}";
            }
            else
            {
                buttonText.text = choice.ButtonText;
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = isEnabled;
            button.onClick.AddListener(HandleClick);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void HandleClick()
    {
        if (choice == null)
        {
            return;
        }

        onSelected?.Invoke(choice);
    }
}
