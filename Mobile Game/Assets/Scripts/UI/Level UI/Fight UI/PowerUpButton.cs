using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//No longer in use as power ups have been scrapped from the game (For now I think...)
public class PowerUpButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    private PowerUp powerUp;
    [SerializeField] private AudioClip audioClip;

    public void SetUp(PowerUp newPowerUp)
    {
        powerUp = newPowerUp;
        buttonText.text = powerUp.abilityName;
    }

    public void OnPress()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
#if UNITY_EDITOR
        Debug.Log("Player Performed: " + powerUp.abilityName);
#endif
    }
}
