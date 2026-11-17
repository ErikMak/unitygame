using UnityEngine;

[CreateAssetMenu(fileName = "GoodItem", menuName = "Hanseatic/Goods/Good Item")]
public sealed class GoodItem : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea] private string description;
    [SerializeField, Min(1)] private int maxStack = 5;
    [SerializeField, Min(0)] private int novgorodBuyPrice;
    [SerializeField, Min(0)] private int novgorodSellPrice;
    [SerializeField, Min(0)] private int torzhokBuyPrice;
    [SerializeField, Min(0)] private int torzhokSellPrice;

    public string Id { get => id; set => id = value; }
    public string DisplayName { get => displayName; set => displayName = value; }
    public string Description => description;
    public Sprite Icon { get => icon; set => icon = value; }
    public int MaxStack { get => maxStack; set => maxStack = Mathf.Max(1, value); }
    public int NovgorodBuyPrice { get => novgorodBuyPrice; set => novgorodBuyPrice = Mathf.Max(0, value); }
    public int NovgorodSellPrice { get => novgorodSellPrice; set => novgorodSellPrice = Mathf.Max(0, value); }
    public int TorzhokBuyPrice { get => torzhokBuyPrice; set => torzhokBuyPrice = Mathf.Max(0, value); }
    public int TorzhokSellPrice { get => torzhokSellPrice; set => torzhokSellPrice = Mathf.Max(0, value); }

    public int GetBuyPrice(LocationId location)
    {
        return location == LocationId.Torzhok ? torzhokBuyPrice : novgorodBuyPrice;
    }

    public int GetSellPrice(LocationId location)
    {
        return location == LocationId.Torzhok ? torzhokSellPrice : novgorodSellPrice;
    }

    private void OnValidate()
    {
        maxStack = Mathf.Max(1, maxStack);
        novgorodBuyPrice = Mathf.Max(0, novgorodBuyPrice);
        novgorodSellPrice = Mathf.Max(0, novgorodSellPrice);
        torzhokBuyPrice = Mathf.Max(0, torzhokBuyPrice);
        torzhokSellPrice = Mathf.Max(0, torzhokSellPrice);
    }
}
