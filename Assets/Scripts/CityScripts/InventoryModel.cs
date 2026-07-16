using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class InventoryModel
{
    [SerializeField] private List<InventorySlotData> slots = new();

    public IReadOnlyList<InventorySlotData> Slots => slots;
    public int SlotCount => slots.Count;

    public void EnsureSlotCount(int count)
    {
        count = Mathf.Max(1, count);
        slots ??= new List<InventorySlotData>();

        while (slots.Count < count)
        {
            slots.Add(new InventorySlotData());
        }

        while (slots.Count > count)
        {
            slots.RemoveAt(slots.Count - 1);
        }

        Normalize();
    }

    public InventorySlotData GetSlot(int index)
    {
        return index >= 0 && index < slots.Count ? slots[index] : null;
    }

    public void ClearInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i]?.Clear();
        }
    }

    public void Normalize()
    {
        slots ??= new List<InventorySlotData>();
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i] ??= new InventorySlotData();
            slots[i].Normalize();
        }
    }

    public InventoryModel Clone()
    {
        InventoryModel clone = new();
        clone.slots = new List<InventorySlotData>(slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            clone.slots.Add(slots[i]?.Clone() ?? new InventorySlotData());
        }

        return clone;
    }

    public void CopyFrom(InventoryModel other)
    {
        slots.Clear();
        if (other == null)
        {
            return;
        }

        for (int i = 0; i < other.slots.Count; i++)
        {
            slots.Add(other.slots[i]?.Clone() ?? new InventorySlotData());
        }

        Normalize();
    }
}
