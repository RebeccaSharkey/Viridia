using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreedAbilities : MonoBehaviour
{
    private Monster monster;

    void Start()
    {
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Essence Steal");
        GetComponent<BossManager>().abilities.Add("Group Attack");
        GetComponent<BossManager>().abilities.Add("Solo Attack");
    }

    public void PerformAbility(string ability)
    {
        switch (ability)
        {
            case "Essence Steal":
                EssenceSteal();
                break;
            case "Group Attack":
                GroupAttack();
                break;
            case "Solo Attack":
                SoloAttack();
                break;
        }
    }

    private void EssenceSteal()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnStealUmbraEssence?.Invoke(monster.currentAttackPower, "Pick Pocket");
    }

    private void SoloAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnUmbraEssenceAttack?.Invoke("Loan Shark");
    }

    private void GroupAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, DamageType.Umbra, "Collect Debts");
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
