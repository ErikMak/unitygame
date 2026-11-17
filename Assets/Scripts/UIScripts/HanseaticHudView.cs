using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static HanseaticDocument;

internal sealed class HanseaticHudView : System.IDisposable
{
    private readonly HanseaticDocument document;
    private VisualElement Root => document.Root;
    public Label Time => Root.Q<Label>("time-value");
    public Button Location => Root.Q<Button>("location");
    public Label Status => Root.Q<Label>("status");
    public Label Summary => Root.Q<Label>("summary");
    public Button Inventory => Root.Q<Button>("inventory");
    public Button Leave => Root.Q<Button>("leave");
    public bool IsMenuOpen => Root.Q("confirm").style.display.value == DisplayStyle.Flex;

    public HanseaticHudView(Transform parent, bool city)
    {
        document = new HanseaticDocument(parent, "Hud", 20);
        Root.style.backgroundColor = Color.clear;
        Root.pickingMode = PickingMode.Ignore;
        foreach (VisualElement child in Root.Children())
            child.pickingMode = PickingMode.Ignore;

        Root.Q("frame").AddToClassList(city ? "city" : "journey");
        foreach (string name in new[] { "relations", "inventory", "leave", "menu" })
        {
            Art(Root.Q(name + "-icon"), "Hud/" + char.ToUpperInvariant(name[0]) + name.Substring(1));
        }
        Art(Root.Q("sun-icon"), "Hud/Sun");
        Root.Q<Button>("relations").SetEnabled(false);
        Inventory.tooltip = "Инвентарь";
        Leave.tooltip = "Покинуть город";
        Root.Q<Button>("menu").tooltip = "Главное меню";
        Location.tooltip = city ? "Места в городе" : "Сведения о путешествии";
        Location.clicked += () =>
        {
            VisualElement details = Root.Q("details");
            Show(details, details.style.display.value != DisplayStyle.Flex);
        };

        Show(Root.Q("details"), false);
        VisualElement confirmation = Root.Q("confirm");
        Show(confirmation, false);
        DecorateAction(Root.Q<Button>("cancel"));
        DecorateAction(Root.Q<Button>("quit"));
        Root.Q<Button>("menu").clicked += () =>
        {
            Show(confirmation, true);
            Root.Q<Button>("cancel").Focus();
        };
        Root.Q<Button>("cancel").clicked += () =>
        {
            Show(confirmation, false);
            Root.Q<Button>("menu").Focus();
        };
        Root.Q<Button>("quit").clicked += () => SceneManager.LoadScene("MainMenu");
    }

    public Button CityAction(string caption)
    {
        Button action = new() { text = caption };
        action.AddToClassList("city-action");
        Root.Q("city-actions").Add(action);
        return action;
    }

    public void Dispose() => document.Dispose();
}
