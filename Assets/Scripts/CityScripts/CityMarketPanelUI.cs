using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CityMarketPanelUI : MonoBehaviour
{
    [SerializeField] private MarketController marketController;
    [SerializeField] private GoodItem[] goods;
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Button[] sellButtons;
    [SerializeField] private TMP_Text[] itemTexts;
    [SerializeField] private TMP_Text feedbackText;

    private GameState gameState;

    private void Awake()
    {
        gameState = GameState.Instance;
        if (marketController != null && gameState != null)
        {
            marketController.SetLocation(gameState.CurrentLocation);
        }

        WireButtons();
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void WireButtons()
    {
        for (int i = 0; i < goods.Length; i++)
        {
            int index = i;
            if (i < buyButtons.Length && buyButtons[i] != null)
            {
                buyButtons[i].onClick.RemoveAllListeners();
                buyButtons[i].onClick.AddListener(() => Buy(index));
            }

            if (i < sellButtons.Length && sellButtons[i] != null)
            {
                sellButtons[i].onClick.RemoveAllListeners();
                sellButtons[i].onClick.AddListener(() => Sell(index));
            }
        }
    }

    private void Buy(int index)
    {
        GoodItem item = GetGood(index);
        bool success = item != null && marketController != null && marketController.TryBuy(item);
        SetFeedback(success ? $"Куплено: {item.DisplayName}" : "Покупка невозможна: проверьте золото и свободные слоты.");
        Refresh();
    }

    private void Sell(int index)
    {
        GoodItem item = GetGood(index);
        bool success = item != null && marketController != null && marketController.TrySell(item);
        SetFeedback(success ? $"Продано: {item.DisplayName}" : "В грузе нет этого товара.");
        Refresh();
    }

    private void Refresh()
    {
        gameState ??= GameState.Instance;
        if (gameState == null)
        {
            return;
        }

        for (int i = 0; i < goods.Length && i < itemTexts.Length; i++)
        {
            GoodItem item = goods[i];
            if (item == null || itemTexts[i] == null)
            {
                continue;
            }

            itemTexts[i].text = $"{item.DisplayName}  Купить: {item.GetBuyPrice(gameState.CurrentLocation)}  Продать: {item.GetSellPrice(gameState.CurrentLocation)}  В грузе: {gameState.GetItemQuantity(item)}";
        }
    }

    private GoodItem GetGood(int index)
    {
        return index >= 0 && index < goods.Length ? goods[index] : null;
    }

    private void SetFeedback(string text)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
        }
    }
}
