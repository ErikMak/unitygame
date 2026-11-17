using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static HanseaticDocument;

public sealed class InventoryPanelUI : MonoBehaviour
{
    [SerializeField] private EventController eventController;

    private const int Columns = 7;
    private readonly List<Button> slots = new();
    private HanseaticDocument document;
    private GameState gameState;
    private VisualElement grid;
    private VisualElement ghost;
    private int selected = -1;
    private int pressed = -1;
    private int pointerId = -1;
    private Vector2 origin;

    public bool IsOpen { get; private set; }
    public bool IsDragging { get; private set; }
    public bool IsDragBlocked => eventController != null && eventController.IsEventOpen;

    private void OnDestroy()
    {
        if (gameState != null) gameState.OnInventoryChanged -= Refresh;
        document?.Dispose();
    }

    public void SetVisible(bool visible)
    {
        if (visible && document == null)
            Build();

        if (document == null) return;
        IsOpen = visible;
        Show(document.Root, visible);
        if (!visible)
        {
            CancelDrag();
            return;
        }

        Refresh();
        Button close = document.Root.Q<Button>("close");
        close.schedule.Execute(() => close.Focus());
    }

    public void Hide() => SetVisible(false);

    public void MoveOrMerge(int source, int target)
    {
        if (gameState == null || IsDragBlocked) return;
        gameState.MoveOrMergeSlot(source, target);
        selected = target;
        Refresh();
        if (IsOpen && target >= 0 && target < slots.Count)
            slots[target].Focus();
    }

    private void Build()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        if (eventController == null) eventController = FindAnyObjectByType<EventController>();

        // Disable the old visual children while retaining the serialized controller instance.
        foreach (Transform child in transform) child.gameObject.SetActive(false);

        gameObject.SetActive(true);
        document = new HanseaticDocument(transform, "Inventory", 60);
        VisualElement root = document.Root;
        Art(root.Q("deck"), "Inventory/Deck");
        Art(root.Q("board"), "Inventory/GridFrame");
        grid = root.Q("slots");
        ghost = root.Q("drag-ghost");
        Button close = root.Q<Button>("close");
        DecorateAction(close);
        close.clicked += Hide;
        root.RegisterCallback<NavigationCancelEvent>(evt =>
        {
            Hide();
            evt.StopPropagation();
        });
        if (gameState != null) gameState.OnInventoryChanged += Refresh;
    }

    public void Refresh()
    {
        if (!IsOpen || gameState == null) return;
        int count = gameState.InventorySlotCount;
        int rows = Mathf.Max(4, Mathf.CeilToInt(count / (float)Columns));
        while (slots.Count < count) AddSlot(slots.Count);
        if (selected < 0 || selected >= count)
        {
            selected = -1;
            for (int i = 0; i < count; i++)
            {
                if (gameState.Inventory.GetSlot(i)?.IsEmpty != false) continue;
                selected = i;
                break;
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            Button slot = slots[i];
            Show(slot, i < count);
            if (i >= count) continue;
            slot.style.left = Length.Percent(i % Columns * 100f / Columns);
            slot.style.top = Length.Percent(i / Columns * 100f / rows);
            slot.style.width = Length.Percent(100f / Columns);
            slot.style.height = Length.Percent(100f / rows);
            slot.EnableInClassList("selected", i == selected);

            InventorySlotData item = gameState.Inventory.GetSlot(i);
            bool filled = item?.IsEmpty == false;
            slot.Q("icon").style.backgroundImage = filled ? new StyleBackground(item.Item.Icon) : new StyleBackground(StyleKeyword.None);
            slot.Q<Label>("quantity").text = filled ? $"×{item.Quantity}" : "";
            slot.tooltip = filled ? item.Item.DisplayName : "Пустая ячейка";
        }

        InventorySlotData selectedItem = selected >= 0 ? gameState.Inventory.GetSlot(selected) : null;
        bool hasItem = selectedItem?.IsEmpty == false;
        document.Root.Q<Label>("title").text = hasItem ? selectedItem.Item.DisplayName : "Груз";
        document.Root.Q<Label>("description").text = hasItem
            ? $"{selectedItem.Item.Description}\n\nКоличество: {selectedItem.Quantity}".TrimStart()
            : "Выберите товар, чтобы посмотреть сведения о нём.";
    }

    private void AddSlot(int index)
    {
        Button slot = new(() =>
        {
            if (IsDragging) return;
            selected = index;
            Refresh();
        }) { name = $"slot-{index}" };
        slot.AddToClassList("cargo-slot");
        VisualElement icon = new() { name = "icon", pickingMode = PickingMode.Ignore };
        icon.AddToClassList("cargo-icon");
        slot.Add(icon);
        Label quantity = new() { name = "quantity", pickingMode = PickingMode.Ignore };
        quantity.AddToClassList("cargo-quantity");
        slot.Add(quantity);
        slot.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.button != 0 || pressed >= 0 || IsDragBlocked || gameState.Inventory.GetSlot(index)?.IsEmpty != false) return;
            pressed = index;
            pointerId = evt.pointerId;
            origin = evt.position;
        }, TrickleDown.TrickleDown);
        slot.RegisterCallback<PointerMoveEvent>(evt =>
        {
            if (pressed != index || evt.pointerId != pointerId || (evt.pressedButtons & 1) == 0) return;
            Vector2 position = evt.position;
            if (!IsDragging && (position - origin).sqrMagnitude < 100f) return;
            if (!IsDragging)
            {
                IsDragging = true;
                slot.CapturePointer(pointerId);
                ghost.style.backgroundImage = new StyleBackground(gameState.Inventory.GetSlot(index).Item.Icon);
                Show(ghost, true);
            }
            Vector2 local = ghost.parent.WorldToLocal(position);
            ghost.style.left = local.x - 40;
            ghost.style.top = local.y - 40;
        }, TrickleDown.TrickleDown);
        slot.RegisterCallback<PointerUpEvent>(evt =>
        {
            if (pressed != index || evt.pointerId != pointerId) return;
            bool dragging = IsDragging;
            Vector2 position = evt.position;
            CancelDrag();
            if (!dragging) return;
            evt.StopImmediatePropagation();
            for (int target = 0; target < gameState.InventorySlotCount; target++)
            {
                if (target != index && slots[target].worldBound.Contains(position))
                {
                    MoveOrMerge(index, target);
                    break;
                }
            }
        }, TrickleDown.TrickleDown);
        slot.RegisterCallback<PointerCaptureOutEvent>(_ =>
        {
            if (pressed == index) CancelDrag();
        });
        slots.Add(slot);
        grid.Add(slot);
    }

    private void CancelDrag()
    {
        int old = pressed;
        int pointer = pointerId;
        pressed = pointerId = -1;
        IsDragging = false;
        if (ghost != null) Show(ghost, false);
        if (old >= 0 && slots[old].HasPointerCapture(pointer))
            slots[old].ReleasePointer(pointer);
    }
}
