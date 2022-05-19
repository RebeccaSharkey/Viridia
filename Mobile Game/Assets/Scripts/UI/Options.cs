using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class Options : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;

    const string master = "MasterVolume";
    [SerializeField] private Toggle masterToggle;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private TextMeshProUGUI masterText;

    const string music = "MusicVolume";
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI musicText;

    const string sfx = "SFXVolume";
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxText;

    public void OnOpen()
    {
        Time.timeScale = 0;

        //Sets up master options
        float tempFloat;
        mainMixer.GetFloat(master, out tempFloat);
        masterSlider.value = tempFloat + 80f;
        if (masterSlider.value > 0f)
        {
            masterToggle.isOn = true;
        }
        else
        {
            masterToggle.isOn = false;
        }
        masterText.text = masterSlider.value.ToString();

        mainMixer.GetFloat(music, out tempFloat);
        musicSlider.value = tempFloat + 80f;
        if (musicSlider.value > 0f)
        {
            musicToggle.isOn = true;
        }
        else
        {
            musicToggle.isOn = false;
        }
        musicText.text = musicSlider.value.ToString();

        mainMixer.GetFloat(sfx, out tempFloat);
        sfxSlider.value = tempFloat + 80f;
        if (sfxSlider.value > 0f)
        {
            sfxToggle.isOn = true;
        }
        else
        {
            sfxToggle.isOn = false;
        }
        sfxText.text = sfxSlider.value.ToString();
    }
    public void OnClose()
    {
        Time.timeScale = 1;
    }

    public void MasterToggle()
    {
        if(masterToggle.isOn)
        {
            masterSlider.value = 80;
        }
        else
        {
            masterSlider.value = 0;
        }
    }
    public void OnMasterSliderChagned()
    {
        mainMixer.SetFloat(master, masterSlider.value - 80); 
        masterText.text = masterSlider.value.ToString();
    }

    public void MusicToggle()
    {
        if(musicToggle.isOn)
        {
            musicSlider.value = 65;
        }
        else
        {
            musicSlider.value = 0;
        }
    }
    public void OnMusicSliderChagned()
    {
        mainMixer.SetFloat(music, musicSlider.value - 80); 
        musicText.text = musicSlider.value.ToString();
    }

    public void SFXToggle()
    {
        if(sfxToggle.isOn)
        {
            sfxSlider.value = 80;
        }
        else
        {
            sfxSlider.value = 0;
        }
    }
    public void OnSFXSliderChagned()
    {
        mainMixer.SetFloat(sfx, sfxSlider.value - 80); 
        sfxText.text = sfxSlider.value.ToString();
    }
}
