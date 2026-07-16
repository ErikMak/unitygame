using UnityEngine;

public sealed class MarketController : MonoBehaviour
{
    [SerializeField] private LocationId marketLocation = LocationId.Novgorod;
    [SerializeField] private GameState gameState;

    private void Awake()
    {
        gameState = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
    }

    public bool TryBuy(GoodItem item)
    {
        if (gameState == null || item == null)
        {
            return false;
        }

        int price = item.GetBuyPrice(marketLocation);
        if (gameState.Gold < price)
        {
            return false;
        }

        if (!gameState.TryAddItem(item, 1))
        {
            return false;
        }

        return gameState.TrySpendGold(price);
    }

    public bool TrySell(GoodItem item)
    {
        if (gameState == null || item == null || !gameState.TryRemoveItem(item, 1))
        {
            return false;
        }

        gameState.AddGold(item.GetSellPrice(marketLocation));
        return true;
    }
}
