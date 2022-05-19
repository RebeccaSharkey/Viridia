using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] private List<Transform> monsterSpawnLocations = new List<Transform>();
    [SerializeField] private List<MonsterSO> availableMonstersForScene = new List<MonsterSO>();
    [SerializeField] private GameObject monsterPrefab;
    public GameObject[] traverseSigns;

    public List<GameObject> monstersInScene = new List<GameObject>();

    public void SetUpMonsters()
    {
        /*DebugMonsters();*/
        int randomAmount = UnityEngine.Random.Range(2, 6);
        for(int i = 2; i <= randomAmount; i++)
        {
            int index = UnityEngine.Random.Range(1, monsterSpawnLocations.Count);
            CreateNewMonster(monsterSpawnLocations[index]);
        }
        MyEventManager.Monsters.OnToggleRoaming?.Invoke(true);

        //Below removed as it was making the game too long for the demo
        /*//Removes traverse signs until all monsters defeated of captured
        traverseSigns = GameObject.FindGameObjectsWithTag("Traverse Sign");
        foreach (GameObject g in traverseSigns)
        {
            g.SetActive(false);
        }*/
    }

    //Used to create a monter of every build type and level for debug reasons only
    /*public void DebugMonsters()
    {
        for(int i = 1; i <= 3; i++)
        {
            Monster newMonster = new Monster(availableMonstersForScene[1], i, MonsterAggression.Normal, MonsterBuild.Small); 
            GameObject newMonsterObject = Instantiate(monsterPrefab, monsterSpawnLocations[0].transform.position, Quaternion.identity);
            newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

            newMonster = new Monster(availableMonstersForScene[1], i, MonsterAggression.Normal, MonsterBuild.Medium); 
            newMonsterObject = Instantiate(monsterPrefab, monsterSpawnLocations[0].transform.position, Quaternion.identity);
            newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

            newMonster = new Monster(availableMonstersForScene[1], i, MonsterAggression.Normal, MonsterBuild.Large);
            newMonsterObject = Instantiate(monsterPrefab, monsterSpawnLocations[0].transform.position, Quaternion.identity);
            newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

            newMonster = new Monster(availableMonstersForScene[1], i, MonsterAggression.Normal, MonsterBuild.Apex);
            newMonsterObject = Instantiate(monsterPrefab, monsterSpawnLocations[0].transform.position, Quaternion.identity);
            newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

        }
    }*/

    public void CreateNewMonster(Transform position)
    {
        //Chooses Random Monster stats from those available 
        int randomMonster = UnityEngine.Random.Range(0, availableMonstersForScene.Count);
        MonsterBuild randomMonsterBuild = (MonsterBuild)(UnityEngine.Random.Range(0, Enum.GetValues(typeof(MonsterBuild)).Length));
        MonsterAggression randomMonsterAggression = (MonsterAggression)(UnityEngine.Random.Range(0, Enum.GetValues(typeof(MonsterAggression)).Length));
        //Creates random level of monster based on players
        int randomLevel = UnityEngine.Random.Range((int)MyEventManager.Player.OnGetPlayerLevel?.Invoke(), (int)MyEventManager.Player.OnGetPlayerLevel?.Invoke() + 3);

        //Creates Monster Class
        Monster newMonster = new Monster(availableMonstersForScene[randomMonster], randomLevel, randomMonsterAggression, randomMonsterBuild);

        //Creates New Monster GameObject and Sets it up
        GameObject newMonsterObject = Instantiate(monsterPrefab, position.transform.position, Quaternion.identity);
        newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

        //Adds to list of monsters in scene
        monstersInScene.Add(newMonsterObject);
    }

    private void CreateTutorialMonster(MonsterSO tutorialMonster)
    {
        //Gets all possible spawn locations and picks a random one
        int randomSpawnLocation = UnityEngine.Random.Range(0, monsterSpawnLocations.Count);

        //Creates Monster Class
        Monster newMonster = new Monster(tutorialMonster, 1);

        //Creates New Monster GameObject and Sets it up
        GameObject newMonsterObject = Instantiate(monsterPrefab, monsterSpawnLocations[randomSpawnLocation].transform.position, Quaternion.identity);
        newMonsterObject.GetComponent<EnemyManager>().Initialize(newMonster);

        //Adds to list of monsters in scene
        monstersInScene.Add(newMonsterObject);

        //Allows monster movement
        MyEventManager.Monsters.OnToggleRoaming?.Invoke(true);
    }

    public void RemoveMonster(GameObject thisMonster)
    {
        if(monstersInScene.Contains(thisMonster))
        {
            monstersInScene.Remove(thisMonster);
            Destroy(thisMonster);
            //Below mechanic removed for now as game demo is too long and difficult with it in (WILL BE ADDED TO FIANL BUILD)
            /*if(monstersInScene.Count <= 0)
            {
                if(traverseSigns.Length > 0)
                {
                    foreach (GameObject g in traverseSigns)
                    {
                        g.SetActive(true);
                    }
                }
            }*/
        }
    }

    private void DeleteAllMonsters()
    {
        foreach(GameObject monsters in monstersInScene)
        {
            Destroy(monsters);
        }
        monstersInScene.Clear();
    }

    private void OnEnable()
    {
        MyEventManager.Monsters.Manager.OnSetUpMonsters += SetUpMonsters;
        MyEventManager.Monsters.Manager.OnDeleteAllMonsters += DeleteAllMonsters;
        MyEventManager.Monsters.Manager.OnRemoveMonsterFromScene += RemoveMonster;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;

        MyEventManager.Monsters.Manager.OnCreateTutorialMonster += CreateTutorialMonster;
    }

    private void OnDisable()
    {
        MyEventManager.Monsters.Manager.OnSetUpMonsters -= SetUpMonsters;
        MyEventManager.Monsters.Manager.OnDeleteAllMonsters -= DeleteAllMonsters;
        MyEventManager.Monsters.Manager.OnRemoveMonsterFromScene -= RemoveMonster;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;

        MyEventManager.Monsters.Manager.OnCreateTutorialMonster -= CreateTutorialMonster;
    }
}
