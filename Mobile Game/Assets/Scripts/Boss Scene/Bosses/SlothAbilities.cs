using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlothAbilities : MonoBehaviour
{
    private Monster monster;
    [SerializeField] private int sleepCountDown;
    [SerializeField] private Sprite awokeMonster;

    void Start()
    {
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Sleep");
    }

    public void PerformAbility(string ability)
    {
        switch (ability)
        {
            case "Sleep":
                Sleep();
                break;
            case "Awaken":
                GroupAttack();
                break;
        }
    }

    private void Sleep()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"Zzz Zzz Zzz\n\nTurn: {sleepCountDown}/20", "Red");
        sleepCountDown--;
        if (sleepCountDown == 0)
        {
            GetComponent<BossManager>().abilities.Remove("Sleep");
            GetComponent<BossManager>().abilities.Add("Awaken");
        }
    }

    private void GroupAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        //GetComponent<Image>().sprite = awokeMonster;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, DamageType.Umbra, "Awaken");
    }


    private void OnEnable()
    {
        MyEventManager.BossManager.OnExecuteAbility += PerformAbility;
    }

    private void OnDisable()
    {
        MyEventManager.BossManager.OnExecuteAbility -= PerformAbility;
    }
}
