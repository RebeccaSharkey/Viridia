using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LustAbilities : MonoBehaviour
{
    private Monster monster;
    [SerializeField] private List<MonsterSO> monsters;

    void Start()
    {
        monster = GetComponent<BossManager>().monster;
        GetComponent<BossManager>().abilities.Add("Charm");
        GetComponent<BossManager>().abilities.Add("Summon");
    }

    public void PerformAbility(string ability)
    {
        switch (ability)
        {
            case "Charm":
                Charm();
                break;
            case "Summon":
                Summon();
                break;
        }
    }

    private void Charm()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        MyEventManager.LevelManager.BossLevelManager.OnMonsterCharm?.Invoke("Kiss");
    }

    private void Summon()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer?.Invoke();
        Monster summonedMonster = new Monster(monsters[Random.Range(0, monsters.Count)], monster.level);
        MyEventManager.LevelManager.BossLevelManager.OnMonsterSummon?.Invoke(summonedMonster, "Mating Call");
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
