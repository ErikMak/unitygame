using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HanseaticUIRestyler
{
    private static readonly Color Surface = new(0.055f, 0.045f, 0.04f, 0.94f);
    private static readonly Color SurfaceSoft = new(0.09f, 0.072f, 0.058f, 0.92f);
    private static readonly Color SurfaceRaised = new(0.14f, 0.105f, 0.075f, 0.96f);
    private static readonly Color Gold = new(0.72f, 0.54f, 0.28f, 1f);
    private static readonly Color GoldBright = new(0.9f, 0.73f, 0.43f, 1f);
    private static readonly Color Ivory = new(0.94f, 0.91f, 0.84f, 1f);
    private static readonly Color Muted = new(0.72f, 0.68f, 0.61f, 1f);
    private static readonly Color Accent = new(0.55f, 0.2f, 0.13f, 1f);

    [MenuItem("Tools/Hanseatic/Restyle All UI")]
    public static void RestyleAll()
    {
        string previousScene = SceneManager.GetActiveScene().path;

        RestylePrefab("Assets/Prefabs/UI/GameHUD.prefab", root => StyleHud(root, false));
        RestylePrefab("Assets/Prefabs/UI/InventoryPanel.prefab", StyleInventoryPanel);
        RestylePrefab("Assets/Prefabs/UI/InventorySlot.prefab", StyleInventorySlot);

        RestyleScene("Assets/Scenes/MainMenu.unity", StyleMainMenu);
        RestyleScene("Assets/Scenes/MapScene.unity", StyleMapScene);
        RestyleScene("Assets/Scenes/CityScene.unity", StyleCityScene);
        RestyleScene("Assets/Scenes/EndScene.unity", StyleEndScene);

        if (!string.IsNullOrEmpty(previousScene))
        {
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Hanseatic UI: restyling completed for all gameplay scenes.");
    }

    private static void RestyleScene(string path, Action<Scene> action)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        action(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void RestylePrefab(string path, Action<GameObject> action)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            action(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleMainMenu(Scene scene)
    {
        Canvas canvas = FindInScene<Canvas>(scene);
        if (canvas == null) return;

        ConfigureCanvas(canvas, 0);
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Transform map = FindDeep(canvas.transform, "MapBG");
        if (map != null)
        {
            Stretch((RectTransform)map, 0f);
            Image mapImage = map.GetComponent<Image>();
            if (mapImage != null)
            {
                mapImage.preserveAspect = false;
                mapImage.raycastTarget = false;
                mapImage.color = Color.white;
            }
        }

        Image tint = EnsureImage(canvas.transform, "MenuTint", new Color(0.035f, 0.03f, 0.026f, 0.48f));
        Stretch(tint.rectTransform, 0f);
        tint.raycastTarget = false;
        tint.transform.SetSiblingIndex(Mathf.Min(1, canvas.transform.childCount - 1));

        RectTransform title = FindRect(canvas.transform, "MainMenuText");
        if (title != null)
        {
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -76f), new Vector2(760f, 96f), new Vector2(0.5f, 1f));
            TMP_Text text = title.GetComponent<TMP_Text>();
            StyleText(text, 54f, Ivory, TextAlignmentOptions.Center, FontStyles.Bold);
            text.text = "ГАНЗЕЙСКИЙ ПУТЬ";
            AddShadow(title.gameObject, new Color(0f, 0f, 0f, 0.8f), new Vector2(2f, -3f));
        }

        RectTransform main = FindRect(canvas.transform, "MainContainer");
        if (main != null)
        {
            SetRect(main, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -30f), new Vector2(430f, 330f), Vector2.one * 0.5f);
            StylePanel(main.gameObject, Surface, Gold, true);
            StyleButton(FindDeep(main, "PlayButton"), "ИГРАТЬ", 74f);
            StyleButton(FindDeep(main, "SettingsButton"), "НАСТРОЙКИ", 0f);
            StyleButton(FindDeep(main, "QuitButton"), "ВЫХОД", -74f);
        }

        RectTransform settings = FindRect(canvas.transform, "SettingsContainer");
        if (settings != null)
        {
            SetRect(settings, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(980f, 620f), Vector2.one * 0.5f);
            StylePanel(settings.gameObject, Surface, Gold, true);
            LayoutSettings(settings);
        }

        Image fade = FindDeep(canvas.transform, "FadeImage")?.GetComponent<Image>();
        if (fade != null)
        {
            Stretch(fade.rectTransform, 0f);
        }

        canvasRect.localScale = Vector3.one;
    }

    private static void LayoutSettings(RectTransform settings)
    {
        RectTransform title = FindRect(settings, "SettingsText");
        if (title != null)
        {
            title.SetParent(settings, false);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(720f, 62f), new Vector2(0.5f, 1f));
            StyleText(title.GetComponent<TMP_Text>(), 34f, Ivory, TextAlignmentOptions.Center, FontStyles.Bold);
            title.GetComponent<TMP_Text>().text = "НАСТРОЙКИ";
        }

        TMP_Text videoHeader = EnsureText(settings, "VideoHeader", "ИЗОБРАЖЕНИЕ", 21f, GoldBright);
        SetRect(videoHeader.rectTransform, new Vector2(0.25f, 1f), new Vector2(0.25f, 1f), new Vector2(0f, -112f), new Vector2(380f, 38f), new Vector2(0.5f, 1f));
        TMP_Text audioHeader = EnsureText(settings, "AudioHeader", "ЗВУК", 21f, GoldBright);
        SetRect(audioHeader.rectTransform, new Vector2(0.75f, 1f), new Vector2(0.75f, 1f), new Vector2(0f, -112f), new Vector2(380f, 38f), new Vector2(0.5f, 1f));

        Image videoSurface = EnsureImage(settings, "VideoSurface", new Color(0.025f, 0.022f, 0.02f, 0.32f));
        SetRect(videoSurface.rectTransform, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(-245f, -18f), new Vector2(420f, 390f), Vector2.one * 0.5f);
        videoSurface.raycastTarget = false;
        videoSurface.transform.SetSiblingIndex(0);
        Image audioSurface = EnsureImage(settings, "AudioSurface", new Color(0.025f, 0.022f, 0.02f, 0.32f));
        SetRect(audioSurface.rectTransform, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(245f, -18f), new Vector2(420f, 390f), Vector2.one * 0.5f);
        audioSurface.raycastTarget = false;
        audioSurface.transform.SetSiblingIndex(1);

        GraphicsSettingsUI graphics = UnityEngine.Object.FindFirstObjectByType<GraphicsSettingsUI>(FindObjectsInactive.Include);
        if (graphics != null)
        {
            SerializedObject so = new(graphics);
            TMP_Dropdown dropdown = so.FindProperty("resolutionDropdown").objectReferenceValue as TMP_Dropdown;
            Toggle fullscreen = so.FindProperty("fullScreenToggle").objectReferenceValue as Toggle;
            Toggle vsync = so.FindProperty("vSyncToggle").objectReferenceValue as Toggle;
            LayoutDropdown(settings, dropdown, new Vector2(-245f, 112f));
            LayoutToggle(settings, fullscreen, "ПОЛНОЭКРАННЫЙ РЕЖИМ", new Vector2(-245f, 8f));
            LayoutToggle(settings, vsync, "ВЕРТИКАЛЬНАЯ СИНХРОНИЗАЦИЯ", new Vector2(-245f, -64f));
        }

        AudioSettingsUI audio = UnityEngine.Object.FindFirstObjectByType<AudioSettingsUI>(FindObjectsInactive.Include);
        if (audio != null)
        {
            SerializedObject so = new(audio);
            string[] fields = { "masterSlider", "musicSlider", "sfxSlider", "voicesSlider" };
            string[] labels = { "ОБЩАЯ ГРОМКОСТЬ", "МУЗЫКА", "ЭФФЕКТЫ", "ГОЛОСА" };
            float[] ys = { 120f, 40f, -40f, -120f };
            for (int i = 0; i < fields.Length; i++)
            {
                Slider slider = so.FindProperty(fields[i]).objectReferenceValue as Slider;
                LayoutSlider(settings, slider, labels[i], new Vector2(245f, ys[i]));
            }
        }

        Transform videoContainer = FindDeep(settings, "VideoContainer");
        Transform audioContainer = FindDeep(settings, "AudioContainer");
        if (videoContainer != null) videoContainer.gameObject.SetActive(false);
        if (audioContainer != null) audioContainer.gameObject.SetActive(false);

        RectTransform back = FindRect(settings, "BackButton");
        if (back != null)
        {
            back.SetParent(settings, false);
            back.gameObject.SetActive(true);
            StyleButton(back, "НАЗАД", -258f, 240f);
        }
    }

    private static void LayoutDropdown(RectTransform parent, TMP_Dropdown dropdown, Vector2 position)
    {
        if (dropdown == null) return;
        dropdown.transform.SetParent(parent, false);
        dropdown.gameObject.SetActive(true);
        SetRect(dropdown.GetComponent<RectTransform>(), Vector2.one * 0.5f, Vector2.one * 0.5f, position, new Vector2(360f, 52f), Vector2.one * 0.5f);
        Image image = dropdown.GetComponent<Image>();
        if (image != null) image.color = SurfaceRaised;
        AddOutline(dropdown.gameObject, new Color(Gold.r, Gold.g, Gold.b, 0.55f), new Vector2(1f, -1f));
        ColorBlock dropdownColors = dropdown.colors;
        dropdownColors.normalColor = Color.white;
        dropdownColors.highlightedColor = new Color(1f, 0.93f, 0.8f, 1f);
        dropdownColors.pressedColor = new Color(0.85f, 0.72f, 0.5f, 1f);
        dropdown.colors = dropdownColors;
        StyleText(dropdown.captionText, 18f, Ivory, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        TMP_Text label = EnsureText(parent, "ResolutionLabel", "РАЗРЕШЕНИЕ", 15f, Muted);
        SetRect(label.rectTransform, Vector2.one * 0.5f, Vector2.one * 0.5f, position + new Vector2(0f, 44f), new Vector2(360f, 28f), Vector2.one * 0.5f);
        label.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void LayoutToggle(RectTransform parent, Toggle toggle, string caption, Vector2 position)
    {
        if (toggle == null) return;
        RectTransform carrier = toggle.transform.parent as RectTransform;
        if (carrier == null) carrier = toggle.GetComponent<RectTransform>();
        carrier.SetParent(parent, false);
        carrier.gameObject.SetActive(true);
        toggle.gameObject.SetActive(true);
        SetRect(carrier, Vector2.one * 0.5f, Vector2.one * 0.5f, position, new Vector2(360f, 52f), Vector2.one * 0.5f);
        SetRect(toggle.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.one * 0.5f);
        Image row = EnsureComponent<Image>(carrier.gameObject);
        row.color = SurfaceRaised;
        row.raycastTarget = false;
        Transform oldBackground = FindDeep(carrier, "ToggleContainerBG");
        if (oldBackground != null) oldBackground.gameObject.SetActive(false);
        TMP_Text text = toggle.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            text.text = caption;
            StyleText(text, 16f, Ivory, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-76f, 0f), new Vector2(0.5f, 0.5f));
        }
        if (toggle.graphic != null)
        {
            RectTransform check = toggle.graphic.rectTransform;
            SetRect(check, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-27f, 0f), new Vector2(24f, 24f), Vector2.one * 0.5f);
            check.GetComponent<Image>().color = GoldBright;
        }
    }

    private static void LayoutSlider(RectTransform parent, Slider slider, string caption, Vector2 position)
    {
        if (slider == null) return;
        RectTransform carrier = slider.transform.parent as RectTransform;
        if (carrier == null) carrier = slider.GetComponent<RectTransform>();
        carrier.SetParent(parent, false);
        carrier.gameObject.SetActive(true);
        slider.gameObject.SetActive(true);
        SetRect(carrier, Vector2.one * 0.5f, Vector2.one * 0.5f, position, new Vector2(360f, 58f), Vector2.one * 0.5f);
        SetRect(slider.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 4f), new Vector2(0f, 22f), new Vector2(0.5f, 0f));
        Transform oldBackground = FindDeep(carrier, "SliderContainerBG");
        if (oldBackground != null) oldBackground.gameObject.SetActive(false);
        Transform oldLabel = FindDeep(carrier, "SliderNameText");
        if (oldLabel != null) oldLabel.gameObject.SetActive(false);
        if (slider.fillRect != null && slider.fillRect.GetComponent<Image>() is Image fill) fill.color = Gold;
        if (slider.handleRect != null && slider.handleRect.GetComponent<Image>() is Image handle) handle.color = GoldBright;
        TMP_Text label = EnsureText(carrier, "AudioLabel_" + caption, caption, 15f, Muted);
        SetRect(label.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -2f), new Vector2(0f, 24f), new Vector2(0.5f, 1f));
        label.alignment = TextAlignmentOptions.MidlineLeft;
    }

    private static void StyleMapScene(Scene scene)
    {
        GameHUD hud = FindInScene<GameHUD>(scene);
        if (hud != null) StyleHud(hud.gameObject, false);
    }

    private static void StyleCityScene(Scene scene)
    {
        Canvas cityCanvas = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Canvas>(true)).FirstOrDefault(c => c.name == "Canvas");
        if (cityCanvas != null) ConfigureCanvas(cityCanvas, 0, new Vector2(1920f, 1080f));

        GameHUD hud = FindInScene<GameHUD>(scene);
        if (hud != null) StyleHud(hud.gameObject, true);

        Transform redundantCityName = cityCanvas != null ? FindDeep(cityCanvas.transform, "CityNameText") : null;
        if (redundantCityName != null) redundantCityName.gameObject.SetActive(false);

        Transform market = cityCanvas != null ? FindDeep(cityCanvas.transform, "MarketPanel") : null;
        if (market != null) StyleMarketPanel((RectTransform)market);
    }

    private static void StyleHud(GameObject root, bool cityVariant)
    {
        Canvas canvas = EnsureComponent<Canvas>(root);
        ConfigureCanvas(canvas, 20);
        EnsureComponent<GraphicRaycaster>(root);

        RectTransform topBar = FindRect(root.transform, "TopBar");
        if (topBar != null)
        {
            HorizontalLayoutGroup horizontal = topBar.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null) horizontal.enabled = false;
            ContentSizeFitter fitter = topBar.GetComponent<ContentSizeFitter>();
            if (fitter != null) fitter.enabled = false;

            if (cityVariant)
            {
                SetRect(topBar, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 18f), new Vector2(310f, 128f), new Vector2(1f, 0f));
                StylePanel(topBar.gameObject, Surface, Gold, true);
                LayoutCityHud(topBar);
            }
            else
            {
                SetRect(topBar, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -12f), new Vector2(-32f, 92f), new Vector2(0.5f, 1f));
                StylePanel(topBar.gameObject, Surface, Gold, true);
                LayoutMapHud(topBar);
            }
        }

        RectTransform inventory = FindRect(root.transform, "InventoryPanel");
        if (inventory != null)
        {
            StyleInventoryPanel(inventory.gameObject);
            SetRect(inventory, cityVariant ? Vector2.one * 0.5f : Vector2.one,
                cityVariant ? Vector2.one * 0.5f : Vector2.one,
                cityVariant ? Vector2.zero : new Vector2(-18f, -116f),
                new Vector2(600f, 360f),
                cityVariant ? Vector2.one * 0.5f : Vector2.one);
        }
    }

    private static void LayoutMapHud(RectTransform topBar)
    {
        LayoutHudBlock(topBar, "GoldBlock", 12f, 142f);
        LayoutHudBlock(topBar, "DaysBlock", 162f, 150f);
        LayoutHudBlock(topBar, "JourneyBlock", 320f, 404f);
        LayoutHudBlock(topBar, "GoalBlock", 732f, 182f);
        LayoutHudBlock(topBar, "CargoBlock", 922f, 310f);

        StyleHudText(topBar, "GoldValue");
        StyleHudText(topBar, "DaysValue");
        StyleHudText(topBar, "GoalValue");
        StyleHudText(topBar, "CargoValue");

        RectTransform journeyText = FindRect(topBar, "JourneyStatusText");
        if (journeyText != null)
        {
            if (journeyText.parent is RectTransform wrapper && wrapper.name == "Text")
            {
                Stretch(wrapper, 0f);
            }
            SetRect(journeyText, new Vector2(0f, 0.46f), Vector2.one, new Vector2(12f, -8f), new Vector2(-24f, -6f), new Vector2(0.5f, 0.5f));
            TMP_Text text = journeyText.GetComponent<TMP_Text>();
            StyleText(text, 15f, Ivory, TextAlignmentOptions.TopLeft, FontStyles.Normal);
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
        }

        RectTransform progress = FindRect(topBar, "JourneyProgressBar");
        if (progress != null)
        {
            SetRect(progress, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(12f, 10f), new Vector2(-24f, 9f), new Vector2(0.5f, 0f));
            progress.GetComponent<Image>().color = new Color(0.22f, 0.17f, 0.12f, 1f);
            Transform fill = FindDeep(progress, "Fill");
            if (fill != null && fill.GetComponent<Image>() is Image image) image.color = Accent;
        }

        RectTransform cargo = FindRect(topBar, "CargoBlock");
        RectTransform inventoryButton = FindRect(topBar, "InventoryButton");
        if (cargo != null && inventoryButton != null)
        {
            SetRect(inventoryButton, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-68f, 0f), new Vector2(124f, 42f), Vector2.one * 0.5f);
            StyleButtonVisual(inventoryButton.gameObject, "ИНВЕНТАРЬ", 15f);
            RectTransform cargoValue = FindRect(cargo, "CargoValue");
            if (cargoValue != null) SetRect(cargoValue, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-144f, 0f), Vector2.one * 0.5f);
        }
    }

    private static void LayoutCityHud(RectTransform topBar)
    {
        string[] blockNames = { "GoldBlock", "DaysBlock", "CargoBlock" };
        for (int i = 0; i < blockNames.Length; i++)
        {
            RectTransform block = FindRect(topBar, blockNames[i]);
            if (block == null) continue;
            DisableLayoutDrivers(block);
            block.gameObject.SetActive(true);
            SetRect(block, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -10f - i * 38f), new Vector2(-20f, 32f), new Vector2(0.5f, 1f));
            Image image = EnsureComponent<Image>(block.gameObject);
            image.color = i % 2 == 0 ? SurfaceRaised : SurfaceSoft;
            image.raycastTarget = false;
        }

        Transform journey = FindDeep(topBar, "JourneyBlock");
        Transform goal = FindDeep(topBar, "GoalBlock");
        Transform inventory = FindDeep(topBar, "InventoryButton");
        if (journey != null) journey.gameObject.SetActive(false);
        if (goal != null) goal.gameObject.SetActive(false);
        if (inventory != null) inventory.gameObject.SetActive(false);
        StyleHudText(topBar, "GoldValue");
        StyleHudText(topBar, "DaysValue");
        StyleHudText(topBar, "CargoValue");
    }

    private static void LayoutHudBlock(RectTransform parent, string name, float x, float width)
    {
        RectTransform block = FindRect(parent, name);
        if (block == null) return;
        DisableLayoutDrivers(block);
        block.gameObject.SetActive(true);
        SetRect(block, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(width, 68f), new Vector2(0f, 0.5f));
        Image image = EnsureComponent<Image>(block.gameObject);
        image.color = SurfaceSoft;
        image.raycastTarget = false;
    }

    private static void StyleHudText(Transform root, string name)
    {
        RectTransform rect = FindRect(root, name);
        if (rect == null) return;
        if (rect.parent is RectTransform wrapper && wrapper.name == "Text")
        {
            Stretch(wrapper, 0f);
        }
        SetRect(rect, Vector2.zero, Vector2.one, new Vector2(12f, 0f), new Vector2(-24f, 0f), Vector2.one * 0.5f);
        TMP_Text text = rect.GetComponent<TMP_Text>();
        StyleText(text, 15f, Ivory, TextAlignmentOptions.MidlineLeft, FontStyles.Bold);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void StyleInventoryPanel(GameObject root)
    {
        RectTransform panel = root.GetComponent<RectTransform>();
        StylePanel(root, Surface, Gold, true);

        TMP_Text title = EnsureText(panel, "InventoryTitle", "ГРУЗОВОЙ ТРЮМ", 20f, Ivory);
        SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -6f), new Vector2(-88f, 32f), new Vector2(0.5f, 1f));
        title.alignment = TextAlignmentOptions.MidlineLeft;
        title.fontStyle = FontStyles.Bold;

        Button close = EnsureButton(panel, "CloseButton", "×");
        SetRect(close.GetComponent<RectTransform>(), Vector2.one, Vector2.one, new Vector2(-20f, -18f), new Vector2(42f, 42f), Vector2.one);
        StyleButtonVisual(close.gameObject, "×", 28f);

        InventoryPanelUI ui = root.GetComponent<InventoryPanelUI>();
        if (ui != null)
        {
            SerializedObject so = new(ui);
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        RectTransform grid = FindRect(panel, "SlotsGrid");
        if (grid != null)
        {
            SetRect(grid, Vector2.zero, Vector2.one, new Vector2(24f, 22f), new Vector2(-48f, -88f), Vector2.one * 0.5f);
            GridLayoutGroup layout = EnsureComponent<GridLayoutGroup>(grid.gameObject);
            layout.cellSize = new Vector2(120f, 112f);
            layout.spacing = new Vector2(12f, 12f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(0, 0, 0, 0);
        }

        RectTransform dragged = FindRect(panel, "DraggedItem");
        if (dragged != null)
        {
            dragged.SetAsLastSibling();
        }
    }

    private static void StyleInventorySlot(GameObject root)
    {
        RectTransform rect = root.GetComponent<RectTransform>();
        if (rect != null) rect.sizeDelta = new Vector2(120f, 112f);
        Image image = EnsureComponent<Image>(root);
        image.color = SurfaceRaised;
        AddOutline(root, new Color(Gold.r, Gold.g, Gold.b, 0.55f), new Vector2(1f, -1f));

        TMP_Text empty = FindDeep(root.transform, "EmptyText")?.GetComponent<TMP_Text>();
        StyleText(empty, 14f, Muted, TextAlignmentOptions.Center, FontStyles.Italic);
        TMP_Text item = FindDeep(root.transform, "ItemName")?.GetComponent<TMP_Text>();
        StyleText(item, 13f, Ivory, TextAlignmentOptions.Bottom, FontStyles.Bold);
        TMP_Text quantity = FindDeep(root.transform, "QuantityText")?.GetComponent<TMP_Text>();
        StyleText(quantity, 15f, GoldBright, TextAlignmentOptions.TopRight, FontStyles.Bold);
    }

    private static void StyleMarketPanel(RectTransform panel)
    {
        SetRect(panel, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(920f, 560f), Vector2.one * 0.5f);
        StylePanel(panel.gameObject, Surface, Gold, true);

        RectTransform title = FindRect(panel, "Title");
        if (title != null)
        {
            SetRect(title, new Vector2(0f, 1f), Vector2.one, new Vector2(34f, -20f), new Vector2(-68f, 52f), new Vector2(0.5f, 1f));
            StyleText(title.GetComponent<TMP_Text>(), 30f, Ivory, TextAlignmentOptions.Center, FontStyles.Bold);
        }

        for (int i = 0; i < 4; i++)
        {
            float y = 132f - i * 88f;
            RectTransform item = FindRect(panel, "Item_" + i);
            if (item != null)
            {
                SetRect(item, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(-105f, y), new Vector2(590f, 70f), Vector2.one * 0.5f);
                Image row = EnsureComponent<Image>(item.gameObject);
                if (row != null)
                {
                    row.color = i % 2 == 0 ? SurfaceRaised : SurfaceSoft;
                    row.raycastTarget = false;
                }
                TMP_Text rowText = item.GetComponentInChildren<TMP_Text>(true);
                StyleText(rowText, 16f, Ivory, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
                if (rowText != null) SetRect(rowText.rectTransform, Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-36f, 0f), Vector2.one * 0.5f);
            }
            RectTransform buy = FindRect(panel, "Buy_" + i);
            RectTransform sell = FindRect(panel, "Sell_" + i);
            if (buy != null)
            {
                SetRect(buy, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(270f, y), new Vector2(120f, 46f), Vector2.one * 0.5f);
                StyleButtonVisual(buy.gameObject, "КУПИТЬ", 14f);
            }
            if (sell != null)
            {
                SetRect(sell, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(400f, y), new Vector2(120f, 46f), Vector2.one * 0.5f);
                StyleButtonVisual(sell.gameObject, "ПРОДАТЬ", 14f);
            }
        }

        RectTransform feedback = FindRect(panel, "Feedback");
        if (feedback != null)
        {
            SetRect(feedback, new Vector2(0f, 0f), new Vector2(0.72f, 0f), new Vector2(30f, 28f), new Vector2(-30f, 52f), new Vector2(0.5f, 0f));
            StyleText(feedback.GetComponent<TMP_Text>(), 15f, GoldBright, TextAlignmentOptions.MidlineLeft, FontStyles.Normal);
        }

        RectTransform close = FindRect(panel, "CloseMarketButton");
        if (close != null)
        {
            SetRect(close, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 26f), new Vector2(170f, 50f), new Vector2(1f, 0f));
            StyleButtonVisual(close.gameObject, "ЗАКРЫТЬ", 15f);
        }
    }

    private static void StyleEndScene(Scene scene)
    {
        Canvas canvas = FindInScene<Canvas>(scene);
        if (canvas == null) return;
        ConfigureCanvas(canvas, 0);

        RectTransform map = FindRect(canvas.transform, "EndScreenMap");
        if (map != null)
        {
            Stretch(map, 0f);
            Image image = map.GetComponent<Image>();
            if (image != null) image.preserveAspect = false;
        }

        RectTransform bg = FindRect(canvas.transform, "EndScreenBg");
        if (bg != null)
        {
            Stretch(bg, 0f);
            Graphic graphic = bg.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.color = new Color(0.025f, 0.022f, 0.02f, 0.58f);
                graphic.raycastTarget = false;
            }
        }

        Image panel = EnsureImage(canvas.transform, "EndContentPanel", Surface);
        SetRect(panel.rectTransform, Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(1120f, 640f), Vector2.one * 0.5f);
        StylePanel(panel.gameObject, Surface, Gold, true);
        panel.transform.SetSiblingIndex(Mathf.Min(2, canvas.transform.childCount - 1));

        RectTransform title = FindRect(canvas.transform, "EndScreenTitleText");
        if (title != null)
        {
            title.SetParent(canvas.transform, false);
            SetRect(title, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(900f, 96f), new Vector2(0.5f, 1f));
            StyleText(title.GetComponent<TMP_Text>(), 44f, Ivory, TextAlignmentOptions.Center, FontStyles.Bold);
            title.SetAsLastSibling();
        }

        RectTransform narrative = FindRect(canvas.transform, "NarrativeText");
        if (narrative != null)
        {
            narrative.SetParent(canvas.transform, false);
            SetRect(narrative, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(-270f, 18f), new Vector2(460f, 300f), Vector2.one * 0.5f);
            StyleText(narrative.GetComponent<TMP_Text>(), 22f, Ivory, TextAlignmentOptions.TopLeft, FontStyles.Normal);
            narrative.GetComponent<TMP_Text>().margin = new Vector4(18f, 18f, 18f, 18f);
            narrative.GetComponent<TMP_Text>().overflowMode = TextOverflowModes.Ellipsis;
            narrative.SetAsLastSibling();

            Image narrativePanel = EnsureImage(canvas.transform, "NarrativePanel", SurfaceSoft);
            SetRect(narrativePanel.rectTransform, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(-270f, 18f), new Vector2(500f, 340f), Vector2.one * 0.5f);
            AddOutline(narrativePanel.gameObject, new Color(Gold.r, Gold.g, Gold.b, 0.45f), new Vector2(1f, -1f));
            narrativePanel.raycastTarget = false;
            narrativePanel.transform.SetSiblingIndex(Mathf.Max(0, narrative.GetSiblingIndex() - 1));
        }

        Transform legacyStats = FindDeep(canvas.transform, "StatisticContainer");
        if (legacyStats != null)
        {
            Image statsImage = EnsureImage(canvas.transform, "StatisticsPanel", SurfaceSoft);
            RectTransform stats = statsImage.rectTransform;
            SetRect(stats, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(275f, 10f), new Vector2(470f, 340f), Vector2.one * 0.5f);
            DisableLayoutDrivers(stats);
            StylePanel(stats.gameObject, SurfaceSoft, new Color(Gold.r, Gold.g, Gold.b, 0.55f), false);
            TMP_Text[] statTexts = legacyStats.GetComponentsInChildren<TMP_Text>(true)
                .Concat(stats.GetComponentsInChildren<TMP_Text>(true))
                .Distinct()
                .ToArray();
            foreach (TMP_Text text in statTexts)
            {
                if (text.transform.parent != stats) text.transform.SetParent(stats, false);
            }
            legacyStats.gameObject.SetActive(false);
            stats.SetAsLastSibling();
            foreach (TMP_Text text in statTexts)
            {
                StyleText(text, 18f, Ivory, text.alignment, text.fontStyle);
            }
            LayoutStatisticTexts(statTexts);
        }

        Button[] buttons = canvas.GetComponentsInChildren<Button>(true)
            .Where(b => !IsChildOf(b.transform, "EventPopup"))
            .Take(3)
            .ToArray();
        float[] xs = { -320f, 0f, 320f };
        for (int i = 0; i < buttons.Length; i++)
        {
            RectTransform rect = buttons[i].GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(xs[i], 56f), new Vector2(260f, 56f), new Vector2(0.5f, 0f));
            string caption = buttons[i].GetComponentInChildren<TMP_Text>(true)?.text ?? "";
            StyleButtonVisual(buttons[i].gameObject, caption, 18f);
            rect.SetAsLastSibling();
        }
    }

    private static void LayoutStatisticTexts(TMP_Text[] texts)
    {
        Dictionary<string, TMP_Text> byName = texts.ToDictionary(t => t.name, t => t);
        for (int i = 0; i < 5; i++)
        {
            float y = 112f - i * 58f;
            if (byName.TryGetValue("ResultText (" + (i + 1) + ")", out TMP_Text label))
            {
                RectTransform rect = label.rectTransform;
                SetRect(rect, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(-62f, y), new Vector2(300f, 42f), Vector2.one * 0.5f);
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.margin = new Vector4(8f, 0f, 8f, 0f);
                label.overflowMode = TextOverflowModes.Ellipsis;
            }
            if (byName.TryGetValue("ResultText (" + (i + 6) + ")", out TMP_Text value))
            {
                RectTransform rect = value.rectTransform;
                SetRect(rect, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(170f, y), new Vector2(88f, 42f), Vector2.one * 0.5f);
                value.alignment = TextAlignmentOptions.MidlineRight;
                value.color = GoldBright;
                value.fontStyle = FontStyles.Bold;
                value.margin = Vector4.zero;
            }
        }
    }

    private static void ConfigureCanvas(Canvas canvas, int order)
    {
        ConfigureCanvas(canvas, order, new Vector2(1280f, 720f));
    }

    private static void ConfigureCanvas(Canvas canvas, int order, Vector2 referenceResolution)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        EnsureComponent<GraphicRaycaster>(canvas.gameObject);
    }

    private static void StyleButton(Transform transform, string caption, float y, float width = 330f)
    {
        if (transform is not RectTransform rect) return;
        SetRect(rect, Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, y), new Vector2(width, 58f), Vector2.one * 0.5f);
        StyleButtonVisual(rect.gameObject, caption, 20f);
    }

    private static void StyleButtonVisual(GameObject target, string caption, float fontSize)
    {
        Image image = EnsureComponent<Image>(target);
        image.color = SurfaceRaised;
        image.type = Image.Type.Simple;
        Button button = EnsureComponent<Button>(target);
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = SurfaceRaised;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.9f, 0.72f, 1f);
        colors.pressedColor = new Color(0.82f, 0.66f, 0.42f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.16f, 0.15f, 0.14f, 0.72f);
        colors.colorMultiplier = 1f;
        button.colors = colors;
        AddOutline(target, new Color(Gold.r, Gold.g, Gold.b, 0.78f), new Vector2(1f, -1f));

        Image accent = EnsureImage(target.transform, "AccentLine", GoldBright);
        SetRect(accent.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, -8f), new Vector2(0f, 0.5f));
        accent.raycastTarget = false;
        accent.transform.SetAsFirstSibling();

        TMP_Text text = target.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            if (!string.IsNullOrEmpty(caption)) text.text = caption;
            StyleText(text, fontSize, Ivory, TextAlignmentOptions.Center, FontStyles.Bold);
            Stretch(text.rectTransform, 10f);
        }
    }

    private static void StylePanel(GameObject target, Color fill, Color edge, bool shadow)
    {
        Image image = EnsureComponent<Image>(target);
        image.color = fill;
        image.type = Image.Type.Simple;
        AddOutline(target, edge, new Vector2(1.5f, -1.5f));
        if (shadow) AddShadow(target, new Color(0f, 0f, 0f, 0.72f), new Vector2(5f, -6f));
    }

    private static void StyleText(TMP_Text text, float size, Color color, TextAlignmentOptions alignment, FontStyles style)
    {
        if (text == null) return;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.enableAutoSizing = false;
        text.characterSpacing = 0f;
        text.raycastTarget = false;
    }

    private static Button EnsureButton(Transform parent, string name, string caption)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        if (existing == null) go.transform.SetParent(parent, false);
        TMP_Text text = go.GetComponentInChildren<TMP_Text>(true);
        if (text == null)
        {
            text = EnsureText(go.transform, "Label", caption, 18f, Ivory);
            Stretch(text.rectTransform, 4f);
        }
        return EnsureComponent<Button>(go);
    }

    private static TMP_Text EnsureText(Transform parent, string name, string value, float size, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        if (existing == null) go.transform.SetParent(parent, false);
        TMP_Text text = EnsureComponent<TextMeshProUGUI>(go);
        text.text = value;
        StyleText(text, size, color, TextAlignmentOptions.Center, FontStyles.Normal);
        return text;
    }

    private static Image EnsureImage(Transform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        if (existing == null) go.transform.SetParent(parent, false);
        Image image = EnsureComponent<Image>(go);
        image.color = color;
        return image;
    }

    private static void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = EnsureComponent<Outline>(target);
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
    }

    private static void AddShadow(GameObject target, Color color, Vector2 distance)
    {
        Shadow[] shadows = target.GetComponents<Shadow>();
        Shadow shadow = shadows.FirstOrDefault(s => s is not Outline) ?? target.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void DisableLayoutDrivers(RectTransform rect)
    {
        foreach (LayoutGroup group in rect.GetComponents<LayoutGroup>()) group.enabled = false;
        LayoutElement element = rect.GetComponent<LayoutElement>();
        if (element != null) element.enabled = false;
        ContentSizeFitter fitter = rect.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        return FindDeep(root, name) as RectTransform;
    }

    private static Transform FindDeep(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindDeep(root.GetChild(i), name);
            if (result != null) return result;
        }
        return null;
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true))
            .FirstOrDefault();
    }

    private static bool IsChildOf(Transform transform, string ancestorName)
    {
        while (transform != null)
        {
            if (transform.name == ancestorName) return true;
            transform = transform.parent;
        }
        return false;
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        return target.GetComponent<T>() ?? target.AddComponent<T>();
    }
}
