using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private Transform slotsRoot;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private DraggedItemUI draggedItem;
    [SerializeField] private EventController eventController;

    private readonly List<InventorySlotUI> slots = new();
    private GameState gameState;

    public bool IsOpen => gameObject.activeSelf;
    public DraggedItemUI DraggedItem => draggedItem;
    public bool IsDragBlocked => eventController != null && eventController.IsEventOpen;

    private void Awake()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        eventController ??= FindAnyObjectByType<EventController>();
        BuildSlots();
        Refresh();
    }

    private void OnEnable()
    {
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
            Refresh();
        }
    }

    public void Hide()
    {
        SetVisible(false);
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
