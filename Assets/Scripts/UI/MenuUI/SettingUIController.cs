using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingUIController : MonoBehaviour
{
    [Header("Audio References")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("UI Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVol", 1f);
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVol", 1f);

        _musicSlider.value = savedMusicVol;
        _sfxSlider.value = savedSFXVol;

        _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float volume)
    {
        float volumeInDb = Mathf.Log10(volume) * 20;
        _audioMixer.SetFloat("MusicVolume", volumeInDb);
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        float volumeInDb = Mathf.Log10(volume) * 20;
        _audioMixer.SetFloat("SFXVolume", volumeInDb);
        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void BackToMenu()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneTransitionManager.Instance.LoadSceneAsync("Menu");
    }
}
