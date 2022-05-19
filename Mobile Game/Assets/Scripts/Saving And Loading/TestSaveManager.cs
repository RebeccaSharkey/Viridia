using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestSaveManager : MonoBehaviour
{
    [SerializeField] private List<Monster> myMonsters;

    public void SaveMonsters()
    {
        TestSave newTest = new TestSave();
        newTest.myTestMonsters = myMonsters;
        var myPath = Path.Combine(Application.persistentDataPath, "testMonsters.dat");
        try
        {
            File.WriteAllText(myPath, newTest.ToJson());
        }
        catch(System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't write to: {myPath} - with exception {e}");
#endif
        }
    }

    public void LoadMonsters()
    {
        string _data;
        var myPath = Path.Combine(Application.persistentDataPath, "testMonsters.dat");
        TestSave newTest = new TestSave();
        try
        {
            _data = File.ReadAllText(myPath);
            newTest.LoadFromJson(_data);
            myMonsters = newTest.myTestMonsters;
        }
        catch(System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't read from: {myPath} - with exception {e}");
#endif
            _data = "";
        }
    }
}
