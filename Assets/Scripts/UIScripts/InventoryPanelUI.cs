using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private Transform slotsRoot;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private DraggedItemUI draggedItem;
    [SerializeField] private EventController eventController;
    [SerializeField] private Button closeButton;

    private readonly List<InventorySlotUI> slots = new();
    private readonly List<Button> boundCloseButtons = new();
    private GameState gameState;

    public bool IsOpen => gameObject.activeSelf;
    public DraggedItemUI DraggedItem => draggedItem;
    public bool IsDragBlocked => eventController != null && eventController.IsEventOpen;
    public bool IsDragging => IsOpen && draggedItem != null && draggedItem.gameObject.activeInHierarchy;

    private void Awake()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        eventController ??= FindAnyObjectByType<EventController>();
        BuildSlots();
        Refresh();

        BindCloseButton();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < boundCloseButtons.Count; i++)
        {
            if (boundCloseButtons[i] != null)
            {
                boundCloseButtons[i].onClick.RemoveListener(Hide);
            }
        }
    }

    private void OnEnable()
    {
        BindCloseButton();

        if (gameState != null)
        {
            gameState.OnInventoryChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (gameState != null)
        {
            gameState.OnInventoryChanged -= Refresh;
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible)
        {
            transform.SetAsLastSibling();
            Refresh();
        }
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void BindCloseButton()
    {
        boundCloseButtons.Clear();
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name != "CloseButton")
            {
                continue;
            }

            closeButton ??= buttons[i];
            buttons[i].interactable = true;
            buttons[i].onClick.RemoveListener(Hide);
            buttons[i].onClick.AddListener(Hide);
            boundCloseButtons.Add(buttons[i]);
        }
    }

    public void MoveOrMerge(int sourceIndex, int targetIndex)
    {
        if (gameState == null || IsDragBlocked)
        {
            return;
        }

        gameState.MoveOrMergeSlot(sourceIndex, targetIndex);
        Refresh();
    }

    public void Refresh()
    {
        if (gameState == null)
        {
            return;
        }

        BuildSlots();
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Refresh(gameState.Inventory.GetSlot(i));
        }
    }

    private void BuildSlots()
    {
        if (gameState == null || slotsRoot == null || slotPrefab == null)
        {
            return;
        }

        while (slots.Count < gameState.InventorySlotCount)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, slotsRoot);
            slot.Initialize(this, slots.Count);
            slots.Add(slot);
        }
    }
}
