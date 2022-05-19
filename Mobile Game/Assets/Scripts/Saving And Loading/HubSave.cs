using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HubSave
{
    public List<Monster> monsters;
    public List<Item> inventoryItems;
    public List<int> inventoryAmounts;
    public int playerLevel;
    public long umbraEssence;

    public HubSave()
    {
        monsters = new List<Monster>();
        inventoryItems = new List<Item>();
        inventoryAmounts = new List<int>();
        playerLevel = 1;
        umbraEssence = 0;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void LoadFromJson(string data)
    {
        JsonUtility.FromJsonOverwrite(data, this);
    }
}
