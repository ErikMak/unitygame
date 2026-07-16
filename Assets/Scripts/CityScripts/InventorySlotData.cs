using System;
using UnityEngine;

[Serializable]
public sealed class InventorySlotData
{
    [SerializeField] private GoodItem item;
    [SerializeField, Min(0)] private int quantity;

    public GoodItem Item { get => item; set => item = value; }
    public int Quantity { get => quantity; set => quantity = Mathf.Max(0, value); }
    public bool IsEmpty => item == null || quantity <= 0;

    public void Set(GoodItem newItem, int newQuantity)
    {
        item = newItem;
        quantity = Mathf.Max(0, newQuantity);
        Normalize();
    }

    public void Clear()
    {
        item = null;
        quantity = 0;
    }

    public void Normalize()
    {
        if (item == null || quantity <= 0)
        {
            Clear();
            return;
        }

        quantity = Mathf.Min(quantity, item.MaxStack);
    }

    public InventorySlotData Clone()
    {
        InventorySlotData clone = new();
        clone.Set(item, quantity);
        return clone;
    }
}
