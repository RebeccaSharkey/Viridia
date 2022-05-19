using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GluttonyAbilities : MonoBehaviour
{
    private Monster monster;

    void Start()
    {
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Potion Steal");
        GetComponent<BossManager>().abilities.Add("Boost Negate");
        GetComponent<BossManager>().abilities.Add("Group Attack");
        GetComponent<BossManager>().abilities.Add("Solo Attack");
    }

    public void PerformAbility(string ability)
    {
        switch (ability)
        {
            case "Potion Steal":
                PotionSteal();
                break;
            case "Boost Negate":
                BoostNegate();
                break;
            case "Group Attack":
                GroupAttack();
                break;
            case "Solo Attack":
                SoloAttack();
                break;
        }
    }

    private void PotionSteal()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnPotionSteal?.Invoke("Beg for Treats");
    }

    private void BoostNegate()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterNegateAllBoosts?.Invoke("Dumpster Dive");
    }

    private void GroupAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, DamageType.Air, "Expel Gas");
    }

    private void SoloAttack()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackSingleTarget?.Invoke(monster.currentAttackPower, DamageType.Air, "Belch");
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
