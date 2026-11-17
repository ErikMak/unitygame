using UnityEngine;
using UnityEngine.Audio;

public sealed class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixerForGame;

    public float MasterVolume => PlayerPrefs.GetFloat("Audio.Master", 1f);
    public float MusicVolume => PlayerPrefs.GetFloat("Audio.Music", 1f);
    public float SoundVolume => PlayerPrefs.GetFloat("Audio.SFX", 1f);

    private void Start()
    {
        Apply("MasterVolume", MasterVolume);
        Apply("MusicVolume", MusicVolume);
        Apply("SFXVolume", SoundVolume);
        Apply("VoicesVolume", PlayerPrefs.GetFloat("Audio.Voices", 1f));
    }

    private void OnDisable() => PlayerPrefs.Save();

    public void SetMasterVolume(float value) => SetVolume("Audio.Master", "MasterVolume", value);
    public void SetMusicVolume(float value) => SetVolume("Audio.Music", "MusicVolume", value);
    public void SetSfxVolume(float value) => SetVolume("Audio.SFX", "SFXVolume", value);
    public void SetVoicesVolume(float value) => SetVolume("Audio.Voices", "VoicesVolume", value);

    public void ResetAudioSettings()
    {
        SetMasterVolume(1);
        SetMusicVolume(1);
        SetSfxVolume(1);
        SetVoicesVolume(1);
        PlayerPrefs.Save();
    }

    private void SetVolume(string key, string parameter, float value)
    {
        value = Mathf.Clamp01(value);
        Apply(parameter, value);
        PlayerPrefs.SetFloat(key, value);
    }

    private void Apply(string parameter, float value)
    {
        if (audioMixerForGame != null)
            audioMixerForGame.SetFloat(parameter, value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f);
    }
}
