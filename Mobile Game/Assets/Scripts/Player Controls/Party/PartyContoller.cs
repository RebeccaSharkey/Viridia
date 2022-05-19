using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyContoller : MonoBehaviour
{
    [SerializeField] private List<Monster> playerParty;

    [SerializeField] private List<Monster> heldMonsters;

    private void Awake()
    {
        playerParty = new List<Monster>();
        heldMonsters = new List<Monster>();

        MyEventManager.Party.OnGetParty += GetParty;
        MyEventManager.Party.OnSetParty += SetParty;
        MyEventManager.Party.OnAddToParty += AddMonsterToMonster;
        MyEventManager.Party.OnRemoveFromParty += RemoveMonsterFromParty;

        MyEventManager.Party.OnGetHeldMonsters += GetHeld;
        MyEventManager.Party.OnSetHeldMonsters += SetHeld;
        MyEventManager.Party.OnAddToHeldMonsters += AddMonsterToHolding;
        MyEventManager.Party.OnRemoveFromHeldMonsters += RemoveMonsterFromHolding;
    }

    private List<Monster> GetParty()
    {
        return playerParty;
    }

    private void SetParty(List<Monster> newMonsters)
    {
        playerParty = new List<Monster>(newMonsters);
    }

    private void AddMonsterToMonster(Monster newMonster)
    {
        if(!playerParty.Contains(newMonster))
        {
            playerParty.Add(newMonster);
        }
    }

    private void RemoveMonsterFromParty(Monster monster)
    {
        if (playerParty.Contains(monster))
        {
            playerParty.Remove(monster);
        }
    }

    private List<Monster> GetHeld()
    {
        return heldMonsters;
    }

    private void SetHeld(List<Monster> newMonsters)
    {
        heldMonsters = new List<Monster>(newMonsters);
    }

    private void AddMonsterToHolding(Monster newMonster)
    {
        if(!playerParty.Contains(newMonster))
        {
            heldMonsters.Add(newMonster);
        }
    }

    private void RemoveMonsterFromHolding(Monster monster)
    {
        if (heldMonsters.Contains(monster))
        {
            heldMonsters.Remove(monster);
        }
    }

    private void OnDestroy()
    {
        MyEventManager.Party.OnGetParty -= GetParty;
        MyEventManager.Party.OnAddToParty -= AddMonsterToMonster;
        MyEventManager.Party.OnRemoveFromParty -= RemoveMonsterFromParty;

        MyEventManager.Party.OnGetHeldMonsters -= GetHeld;
        MyEventManager.Party.OnSetHeldMonsters -= SetHeld;
        MyEventManager.Party.OnAddToHeldMonsters -= AddMonsterToHolding;
        MyEventManager.Party.OnRemoveFromHeldMonsters -= RemoveMonsterFromHolding;
    }
}
