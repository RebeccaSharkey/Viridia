using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMonsterButton : MonoBehaviour
{
    //Base Values
    [HideInInspector] public Monster monster;

    //UI
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI nameText;

    //General
    [SerializeField] private bool isFightButton;
    [SerializeField] private AudioClip audioClip;

    public void SetUpSlot(Monster newMonster)
    {
        monster = newMonster;
        monsterIcon.sprite = monster.sprite;
        nameText.text = monster.shortName;
    }

    public void Use()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if (isFightButton)
        { 
            MyEventManager.LevelManager.FightPhase.OnSwitchToMonster?.Invoke(monster);
        }
        else
        {
            MyEventManager.UI.MapUI.OnSwitchToMonster?.Invoke(monster);
        }
    }
}
