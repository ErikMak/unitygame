using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public sealed class AudioSettingsUI : MonoBehaviour
{
    private const string MasterParameter = "MasterVolume";
    private const string MusicParameter = "MusicVolume";
    private const string SfxParameter = "SFXVolume";
    private const string VoicesParameter = "VoicesVolume";

    private const string MasterPrefsKey = "Audio.Master";
    private const string MusicPrefsKey = "Audio.Music";
    private const string SfxPrefsKey = "Audio.SFX";
    private const string VoicesPrefsKey = "Audio.Voices";

    private const float DefaultVolume = 1f;
    private const float MinDecibels = -80f;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixerForGame;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider voicesSlider;


private void Awake()
    {
        ConfigureSettingsLayout();
        ConfigureSlider(masterSlider);
        ConfigureSlider(musicSlider);
        ConfigureSlider(sfxSlider);
        ConfigureSlider(voicesSlider);

        LoadSettings();
        ApplyAllVolumes();
    }

private void ConfigureSettingsLayout()
    {
        RectTransform settingsRoot = FindSettingsRoot();
        if (settingsRoot == null)
        {
            return;
        }

        LayoutSlider(masterSlider, settingsRoot, new Vector2(245f, 104f));
        LayoutSlider(musicSlider, settingsRoot, new Vector2(245f, 24f));
        LayoutSlider(sfxSlider, settingsRoot, new Vector2(245f, -56f));
        LayoutSlider(voicesSlider, settingsRoot, new Vector2(245f, -136f));
    }

    private RectTransform FindSettingsRoot()
    {
        Slider source = masterSlider != null ? masterSlider : musicSlider;
        Transform current = source != null ? source.transform : null;
        while (current != null)
        {
            if (current.name == "SettingsContainer")
            {
                return current as RectTransform;
            }

            current = current.parent;
        }

        return null;
    }

private static void LayoutSlider(Slider slider, RectTransform settingsRoot, Vector2 position)
    {
        if (slider == null)
        {
            return;
        }

        RectTransform rect = slider.GetComponent<RectTransform>();
        rect.SetParent(settingsRoot, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(360f, 24f);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.localPosition = new Vector3(rect.localPosition.x, rect.localPosition.y, 0f);
        slider.gameObject.SetActive(true);

        TMPro.TMP_Text[] nestedLabels = slider.GetComponentsInChildren<TMPro.TMP_Text>(true);
        foreach (TMPro.TMP_Text nestedLabel in nestedLabels)
        {
            if (nestedLabel.name.StartsWith("AudioLabel_"))
            {
                nestedLabel.gameObject.SetActive(false);
            }
        }

        RectTransform background = slider.transform.Find("Background") as RectTransform;
        if (background != null)
        {
            background.anchorMin = new Vector2(0f, 0.5f);
            background.anchorMax = new Vector2(1f, 0.5f);
            background.anchoredPosition = Vector2.zero;
            background.sizeDelta = new Vector2(-20f, 8f);
        }

        if (slider.fillRect != null && slider.fillRect.parent is RectTransform fillArea)
        {
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.anchoredPosition = Vector2.zero;
            fillArea.sizeDelta = new Vector2(-28f, 8f);
            slider.fillRect.anchorMin = Vector2.zero;
            slider.fillRect.anchorMax = Vector2.one;
            slider.fillRect.offsetMin = Vector2.zero;
            slider.fillRect.offsetMax = Vector2.zero;
        }

        if (slider.handleRect != null && slider.handleRect.parent is RectTransform handleArea)
        {
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(12f, 0f);
            handleArea.offsetMax = new Vector2(-12f, 0f);
            slider.handleRect.sizeDelta = new Vector2(18f, 18f);
        }
    }


    private void OnEnable()
    {
        AddListeners();
    }

    private void OnDisable()
    {
        RemoveListeners();
        PlayerPrefs.Save();
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void LoadSettings()
    {
        SetSliderWithoutNotification(
            masterSlider,
            PlayerPrefs.GetFloat(MasterPrefsKey, DefaultVolume));

        SetSliderWithoutNotification(
            musicSlider,
            PlayerPrefs.GetFloat(MusicPrefsKey, DefaultVolume));

        SetSliderWithoutNotification(
            sfxSlider,
            PlayerPrefs.GetFloat(SfxPrefsKey, DefaultVolume));

        SetSliderWithoutNotification(
            voicesSlider,
            PlayerPrefs.GetFloat(VoicesPrefsKey, DefaultVolume));
    }

    private void SetSliderWithoutNotification(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    private void AddListeners()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSfxVolume);

        if (voicesSlider != null)
            voicesSlider.onValueChanged.AddListener(SetVoicesVolume);
    }

    private void RemoveListeners()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.RemoveListener(SetMasterVolume);

        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(SetSfxVolume);

        if (voicesSlider != null)
            voicesSlider.onValueChanged.RemoveListener(SetVoicesVolume);
    }

    public void SetMasterVolume(float value)
    {
        SetMixerVolume(MasterParameter, value);
        PlayerPrefs.SetFloat(MasterPrefsKey, Mathf.Clamp01(value));
    }

    public void SetMusicVolume(float value)
    {
        SetMixerVolume(MusicParameter, value);
        PlayerPrefs.SetFloat(MusicPrefsKey, Mathf.Clamp01(value));
    }

    public void SetSfxVolume(float value)
    {
        SetMixerVolume(SfxParameter, value);
        PlayerPrefs.SetFloat(SfxPrefsKey, Mathf.Clamp01(value));
    }

    public void SetVoicesVolume(float value)
    {
        SetMixerVolume(VoicesParameter, value);
        PlayerPrefs.SetFloat(VoicesPrefsKey, Mathf.Clamp01(value));
    }

    private void ApplyAllVolumes()
    {
        if (masterSlider != null)
            SetMixerVolume(MasterParameter, masterSlider.value);

        if (musicSlider != null)
            SetMixerVolume(MusicParameter, musicSlider.value);

        if (sfxSlider != null)
            SetMixerVolume(SfxParameter, sfxSlider.value);

        if (voicesSlider != null)
            SetMixerVolume(VoicesParameter, voicesSlider.value);
    }

    private void SetMixerVolume(string parameterName, float normalizedValue)
    {
        if (audioMixerForGame == null)
        {
            Debug.LogWarning(
                $"AudioSettingsUI: AudioMixer �� ��������. " +
                $"�������� {parameterName} �� ������.");
            return;
        }

        normalizedValue = Mathf.Clamp01(normalizedValue);

        float decibels = normalizedValue <= 0.0001f
            ? MinDecibels
            : Mathf.Log10(normalizedValue) * 20f;

        if (!audioMixerForGame.SetFloat(parameterName, decibels))
        {
            Debug.LogWarning(
                $"AudioSettingsUI: �������� \"{parameterName}\" " +
                $"�� ������ ����� Exposed Parameters AudioMixer.");
        }
    }

    [ContextMenu("Reset Audio Settings")]
    public void ResetAudioSettings()
    {
        SetSliderWithoutNotification(masterSlider, DefaultVolume);
        SetSliderWithoutNotification(musicSlider, DefaultVolume);
        SetSliderWithoutNotification(sfxSlider, DefaultVolume);
        SetSliderWithoutNotification(voicesSlider, DefaultVolume);

        SetMasterVolume(DefaultVolume);
        SetMusicVolume(DefaultVolume);
        SetSfxVolume(DefaultVolume);
        SetVoicesVolume(DefaultVolume);

        PlayerPrefs.Save();
    }
}