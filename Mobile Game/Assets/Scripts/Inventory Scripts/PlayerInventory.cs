using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private Dictionary<Item, int> playerInventory;
    [SerializeField] private long umbraEssence;
    private void Awake()
    {
        playerInventory = new Dictionary<Item, int>();
        umbraEssence = 0;
        MyEventManager.Inventory.OnGetInventory += GetInventory;
        MyEventManager.Inventory.OnSetInventory += SetInventory;
        MyEventManager.Inventory.OnAddToInventory += AddItemToInventory;
        MyEventManager.Inventory.OnRemoveFromInventory += RemoveFromInventory;

        MyEventManager.Inventory.OnGetUmbraEssence += GetUmbraEssence;
        MyEventManager.Inventory.OnSetUmbraEssence += SetUmbraEssence;
        MyEventManager.Inventory.OnAddUmbraEssence += AddUmbraEssence;
        MyEventManager.Inventory.OnRemoveUmbraEssence += RemoveUmbraEssence;
    }

    private Dictionary<Item, int> GetInventory()
    {
        return playerInventory;
    }

    private void SetInventory(Dictionary<Item, int> newInventory)
    {
        playerInventory = newInventory;
    }
    private void AddItemToInventory(Item newItem, int amount)
    {
        KeyValuePair<Item, int> tempItem = new KeyValuePair<Item, int>(null, 0);
        foreach (KeyValuePair<Item, int> item in playerInventory)
        {
            if(item.Key.itemName == newItem.itemName)
            {
                tempItem = item;
            }
        }

        if(tempItem.Key == null)
        {
            playerInventory.Add(newItem, amount);
        }
        else
        {
            playerInventory[tempItem.Key] += amount;
        }
    }
    private void RemoveFromInventory(Item newItem, int amount)
    { 
        if (playerInventory.ContainsKey(newItem))
        {
            if((playerInventory[newItem] - amount) == 0)
            {
                playerInventory.Remove(newItem);
            }
            else if((playerInventory[newItem] - amount) > 0)
            {
                playerInventory[newItem] -= amount;
            }
        }
    }

    private void SetUmbraEssence(long newAmount)
    {
        umbraEssence = newAmount;
        MyEventManager.Inventory.OnUpdateEssenceUI?.Invoke(umbraEssence);
    }
    private long GetUmbraEssence()
    {
        return umbraEssence;
    }
    private void AddUmbraEssence(long amount)
    {
        umbraEssence += amount;

        //Checks that the player hasn't got too much essence that they accidentally flip the long into minus
        if(umbraEssence < 0)
        {
            umbraEssence = 9223372036854775807;
        }
        MyEventManager.Inventory.OnUpdateEssenceUI?.Invoke(umbraEssence);
    }
    private void RemoveUmbraEssence(long amount)
    {
        umbraEssence -= amount;

        //Checks if the player has gone below 0 some how (Shouldn't as other code should not allow this.. // Scratch that other code does allow for this as gula (Boss) steals money)
        if(umbraEssence < 0)
        {
            umbraEssence = 0;
        }
        MyEventManager.Inventory.OnUpdateEssenceUI?.Invoke(umbraEssence);
    }
    private void OnDestroy()
    {
        MyEventManager.Inventory.OnGetInventory -= GetInventory;
        MyEventManager.Inventory.OnSetInventory -= SetInventory;
        MyEventManager.Inventory.OnAddToInventory -= AddItemToInventory;
        MyEventManager.Inventory.OnRemoveFromInventory -= RemoveFromInventory;

        MyEventManager.Inventory.OnGetUmbraEssence -= GetUmbraEssence;
        MyEventManager.Inventory.OnSetUmbraEssence -= SetUmbraEssence;
        MyEventManager.Inventory.OnAddUmbraEssence -= AddUmbraEssence;
        MyEventManager.Inventory.OnRemoveUmbraEssence -= RemoveUmbraEssence;
    }
}
