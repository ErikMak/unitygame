using System;
using UnityEngine;
using UnityEngine.UIElements;
using static HanseaticDocument;

public sealed class EventPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject screenBlocker;
    [SerializeField] private GameObject window;
    [SerializeField] private MapCameraController mapCameraController;
    [SerializeField] private int popupSortingOrder = 100;

    private enum Stage { Closed, Introduction, Choice, Result }

    private HanseaticDocument document;
    private Stage stage;
    private bool cameraWasEnabled;
    private Action<EventChoice> onChoice;
    private Action onContinue;
    private VisualElement Root => document.Root;
    public bool IsOpen => stage != Stage.Closed;

    public void Open(EventDefinition definition, GameState state, Action<EventChoice> choiceSelected,
        Action continuePressed, bool startWithDialogue = false)
    {
        if (definition == null || IsOpen)
            return;

        gameObject.SetActive(true);
        if (document == null)
            BuildDocument();

        Show(Root, true);
        onChoice = choiceSelected;
        onContinue = continuePressed;
        if (mapCameraController != null)
        {
            cameraWasEnabled = mapCameraController.enabled;
            mapCameraController.enabled = false;
        }

        Root.Q<Label>("intro-text").text = definition.Description;
        Root.Q<Label>("choice-text").text = definition.Description;
        Root.Q<Label>("dialog-text").text = definition.Description;
        Root.Q<Label>("speaker").text = definition.SpeakerName;
        Root.Q("choices").EnableInClassList("dialogue", startWithDialogue);
        Root.Q("choices").EnableInClassList("journey", !startWithDialogue);
        Art(Root.Q("choice-back"), startWithDialogue ? "Events/DialogBackground" : "Events/ChoiceBackground");
        BuildAnswers(definition, state);
        SwitchTo(startWithDialogue ? Stage.Choice : Stage.Introduction);
    }

    private void BuildDocument()
    {
        if (screenBlocker != null) screenBlocker.SetActive(false);
        if (window != null) window.SetActive(false);

        document = new HanseaticDocument(transform, "JourneyEvent", popupSortingOrder);
        Art(Root.Q("event-image"), "Events/EventImage");
        Art(Root.Q("accept-icon"), "Events/AcceptIcon");
        Art(Root.Q("intro-back"), "Events/IntroductionBackground");
        Art(Root.Q("intro-left"), "Events/LeftWing");
        Art(Root.Q("choice-left"), "Events/LeftWing");
        Art(Root.Q("intro-right"), "Events/RightWing");
        Art(Root.Q("choice-right"), "Events/RightWing");
        Art(Root.Q("result-panel"), "Events/ResultSurface");
        Art(Root.Q("portrait"), "Events/DialogPortrait");
        Art(Root.Q("dialog-panel"), "Events/DialogPanel");
        Root.Q<Button>("accept").clicked += () =>
        {
            if (stage == Stage.Introduction) SwitchTo(Stage.Choice);
        };
        DecorateAction(Root.Q<Button>("finish"));
        Root.Q<Button>("finish").clicked += () =>
        {
            if (stage == Stage.Result) onContinue?.Invoke();
        };
    }

    private void BuildAnswers(EventDefinition definition, GameState state)
    {
        VisualElement answers = Root.Q("answers");
        answers.Clear();
        foreach (EventChoice choice in definition.Choices)
        {
            if (answers.childCount == 4) break;
            string reason = UnavailableReason(choice, state);
            Button button = new(() =>
            {
                if (stage == Stage.Choice) onChoice?.Invoke(choice);
            });
            button.AddToClassList("answer");
            foreach (string side in new[] { "left", "right" })
            {
                VisualElement border = new() { pickingMode = PickingMode.Ignore };
                border.AddToClassList("answer-border");
                border.AddToClassList("answer-border-" + side);
                button.Add(border);
            }
            button.text = choice == null ? reason : reason == null ? choice.ButtonText : choice.ButtonText + "\n" + reason;
            button.SetEnabled(reason == null);
            VisualElement statusIcon = new() { name = "status-icon", pickingMode = PickingMode.Ignore };
            statusIcon.AddToClassList("answer-status-icon");
            Show(statusIcon, false);
            button.Add(statusIcon);
            answers.Add(button);
        }
    }

    private static string UnavailableReason(EventChoice choice, GameState state)
    {
        if (choice == null || !choice.IsAvailable) return "Недоступно";
        if ((state != null ? state.Gold : 0) < choice.RequiredGold) return $"Требуется золото: {choice.RequiredGold}";
        if (choice.RequiredItem != null && (state == null || !state.HasItem(choice.RequiredItem, choice.RequiredItemQuantity)))
            return $"Требуется: {choice.RequiredItem.DisplayName} × {choice.RequiredItemQuantity}";
        return null;
    }

    private void SwitchTo(Stage next)
    {
        stage = next;
        Show(Root.Q("intro"), next == Stage.Introduction);
        Show(Root.Q("choices"), next == Stage.Choice || next == Stage.Result);
        Show(Root.Q("result"), next == Stage.Result);
        Root.Q("answers").SetEnabled(next == Stage.Choice);
        Button focus = next == Stage.Choice
            ? Root.Q("answers").Query<Button>().Where(button => button.enabledInHierarchy).First()
            : Root.Q<Button>(next == Stage.Introduction ? "accept" : "finish");
        focus?.schedule.Execute(() => focus.Focus());
    }

    public void ShowResult(string text)
    {
        if (!IsOpen) return;
        Root.Q<Label>("result-text").text = text;
        SwitchTo(Stage.Result);
    }

    public void Close()
    {
        if (!IsOpen) return;
        stage = Stage.Closed;
        if (mapCameraController != null) mapCameraController.enabled = cameraWasEnabled;
        Show(Root, false);
        onChoice = null;
        onContinue = null;
    }

    private void OnDestroy() => document?.Dispose();
}
