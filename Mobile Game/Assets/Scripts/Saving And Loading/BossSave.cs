using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BossSave
{
    //Bosses
    public List<Boss> bosses;
    public List<Monster> monsters;
    public BossSave()
    {
        bosses = new List<Boss>();
        monsters = new List<Monster>();
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
