using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyPlayerButton : MonoBehaviour
{
    //UI
    [SerializeField] private Image playerIcon;
    [SerializeField] private TextMeshProUGUI nameText;

    //General
    [SerializeField] private bool isFightButton;
    [SerializeField] private AudioClip audioClip;

    public void SetUp(GameObject player)
    {
        playerIcon.sprite = player.GetComponent<PlayerManager>().thisSprite.sprite;
        nameText.text = player.GetComponent<PlayerManager>().playerName;
    }

    public void Use()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if (isFightButton)
        {
            MyEventManager.LevelManager.FightPhase.OnSwitchToPlayer?.Invoke();
        }
        else
        {
            MyEventManager.UI.MapUI.OnSwitchToPlayer?.Invoke();
        }
    }
}
