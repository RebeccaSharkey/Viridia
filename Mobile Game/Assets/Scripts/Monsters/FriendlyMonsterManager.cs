using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyMonsterManager : MonoBehaviour
{
    public Monster monster;

    public void Initialize(Monster newMonster)
    {
        monster = newMonster;
        //Sets up name and sprite
        gameObject.name = newMonster.fullName;
        gameObject.name = gameObject.name.Replace("(MonsterSO)", "");
        gameObject.GetComponent<SpriteRenderer>().sprite = monster.sprite;
    }

    public void TakeDamage(int damage, bool isCurrentFighter = true)
    {
        monster.TakeDamage(damage);
        if (isCurrentFighter)
        {
            if(monster.currentHealth <= 0)
            {
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(gameObject.name + " died.", "Red");
                MyEventManager.LevelManager.FightPhase.OnSwitchToPlayer?.Invoke();
            }
            else
            {
                MyEventManager.UI.FightUI.OnUpdateFighterHealthUI?.Invoke(this);
            }
        }
    }

    public void UseAP(int decriment, bool isCurrentFighter = true)
    {
        monster.UseAP(decriment);
        if (isCurrentFighter)
        {
            MyEventManager.UI.FightUI.OnUpdateFighterActionPointsUI?.Invoke(this);
        }
    }
    public bool CheckAP(int attackUsage)
    {
        if ((monster.currentActionPoints - attackUsage) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public void UseMP(int decriment, bool isCurrentFighter = true)
    {
        monster.UseMP(decriment);
        if (isCurrentFighter)
        {
            MyEventManager.UI.FightUI.OnUpdateFighterMagicPointsUI?.Invoke(this);
        }
    }
    public bool CheckMP(int spellUsage)
    {
        if ((monster.currentMagicPoints - spellUsage) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Heal(int buff, bool isCurrentFighter = true)
    {
        monster.Heal(buff);
        if (isCurrentFighter)
        {
            MyEventManager.UI.FightUI.OnUpdateFighterHealthUI?.Invoke(this);
        }
    }
    public void RestoreAP(int buff, bool isCurrentFighter = true)
    {
        monster.RestoreAP(buff);
        if (isCurrentFighter)
        {
            MyEventManager.UI.FightUI.OnUpdateFighterActionPointsUI?.Invoke(this);
        }
    }
    public void RestoreMP(int buff, bool isCurrentFighter = true)
    {
        monster.RestoreMP(buff);
        if (isCurrentFighter)
        {
            MyEventManager.UI.FightUI.OnUpdateFighterMagicPointsUI?.Invoke(this);
        }
    }

    public void LevelUp()
    {
        monster.LevelUp();
    }

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }

}
