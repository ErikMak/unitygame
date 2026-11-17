using System.Collections.Generic;
using UnityEngine;

public sealed class GraphicsSettingsUI : MonoBehaviour
{
    private const string WidthKey = "Graphics.ResolutionWidth";
    private const string HeightKey = "Graphics.ResolutionHeight";
    private const string FullScreenKey = "Graphics.FullScreen";
    private readonly List<Vector2Int> resolutions = new();
    private int selected;
    public bool IsFullScreen { get; private set; } = true;

    public string CurrentResolutionLabel => resolutions.Count == 0 ? "" :
        $"{resolutions[selected].x} × {resolutions[selected].y}";

    private void Awake()
    {
        int width = Display.main.systemWidth;
        int height = Display.main.systemHeight;
        HashSet<Vector2Int> candidates = new()
        {
            new(1024, 576), new(1280, 720), new(1280, 800), new(1366, 768),
            new(1440, 900), new(1600, 900), new(1680, 1050), new(1920, 1080),
            new(2560, 1440), new(3840, 2160)
        };
        foreach (Resolution resolution in Screen.resolutions)
            candidates.Add(new Vector2Int(resolution.width, resolution.height));
        candidates.Add(new Vector2Int(width, height));
        foreach (Vector2Int candidate in candidates)
            if (candidate.x > 0 && candidate.y > 0 && candidate.x <= width && candidate.y <= height)
                resolutions.Add(candidate);

        if (resolutions.Count == 0) resolutions.Add(new Vector2Int(1920, 1080));
        resolutions.Sort((a, b) =>
        {
            int pixels = (a.x * a.y).CompareTo(b.x * b.y);
            return pixels == 0 ? a.x.CompareTo(b.x) : pixels;
        });
        SelectClosest(PlayerPrefs.GetInt(WidthKey, 1920), PlayerPrefs.GetInt(HeightKey, 1080));
        IsFullScreen = PlayerPrefs.GetInt(FullScreenKey, 1) == 1;
        Apply(save: false);
    }

    public void StepResolution(int direction)
    {
        if (resolutions.Count == 0) return;
        selected = (selected + (direction < 0 ? -1 : 1) + resolutions.Count) % resolutions.Count;
    }

    public void ToggleDisplayMode() => IsFullScreen = !IsFullScreen;
    public void ApplyCurrentSettings() => Apply(save: true);

    public void ResetGraphicsSettings()
    {
        SelectClosest(1920, 1080);
        IsFullScreen = true;
        ApplyCurrentSettings();
    }

    private void SelectClosest(int width, int height)
    {
        float bestDistance = float.PositiveInfinity;
        for (int i = 0; i < resolutions.Count; i++)
        {
            float distance = (resolutions[i] - new Vector2Int(width, height)).sqrMagnitude;
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            selected = i;
        }
    }

    private void Apply(bool save)
    {
        if (resolutions.Count == 0) return;
        Vector2Int resolution = resolutions[selected];
        Screen.SetResolution(resolution.x, resolution.y, IsFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        if (!save) return;
        PlayerPrefs.SetInt(WidthKey, resolution.x);
        PlayerPrefs.SetInt(HeightKey, resolution.y);
        PlayerPrefs.SetInt(FullScreenKey, IsFullScreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
