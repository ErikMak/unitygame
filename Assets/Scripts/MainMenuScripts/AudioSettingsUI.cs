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
        ConfigureSlider(masterSlider);
        ConfigureSlider(musicSlider);
        ConfigureSlider(sfxSlider);
        ConfigureSlider(voicesSlider);

        LoadSettings();
        ApplyAllVolumes();
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
                $"AudioSettingsUI: AudioMixer не назначен. " +
                $"Параметр {parameterName} не изменён.");
            return;
        }

        normalizedValue = Mathf.Clamp01(normalizedValue);

        float decibels = normalizedValue <= 0.0001f
            ? MinDecibels
            : Mathf.Log10(normalizedValue) * 20f;

        if (!audioMixerForGame.SetFloat(parameterName, decibels))
        {
            Debug.LogWarning(
                $"AudioSettingsUI: параметр \"{parameterName}\" " +
                $"не найден среди Exposed Parameters AudioMixer.");
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