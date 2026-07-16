using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private TMP_Text emptyText;

    private InventoryPanelUI panel;
    private int slotIndex;
    private InventorySlotData currentData;

    public int SlotIndex => slotIndex;
    public InventorySlotData CurrentData => currentData;

    public void Initialize(InventoryPanelUI owner, int index)
    {
        panel = owner;
        slotIndex = index;
    }

    public void Refresh(InventorySlotData data)
    {
        currentData = data;
        bool hasItem = data != null && !data.IsEmpty;

        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(hasItem);
            itemIcon.sprite = hasItem ? data.Item.Icon : null;
        }

        if (itemName != null)
        {
            itemName.gameObject.SetActive(hasItem);
            itemName.text = hasItem ? data.Item.DisplayName : string.Empty;
        }

        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(hasItem);
            quantityText.text = hasItem ? $"x{data.Quantity}" : string.Empty;
        }

        if (emptyText != null)
        {
            emptyText.gameObject.SetActive(!hasItem);
            emptyText.text = "Пусто";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (panel == null || panel.IsDragBlocked || currentData == null || currentData.IsEmpty)
        {
            return;
        }

        panel.DraggedItem?.Show(currentData.Item, currentData.Quantity, eventData.position);
        eventData.pointerDrag = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        panel?.DraggedItem?.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        panel?.DraggedItem?.Hide();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (panel == null || panel.IsDragBlocked)
        {
            return;
        }

        InventorySlotUI source = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<InventorySlotUI>() : null;
        if (source != null)
        {
            panel.MoveOrMerge(source.SlotIndex, slotIndex);
        }
    }
}
