using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestSave
{
    public List<Monster> myTestMonsters;

    public TestSave()
    {
        myTestMonsters = new List<Monster>();
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
