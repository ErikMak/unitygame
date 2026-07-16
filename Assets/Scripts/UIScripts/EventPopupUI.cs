using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class EventPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject screenBlocker;
    [SerializeField] private GameObject window;
    [SerializeField] private Image illustration;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject closeBlocker;
    [SerializeField] private EventChoiceButtonUI choiceButtonPrefab;
    [SerializeField] private MapCameraController mapCameraController;
    [SerializeField] private bool continueImmediatelyOnChoice;
    [SerializeField] private int popupSortingOrder = 100;

    private readonly List<EventChoiceButtonUI> spawnedChoiceButtons = new();
    private Action<EventChoice> onChoiceSelected;
    private Action onContinuePressed;
    private Sprite fallbackIllustration;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        EnsureCanvasConfiguration();

        if (illustration != null)
        {
            fallbackIllustration = illustration.sprite;
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(HandleContinuePressed);
        }
    }

    public void Open(EventDefinition eventDefinition, GameState gameState, Action<EventChoice> choiceSelected, Action continuePressed)
    {
        if (eventDefinition == null)
        {
            return;
        }

        onChoiceSelected = choiceSelected;
        onContinuePressed = continuePressed;

        EnsureCanvasConfiguration();
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        SetCameraEnabled(false);

        if (screenBlocker != null)
        {
            screenBlocker.SetActive(true);
        }

        if (window != null)
        {
            window.SetActive(true);
        }

        if (closeBlocker != null)
        {
            closeBlocker.SetActive(true);
        }

        if (titleText != null)
        {
            titleText.text = eventDefinition.Title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = eventDefinition.Description;
        }

        if (illustration != null)
        {
            Sprite sprite = eventDefinition.Illustration != null ? eventDefinition.Illustration : fallbackIllustration;
            illustration.sprite = sprite;
            illustration.gameObject.SetActive(sprite != null);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        ClearChoiceButtons();
        EventChoiceButtonUI[] existingButtons = choicesContainer != null
            ? choicesContainer.GetComponentsInChildren<EventChoiceButtonUI>(true)
            : System.Array.Empty<EventChoiceButtonUI>();

        int visibleCount = Mathf.Min(4, eventDefinition.Choices.Count);
        for (int i = 0; i < existingButtons.Length; i++)
        {
            existingButtons[i].gameObject.SetActive(i < visibleCount);
        }

        for (int i = 0; i < visibleCount; i++)
        {
            EventChoiceButtonUI button = i < existingButtons.Length ? existingButtons[i] : CreateChoiceButton();
            button.gameObject.SetActive(true);
            button.Initialize(eventDefinition.Choices[i], gameState, HandleChoiceSelected);
            spawnedChoiceButtons.Add(button);
        }
    }

    private EventChoiceButtonUI CreateChoiceButton()
    {
        if (choiceButtonPrefab != null)
        {
            return Instantiate(choiceButtonPrefab, choicesContainer);
        }

        GameObject buttonObject = new("ChoiceButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(EventChoiceButtonUI));
        buttonObject.transform.SetParent(choicesContainer, false);

        RectTransform rectTransform = (RectTransform)buttonObject.transform;
        rectTransform.sizeDelta = new Vector2(0f, 64f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.72f, 0.58f, 0.38f, 1f);

        GameObject textObject = new("ButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textTransform = (RectTransform)textObject.transform;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = new Vector2(18f, 6f);
        textTransform.offsetMax = new Vector2(-18f, -6f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.fontSize = 24f;
        text.color = new Color(0.12f, 0.08f, 0.04f, 1f);
        text.alignment = TextAlignmentOptions.MidlineLeft;

        return buttonObject.GetComponent<EventChoiceButtonUI>();
    }

    public void ShowResult(string text)
    {
        for (int i = 0; i < spawnedChoiceButtons.Count; i++)
        {
            spawnedChoiceButtons[i].SetInteractable(false);
        }

        if (resultText != null)
        {
            resultText.text = text;
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
    }

    public void Close()
    {
        ClearChoiceButtons();
        SetCameraEnabled(true);

        if (screenBlocker != null)
        {
            screenBlocker.SetActive(false);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private void HandleChoiceSelected(EventChoice choice)
    {
        onChoiceSelected?.Invoke(choice);

        if (continueImmediatelyOnChoice)
        {
            HandleContinuePressed();
        }
    }

    private void HandleContinuePressed()
    {
        onContinuePressed?.Invoke();
    }

    private void ClearChoiceButtons()
    {
        for (int i = spawnedChoiceButtons.Count - 1; i >= 0; i--)
        {
            if (spawnedChoiceButtons[i] != null)
            {
                if (spawnedChoiceButtons[i].transform.parent == choicesContainer &&
                    spawnedChoiceButtons[i].gameObject.name.StartsWith("ChoiceButton_"))
                {
                    spawnedChoiceButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    Destroy(spawnedChoiceButtons[i].gameObject);
                }
            }
        }

        spawnedChoiceButtons.Clear();
    }

    private void SetCameraEnabled(bool enabled)
    {
        if (mapCameraController != null)
        {
            mapCameraController.enabled = enabled;
        }
    }

    private void EnsureCanvasConfiguration()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = popupSortingOrder;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        if (transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        if (screenBlocker != null && screenBlocker.transform is RectTransform blockerTransform)
        {
            blockerTransform.anchorMin = Vector2.zero;
            blockerTransform.anchorMax = Vector2.one;
            blockerTransform.offsetMin = Vector2.zero;
            blockerTransform.offsetMax = Vector2.zero;

            Image blockerImage = screenBlocker.GetComponent<Image>();
            if (blockerImage != null)
            {
                blockerImage.raycastTarget = true;
            }
        }
    }
}
