using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
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

    [Header("Game Juice")]
    public AudioClip TimerTickSFX;
    public AudioClip CardHoverSFX;    

    [Header("Music")]
    public AudioClip MenuBgMusic;
    public AudioClip BattleBgMusic;

    [Header("UI Interact")]
    public AudioClip ButtonClick;
    public AudioClip Swoosh;

    public void PlaySFX(AudioClip sfx, bool randomPitch = false, bool isOverrided = false, float volume = 1f)
    {
        SFX.volume = volume;
        if(randomPitch)
        {
            SFX.pitch = Random.Range(0.9f, 1.1f);
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
