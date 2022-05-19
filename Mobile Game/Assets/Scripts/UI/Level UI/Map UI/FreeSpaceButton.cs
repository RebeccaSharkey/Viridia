using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeSpaceButton : MonoBehaviour
{
    [SerializeField] private bool isMonsterButton;
    [SerializeField] private bool isAttackButton;
    [SerializeField] private bool isPowerUpButton;
    [SerializeField] private AudioClip audioClip;
    public void Use()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if (isMonsterButton)
        { 
            MyEventManager.UI.MapUI.OnAddMonster?.Invoke();
        }
        else if(isAttackButton)
        {
            MyEventManager.UI.MapUI.OnAddAttack?.Invoke();
        }
        else if(isPowerUpButton)
        {
            MyEventManager.UI.MapUI.OnAddPowerUp?.Invoke();
        }
    }
}
