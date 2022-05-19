using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossOne : MonoBehaviour
{
    private Monster monster;

    void Start()
    {
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Ability One");
        GetComponent<BossManager>().abilities.Add("Ability Two");
    }

    public void PerformAbility(string ability)
    {
        switch(ability)
        {
            case "Ability One":
                AbilityOne();
                break;
            case "Ability Two":
                AbilityTwo();
                break;
        }
    }

    private void AbilityOne()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackSingleTarget?.Invoke(monster.currentAttackPower, DamageType.Water, "Water Kiss");
    }

    private void AbilityTwo()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll?.Invoke(monster.currentAttackPower, DamageType.Fire, "Fire Kiss");
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
