using UnityEngine;
using UnityEngine.UIElements;

internal sealed class HanseaticDocument : System.IDisposable
{
    private const string RootPath = "Hanseatic/";
    private readonly GameObject host;
    private readonly PanelSettings panel;
    public VisualElement Root { get; }

    public HanseaticDocument(Transform parent, string template, int sortingOrder)
    {
        panel = ScriptableObject.CreateInstance<PanelSettings>();
        panel.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        panel.referenceResolution = new Vector2Int(1920, 1080);
        panel.screenMatchMode = PanelScreenMatchMode.Expand;
        panel.sortingOrder = sortingOrder;
        panel.themeStyleSheet = Resources.Load<ThemeStyleSheet>(RootPath + "Toolkit/Hanseatic");
        host = new GameObject(template + " UI Toolkit");
        host.SetActive(false);
        host.transform.SetParent(parent, false);
        UIDocument document = host.AddComponent<UIDocument>();
        document.panelSettings = panel;
        HanseaticDocumentHost viewHost = host.AddComponent<HanseaticDocumentHost>();
        host.SetActive(true);

        Root = Resources.Load<VisualTreeAsset>(RootPath + "Toolkit/" + template).Instantiate();
        Root.AddToClassList("hanseatic-root");
        Root.styleSheets.Add(Resources.Load<StyleSheet>(RootPath + "Toolkit/Common"));
        Root.styleSheets.Add(Resources.Load<StyleSheet>(RootPath + "Toolkit/" + template + "Layout"));
        Root.style.unityFontDefinition = FontDefinition.FromFont(Resources.Load<Font>(RootPath + "RobotoSlab-Regular"));
        viewHost.Bind(document, Root);
    }

    public static void Art(VisualElement target, string path)
    {
        target.style.backgroundImage = new StyleBackground(Resources.Load<Sprite>(RootPath + path));
    }

    public static Button ServiceAction(string title, System.Action invoke)
    {
        Button button = new(invoke) { text = title, name = "action" };
        button.AddToClassList("outlined-action");
        button.AddToClassList("service-action");
        button.Add(new VisualElement { name = "action-left", pickingMode = PickingMode.Ignore });
        button.Add(new VisualElement { name = "action-right", pickingMode = PickingMode.Ignore });
        DecorateAction(button);
        return button;
    }

    public static void DecorateAction(Button button, string finial = "Events/ButtonFinial")
    {
        VisualElement left = button.Q(button.name + "-left");
        VisualElement right = button.Q(button.name + "-right");
        left.AddToClassList("finial-left");
        right.AddToClassList("finial-right");
        Art(left, finial);
        Art(right, finial);
    }

    public static void Show(VisualElement element, bool visible)
    {
        element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(host);
        UnityEngine.Object.Destroy(panel);
    }
}
