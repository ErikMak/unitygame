using System;
using UnityEngine.UIElements;
using static HanseaticDocument;

internal sealed class HanseaticMenuView
{
    private readonly VisualElement root;
    private readonly GraphicsSettingsUI graphics;
    public VisualElement Fade => root.Q("fade");

    public HanseaticMenuView(VisualElement root, CustomSceneManager navigation, GraphicsSettingsUI graphics, AudioSettingsUI audio)
    {
        this.root = root;
        this.graphics = graphics;
        Art(root.Q("ship"), "Menu/Ship");
        Art(root.Q("logo"), "Menu/Logo");
        Art(root.Q("menu-ink"), "Menu/Decoration");
        Art(root.Q("settings-ink"), "Menu/Decoration");
        Art(root.Q("settings-decoration"), "Menu/SettingsDecoration");
        root.Q<Button>("continue").SetEnabled(false);
        Wire("new-game", navigation.LoadMapScene);
        Wire("settings-button", navigation.ShowSettings);
        Wire("quit", navigation.QuitGame);
        foreach (Button item in root.Q("menu").Query<Button>().ToList())
        {
            if (!item.enabledSelf) continue;
            item.RegisterCallback<FocusInEvent>(_ => Art(item, "Menu/Focus"));
            item.RegisterCallback<FocusOutEvent>(_ => item.style.backgroundImage = StyleKeyword.Null);
        }
        DecorateAction(root.Q<Button>("save"), "Menu/GoldFinial");
        Wire("save", () =>
        {
            if (graphics != null) graphics.ApplyCurrentSettings();
            navigation.ShowMainMenu();
        });

        BindVolume(0, audio != null ? audio.MasterVolume : 1f, audio != null ? audio.SetMasterVolume : null);
        BindVolume(1, audio != null ? audio.MusicVolume : 1f, audio != null ? audio.SetMusicVolume : null);
        BindVolume(2, audio != null ? audio.SoundVolume : 1f, audio != null ? audio.SetSfxVolume : null);
        root.Q<Label>("value3").text = "Высокое";
        root.Q<Label>("value5").text = "Включен";
        BindArrows(3, null);
        BindArrows(4, graphics == null ? null : graphics.StepResolution);
        BindArrows(5, null);
        BindArrows(6, graphics == null ? null : _ => graphics.ToggleDisplayMode());
    }

    public void SetPage(bool settings)
    {
        Show(root.Q("menu"), !settings);
        Show(root.Q("settings"), settings);
        RefreshGraphics();
        Button focus = root.Q<Button>(settings ? "save" : "new-game");
        focus.schedule.Execute(() => focus.Focus());
    }

    private void Wire(string name, Action action) => root.Q<Button>(name).clicked += action;

    private void RefreshGraphics()
    {
        if (graphics == null) return;
        root.Q<Label>("value4").text = graphics.CurrentResolutionLabel;
        root.Q<Label>("value6").text = graphics.IsFullScreen ? "Полноэкранный" : "Оконный";
    }

    private void BindArrows(int index, Action<int> step)
    {
        for (int direction = -1; direction <= 1; direction += 2)
        {
            int delta = direction;
            Button arrow = root.Q<Button>((delta < 0 ? "prev" : "next") + index);
            arrow.SetEnabled(step != null);
            arrow.clicked += () =>
            {
                step?.Invoke(delta);
                RefreshGraphics();
            };
        }
    }

    private void BindVolume(int index, float fraction, Action<float> apply)
    {
        SliderInt slider = root.Q<SliderInt>("volume" + index);
        Label value = root.Q<Label>("value" + index);
        slider.SetValueWithoutNotify(UnityEngine.Mathf.RoundToInt(fraction * 100));
        slider.SetEnabled(apply != null);
        value.text = slider.value.ToString();
        VisualElement dragger = slider.Q(className: "unity-base-slider__dragger");
        if (dragger != null) Art(dragger, "Menu/Diamond");
        slider.RegisterValueChangedCallback(change =>
        {
            value.text = change.newValue.ToString();
            apply?.Invoke(change.newValue / 100f);
        });
    }
}
