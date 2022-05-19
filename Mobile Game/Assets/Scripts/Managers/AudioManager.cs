using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource themeSource;

    public void PlaySound(AudioClip audio)
    {
        sfxAudioSource.clip = audio;
        sfxAudioSource.Play();
    }

    public void PlayThemeMusic(AudioClip audio)
    {
        themeSource.clip = audio;
        themeSource.Play();
    }

    private void OnEnable()
    {
        MyEventManager.AudioManager.OnPlaySFX += PlaySound;
        MyEventManager.AudioManager.OnPlayThemeMusic += PlayThemeMusic;
    }

    private void OnDisable()
    {
        MyEventManager.AudioManager.OnPlaySFX -= PlaySound;
        MyEventManager.AudioManager.OnPlayThemeMusic -= PlayThemeMusic;
    }

}
