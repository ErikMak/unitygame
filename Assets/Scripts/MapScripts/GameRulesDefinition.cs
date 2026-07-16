using UnityEngine;

[CreateAssetMenu(fileName = "GameRulesDefinition", menuName = "Hanseatic/Rules/Game Rules Definition")]
public sealed class GameRulesDefinition : ScriptableObject
{
    [SerializeField, Min(0)] private int startingGold = 100;
    [SerializeField, Min(1)] private int inventorySlotCount = 8;
    [SerializeField, Min(1)] private int maxStackPerSlot = 5;
    [SerializeField, Min(0)] private int requiredGoldForSuccess = 160;
    [SerializeField, Min(0)] private int maximumDaysForSuccess = 18;

    public int StartingGold => startingGold;
    public int InventorySlotCount => inventorySlotCount;
    public int MaxStackPerSlot => maxStackPerSlot;
    public int RequiredGoldForSuccess => requiredGoldForSuccess;
    public int MaximumDaysForSuccess => maximumDaysForSuccess;

    private void OnValidate()
    {
        startingGold = Mathf.Max(0, startingGold);
        inventorySlotCount = Mathf.Max(1, inventorySlotCount);
        maxStackPerSlot = Mathf.Max(1, maxStackPerSlot);
        requiredGoldForSuccess = Mathf.Max(0, requiredGoldForSuccess);
        maximumDaysForSuccess = Mathf.Max(0, maximumDaysForSuccess);
    }
}
