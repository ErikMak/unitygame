using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class GraphicsSettingsUI : MonoBehaviour
{
    private const int DefaultWidth = 1920;
    private const int DefaultHeight = 1080;
    private const bool DefaultFullScreen = true;
    private const bool DefaultVSync = true;

    private const string ResolutionWidthKey = "Graphics.ResolutionWidth";
    private const string ResolutionHeightKey = "Graphics.ResolutionHeight";
    private const string FullScreenKey = "Graphics.FullScreen";
    private const string VSyncKey = "Graphics.VSync";

    [Header("UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    private readonly List<Vector2Int> availableResolutions = new();

    private void Awake()
    {
        BuildResolutionList();
        LoadSettings();
        ApplyCurrentSettings(saveSettings: false);
    }

    private void OnEnable()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.AddListener(OnFullScreenChanged);

        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
    }

    private void OnDisable()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        if (fullScreenToggle != null)
            fullScreenToggle.onValueChanged.RemoveListener(OnFullScreenChanged);

        if (vSyncToggle != null)
            vSyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
    }

    private void BuildResolutionList()
    {
        availableResolutions.Clear();

        if (resolutionDropdown == null)
        {
            Debug.LogError(
                "GraphicsSettingsUI: ResolutionDropdown не назначен.");
            return;
        }

        HashSet<string> addedResolutions = new();

        int monitorWidth = Display.main.systemWidth;
        int monitorHeight = Display.main.systemHeight;

        // Разрешения, которые сообщает монитор.
        foreach (Resolution resolution in Screen.resolutions)
        {
            AddResolution(
                resolution.width,
                resolution.height,
                monitorWidth,
                monitorHeight,
                addedResolutions);
        }

        // Стандартные разрешения для оконного режима.
        Vector2Int[] standardResolutions =
        {
        new Vector2Int(1024, 576),
        new Vector2Int(1280, 720),
        new Vector2Int(1280, 800),
        new Vector2Int(1366, 768),
        new Vector2Int(1440, 900),
        new Vector2Int(1600, 900),
        new Vector2Int(1680, 1050),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160)
    };

        foreach (Vector2Int resolution in standardResolutions)
        {
            AddResolution(
                resolution.x,
                resolution.y,
                monitorWidth,
                monitorHeight,
                addedResolutions);
        }

        // Нативное разрешение монитора добавляется обязательно.
        AddResolution(
            monitorWidth,
            monitorHeight,
            monitorWidth,
            monitorHeight,
            addedResolutions);

        // На случай некорректных данных дисплея.
        if (availableResolutions.Count == 0)
        {
            availableResolutions.Add(
                new Vector2Int(DefaultWidth, DefaultHeight));
        }

        availableResolutions.Sort((first, second) =>
        {
            long firstPixels = (long)first.x * first.y;
            long secondPixels = (long)second.x * second.y;

            return firstPixels.CompareTo(secondPixels);
        });

        List<string> dropdownOptions = new();

        foreach (Vector2Int resolution in availableResolutions)
        {
            dropdownOptions.Add(
                $"{resolution.x} × {resolution.y}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(dropdownOptions);
        resolutionDropdown.RefreshShownValue();

        Debug.Log(
            $"GraphicsSettingsUI: найдено разрешений: " +
            $"{availableResolutions.Count}. " +
            $"Монитор: {monitorWidth} × {monitorHeight}");
    }

    private void AddResolution(
        int width,
        int height,
        int monitorWidth,
        int monitorHeight,
        HashSet<string> addedResolutions)
    {
        if (width <= 0 || height <= 0)
            return;

        // Не показываем разрешения больше текущего монитора.
        if (width > monitorWidth || height > monitorHeight)
            return;

        string key = $"{width}x{height}";

        if (!addedResolutions.Add(key))
            return;

        availableResolutions.Add(
            new Vector2Int(width, height));
    }

    private void AddFallbackResolution(int width, int height)
    {
        Vector2Int newResolution = new(width, height);

        if (!availableResolutions.Contains(newResolution))
            availableResolutions.Add(newResolution);
    }

    private void LoadSettings()
    {
        if (availableResolutions.Count == 0)
            return;

        int savedWidth = PlayerPrefs.GetInt(
            ResolutionWidthKey,
            DefaultWidth);

        int savedHeight = PlayerPrefs.GetInt(
            ResolutionHeightKey,
            DefaultHeight);

        bool fullScreen = PlayerPrefs.GetInt(
            FullScreenKey,
            DefaultFullScreen ? 1 : 0) == 1;

        bool vSync = PlayerPrefs.GetInt(
            VSyncKey,
            DefaultVSync ? 1 : 0) == 1;

        int resolutionIndex = FindResolutionIndex(
            savedWidth,
            savedHeight);

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            resolutionDropdown.RefreshShownValue();
        }

        if (fullScreenToggle != null)
            fullScreenToggle.SetIsOnWithoutNotify(fullScreen);

        if (vSyncToggle != null)
            vSyncToggle.SetIsOnWithoutNotify(vSync);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];

            if (resolution.x == width && resolution.y == height)
                return i;
        }

        return FindClosestResolutionIndex(width, height);
    }

    private int FindClosestResolutionIndex(int width, int height)
    {
        int closestIndex = 0;
        long smallestDifference = long.MaxValue;

        for (int i = 0; i < availableResolutions.Count; i++)
        {
            Vector2Int resolution = availableResolutions[i];

            long widthDifference = resolution.x - width;
            long heightDifference = resolution.y - height;

            long totalDifference =
                widthDifference * widthDifference +
                heightDifference * heightDifference;

            if (totalDifference >= smallestDifference)
                continue;

            smallestDifference = totalDifference;
            closestIndex = i;
        }

        return closestIndex;
    }

    private void OnResolutionChanged(int _)
    {
        ApplyCurrentSettings(saveSettings: true);
    }

    private void OnFullScreenChanged(bool _)
    {
        ApplyCurrentSettings(saveSettings: true);
    }

    private void OnVSyncChanged(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        PlayerPrefs.SetInt(VSyncKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ApplyCurrentSettings()
    {
        ApplyCurrentSettings(saveSettings: true);
    }

    private void ApplyCurrentSettings(bool saveSettings)
    {
        if (availableResolutions.Count == 0)
            return;

        int selectedIndex = resolutionDropdown != null
            ? resolutionDropdown.value
            : 0;

        selectedIndex = Mathf.Clamp(
            selectedIndex,
            0,
            availableResolutions.Count - 1);

        Vector2Int selectedResolution =
            availableResolutions[selectedIndex];

        bool isFullScreen =
            fullScreenToggle == null ||
            fullScreenToggle.isOn;

        bool isVSyncEnabled =
            vSyncToggle == null ||
            vSyncToggle.isOn;

        FullScreenMode screenMode = isFullScreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(
            selectedResolution.x,
            selectedResolution.y,
            screenMode);

        QualitySettings.vSyncCount =
            isVSyncEnabled ? 1 : 0;

        if (saveSettings)
        {
            SaveSettings(
                selectedResolution,
                isFullScreen,
                isVSyncEnabled);
        }
    }

    private void SaveSettings(
        Vector2Int resolution,
        bool fullScreen,
        bool vSync)
    {
        PlayerPrefs.SetInt(
            ResolutionWidthKey,
            resolution.x);

        PlayerPrefs.SetInt(
            ResolutionHeightKey,
            resolution.y);

        PlayerPrefs.SetInt(
            FullScreenKey,
            fullScreen ? 1 : 0);

        PlayerPrefs.SetInt(
            VSyncKey,
            vSync ? 1 : 0);

        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Graphics Settings")]
    public void ResetGraphicsSettings()
    {
        int resolutionIndex = FindResolutionIndex(
            DefaultWidth,
            DefaultHeight);

        resolutionDropdown?.SetValueWithoutNotify(
            resolutionIndex);

        resolutionDropdown?.RefreshShownValue();

        fullScreenToggle?.SetIsOnWithoutNotify(
            DefaultFullScreen);

        vSyncToggle?.SetIsOnWithoutNotify(
            DefaultVSync);

        ApplyCurrentSettings(saveSettings: true);
    }

    [ContextMenu("Test Dropdown")]
    private void TestDropdown()
    {
        if (resolutionDropdown == null)
        {
            Debug.LogError("ResolutionDropdown не назначен.");
            return;
        }

        Debug.Log(
            $"Dropdown active: {resolutionDropdown.gameObject.activeInHierarchy}\n" +
            $"Interactable: {resolutionDropdown.interactable}\n" +
            $"Template: {resolutionDropdown.template}\n" +
            $"Options: {resolutionDropdown.options.Count}");

        resolutionDropdown.Show();
    }
}