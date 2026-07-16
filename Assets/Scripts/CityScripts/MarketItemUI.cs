using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MarketItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GoodItem item;
    [SerializeField] private MarketController marketController;

    public GoodItem Item => item;

    public void OnBeginDrag(PointerEventData eventData) { }
    public void OnDrag(PointerEventData eventData) { }
    public void OnEndDrag(PointerEventData eventData) { }

    public void BuyOne()
    {
        marketController ??= FindAnyObjectByType<MarketController>();
        marketController?.TryBuy(item);
    }
}
