using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvyAbilities : MonoBehaviour
{
    private Monster monster;
    public DamageType currentAspect;

    void Start()
    {
        currentAspect = DamageType.Umbra;
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Group Attack");
        GetComponent<BossManager>().abilities.Add("Group Attack1");
        GetComponent<BossManager>().abilities.Add("Solo Attack");
        GetComponent<BossManager>().abilities.Add("Solo Attack1");
    }

    public void PerformAbility(string ability)
    {
        switch (ability)
        {
            case "Group Attack":
                GroupAttack();
                break;
            case "Group Attack1":
                GroupAttack1();
                break;
            case "Solo Attack":
                SoloAttack();
                break;
            case "Solo Attack1":
                SoloAttack1();
                break;
        }
    }

    private void SoloAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, currentAspect, $"{currentAspect} Slap");
    }

    private void SoloAttack1()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, currentAspect, $"{currentAspect} Kick");
    }

    private void GroupAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, DamageType.Umbra, "Umbra Hurricane");
    }

    private void GroupAttack1()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, currentAspect, $"{currentAspect} Storm");
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
