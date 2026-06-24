using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{   
    [Header("Audio Sources")]
    [SerializeField] private AudioMixer _audioMixer;
    public AudioSource SFX;
    public AudioSource Music;

    [Header("SFX - Card")]
    public AudioClip[] DealSFX; 
    public AudioClip FlipSFX;

    [Header("SFX - Stamp")]
    public AudioClip StampUsed;

    [Header("End Game")]
    public AudioClip VictorySFX;
    public AudioClip DefeatSFX;

    [Header("Gameplay SFX")]
    public AudioClip minusHPSFX;
    public AudioClip HealSFX;
    public AudioClip CountScoreSFX;

    [Header("Game Juice")]
    public AudioClip TimerTickSFX;
    public AudioClip CardHoverSFX;    

    [Header("Music")]
    public AudioClip MenuBgMusic;
    public AudioClip BattleBgMusic;

    [Header("UI Interact")]
    public AudioClip ButtonClick;
    public AudioClip SceenTransition;
    public AudioClip Hover;
    public AudioClip InputFieldClick;

    public static AudioManager Instance;
    private void Awake()
    {
        if(Instance != null  && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlaySFX(AudioClip sfx, bool randomPitch = false, bool isOverrided = false, float volume = 1f)
    {
        SFX.volume = volume;
        if(randomPitch)
        {
            SFX.pitch = Random.Range(0.8f, 1.2f);
        }
        else
        {
            SFX.pitch = 1f;
        }
        if(isOverrided)
        {
            SFX.Stop();
            // play sfx
            SFX.clip = sfx;
            SFX.Play();
        }
        else
        {
            SFX.PlayOneShot(sfx);
        }

        SFX.volume = 1f;
    }

    public void PlayMusic(AudioClip music)
    {
        if(Music.clip == music) return;
        Music.clip = music;
        Music.Play();
    }
}
