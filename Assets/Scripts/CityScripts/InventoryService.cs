using UnityEngine;

public sealed class InventoryService
{
    private readonly InventoryModel inventory;

    public InventoryService(InventoryModel inventory)
    {
        this.inventory = inventory;
        this.inventory?.Normalize();
    }

    public bool TryAddItem(GoodItem item, int quantity)
    {
        if (inventory == null || item == null || quantity <= 0)
        {
            return false;
        }

        InventoryModel clone = inventory.Clone();
        if (!TryAddItemInternal(clone, item, quantity))
        {
            return false;
        }

        inventory.CopyFrom(clone);
        return true;
    }

    public bool TryRemoveItem(GoodItem item, int quantity)
    {
        if (inventory == null || item == null || quantity <= 0 || GetItemQuantity(item) < quantity)
        {
            return false;
        }

        int remaining = quantity;
        for (int i = 0; i < inventory.SlotCount && remaining > 0; i++)
        {
            InventorySlotData slot = inventory.GetSlot(i);
            if (slot == null || slot.Item != item)
            {
                continue;
            }

            int removed = Mathf.Min(slot.Quantity, remaining);
            slot.Set(slot.Item, slot.Quantity - removed);
            remaining -= removed;
        }

        inventory.Normalize();
        return true;
    }

    public bool TryRemoveAnyCargo(int quantity)
    {
        if (quantity <= 0)
        {
            return true;
        }

        string[] priority = { "cloth", "salt", "fur", "wax" };
        int total = 0;
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            total += inventory.GetSlot(i)?.Quantity ?? 0;
        }

        if (total < quantity)
        {
            return false;
        }

        int remaining = quantity;
        for (int p = 0; p < priority.Length && remaining > 0; p++)
        {
            for (int i = 0; i < inventory.SlotCount && remaining > 0; i++)
            {
                InventorySlotData slot = inventory.GetSlot(i);
                if (slot == null || slot.Item == null || slot.Item.Id != priority[p])
                {
                    continue;
                }

                int removed = Mathf.Min(slot.Quantity, remaining);
                slot.Set(slot.Item, slot.Quantity - removed);
                remaining -= removed;
            }
        }

        inventory.Normalize();
        return remaining == 0;
    }

    public bool MoveOrMergeSlot(int sourceIndex, int targetIndex)
    {
        if (sourceIndex == targetIndex)
        {
            return true;
        }

        InventorySlotData source = inventory.GetSlot(sourceIndex);
        InventorySlotData target = inventory.GetSlot(targetIndex);
        if (source == null || target == null || source.IsEmpty)
        {
            return false;
        }

        if (target.IsEmpty)
        {
            target.Set(source.Item, source.Quantity);
            source.Clear();
            return true;
        }

        if (target.Item == source.Item)
        {
            int capacity = target.Item.MaxStack - target.Quantity;
            if (capacity <= 0)
            {
                return false;
            }

            int moved = Mathf.Min(capacity, source.Quantity);
            target.Set(target.Item, target.Quantity + moved);
            source.Set(source.Item, source.Quantity - moved);
            return true;
        }

        return SwapSlots(sourceIndex, targetIndex);
    }

    public bool SwapSlots(int sourceIndex, int targetIndex)
    {
        InventorySlotData source = inventory.GetSlot(sourceIndex);
        InventorySlotData target = inventory.GetSlot(targetIndex);
        if (source == null || target == null)
        {
            return false;
        }

        GoodItem item = source.Item;
        int quantity = source.Quantity;
        source.Set(target.Item, target.Quantity);
        target.Set(item, quantity);
        return true;
    }

    public int GetItemQuantity(GoodItem item)
    {
        if (item == null || inventory == null)
        {
            return 0;
        }

        int quantity = 0;
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            InventorySlotData slot = inventory.GetSlot(i);
            if (slot != null && slot.Item == item)
            {
                quantity += slot.Quantity;
            }
        }

        return quantity;
    }

    public int GetOccupiedSlotCount()
    {
        int count = 0;
        for (int i = 0; i < inventory.SlotCount; i++)
        {
            if (inventory.GetSlot(i)?.IsEmpty == false)
            {
                count++;
            }
        }

        return count;
    }

    public bool HasItem(GoodItem item, int quantity)
    {
        return quantity <= 0 || GetItemQuantity(item) >= quantity;
    }

    public void ClearInventory()
    {
        inventory?.ClearInventory();
    }

    private static bool TryAddItemInternal(InventoryModel model, GoodItem item, int quantity)
    {
        int remaining = quantity;
        for (int i = 0; i < model.SlotCount && remaining > 0; i++)
        {
            InventorySlotData slot = model.GetSlot(i);
            if (slot == null || slot.Item != item || slot.Quantity >= item.MaxStack)
            {
                continue;
            }

            int added = Mathf.Min(item.MaxStack - slot.Quantity, remaining);
            slot.Set(item, slot.Quantity + added);
            remaining -= added;
        }

        for (int i = 0; i < model.SlotCount && remaining > 0; i++)
        {
            InventorySlotData slot = model.GetSlot(i);
            if (slot == null || !slot.IsEmpty)
            {
                continue;
            }

            int added = Mathf.Min(item.MaxStack, remaining);
            slot.Set(item, added);
            remaining -= added;
        }

        return remaining == 0;
    }
}
