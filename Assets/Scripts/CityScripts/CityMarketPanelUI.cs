using UnityEngine;
using UnityEngine.UIElements;
using static HanseaticDocument;

public sealed class CityMarketPanelUI : MonoBehaviour
{
    [SerializeField] private MarketController marketController;
    [SerializeField] private GoodItem[] goods;

    private HanseaticDocument document;
    private GameState state;
    private Label summary;
    private Label[] rows;

    private void OnEnable()
    {
        state = GameState.Instance != null ? GameState.Instance : FindAnyObjectByType<GameState>();
        if (marketController != null && state != null)
            marketController.SetLocation(state.CurrentLocation);
        if (document == null) Build();
        Refresh();
    }

    private void OnDisable()
    {
        document?.Dispose();
        document = null;
    }

    private void Build()
    {
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        document = new HanseaticDocument(transform, "ServicePanel", 60);
        VisualElement root = document.Root;
        root.Q<Label>("title").text = "Рынок";
        summary = root.Q<Label>("summary");
        VisualElement body = root.Q<ScrollView>("body").contentContainer;
        rows = new Label[goods.Length];
        for (int i = 0; i < goods.Length; i++)
        {
            GoodItem item = goods[i];
            if (item == null) continue;
            VisualElement row = new();
            row.AddToClassList("service-row");
            rows[i] = new Label();
            row.Add(rows[i]);
            row.Add(ServiceAction("Купить", () => Trade(item, true)));
            row.Add(ServiceAction("Продать", () => Trade(item, false)));
            body.Add(row);
        }

        Button close = ServiceAction("Закрыть рынок", () => gameObject.SetActive(false));
        root.Q("actions").Add(close);
        close.schedule.Execute(() => close.Focus());
    }

    private void Trade(GoodItem item, bool buy)
    {
        if (marketController == null) return;
        bool ok = buy ? marketController.TryBuy(item) : marketController.TrySell(item);
        Refresh();
        summary.text += ok ? " · Сделка завершена" : " · Сделка недоступна";
    }

    private void Refresh()
    {
        if (state == null) return;
        summary.text = $"Золото: {state.Gold}";
        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null) continue;
            GoodItem item = goods[i];
            rows[i].text = $"{item.DisplayName}   {item.GetBuyPrice(state.CurrentLocation)} / {item.GetSellPrice(state.CurrentLocation)}   В грузе: {state.GetItemQuantity(item)}";
        }
    }
}
