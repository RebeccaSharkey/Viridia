using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSave
{
    public string playerName;
    public string spritePath;

    public List<Monster> monsters;
    public List<Item> inventoryItems;
    public List<int> inventoryAmounts;
    public int playerLevel;
    public long umbraEssence;
    public bool isTutorialComplete;
    public List<Attack> attacks;

    public PlayerSave()
    {
        monsters = new List<Monster>();
        inventoryItems = new List<Item>();
        inventoryAmounts = new List<int>();
        playerLevel = 1;
        umbraEssence = 0;
        isTutorialComplete = false;
        playerName = "Player";
        spritePath = "";
        attacks = new List<Attack>();
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
