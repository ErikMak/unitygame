using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MarketSellZoneUI : MonoBehaviour, IDropHandler
{
    [SerializeField] private MarketController marketController;

    public void OnDrop(PointerEventData eventData)
    {
        InventorySlotUI slot = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<InventorySlotUI>() : null;
        GoodItem item = slot != null && slot.CurrentData != null ? slot.CurrentData.Item : null;
        marketController ??= FindAnyObjectByType<MarketController>();
        marketController?.TrySell(item);
    }
}
