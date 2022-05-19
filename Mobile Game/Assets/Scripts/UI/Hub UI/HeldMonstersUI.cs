using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeldMonstersUI : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private GameObject thisUI;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject essenceUI;

    [Header("Hub Inventory")]
    [SerializeField] private GameObject hubInventoryContatiner;
    [SerializeField] private GameObject playerInventoryContainer;
    [SerializeField] private GameObject monsterButtonPrefab;
    private List<Monster> hubMonsters;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Open()
    {
        optionsButton.SetActive(false);
        essenceUI.SetActive(false);
        hubMonsters = new List<Monster>(MyEventManager.LevelManager.OnGetHubMonsters?.Invoke());

        thisUI.SetActive(true);

        SetUI();
    }
    public void Close()
    {
        optionsButton.SetActive(true);
        essenceUI.SetActive(true);
        List<Monster> tempMonsters = MyEventManager.LevelManager.OnGetHubMonsters?.Invoke();
        if (tempMonsters != null)
        {
            foreach (Monster monster in tempMonsters)
            {
                MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(monster);
            }
        }
        thisUI.SetActive(false);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
    }

    private void SetUI()
    {
        //Destorys all old buttons
        foreach (Transform child in playerInventoryContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in hubInventoryContatiner.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        List<Monster> myHeldMonsters = new List<Monster>();
        myHeldMonsters = MyEventManager.Party.OnGetHeldMonsters?.Invoke();

        //Creates buttons for items in player inventory
        foreach (Monster monster in myHeldMonsters)
        {
            GameObject newPotionButton = Instantiate(monsterButtonPrefab, transform.position, Quaternion.identity, playerInventoryContainer.transform);
            newPotionButton.GetComponent<HeldMonsterButton>().SetUp(monster);
            newPotionButton.GetComponent<HeldMonsterButton>().treeState = TreeState.InPlayerInventory;
        }

        //Creates buttons for items in hub inventory
        foreach (Monster monster in hubMonsters)
        {
            GameObject newPotionButton = Instantiate(monsterButtonPrefab, transform.position, Quaternion.identity, hubInventoryContatiner.transform);
            newPotionButton.GetComponent<HeldMonsterButton>().SetUp(monster);
            newPotionButton.GetComponent<HeldMonsterButton>().treeState = TreeState.InHubInventory;
        }
    }


    private void AddItemToHub(Monster monster)
    {
        if (!hubMonsters.Contains(monster))
        {
            hubMonsters.Add(monster);
            MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(monster);
            SetUI();
        }
    }
    private void AddItemToPlayer(Monster monster)
    {
        if (hubMonsters.Contains(monster))
        {
            hubMonsters.Remove(monster);
            MyEventManager.Party.OnAddToHeldMonsters?.Invoke(monster);
            SetUI();
        }
    }
    public void SaveChanges()
    {
        MyEventManager.LevelManager.OnSetHubMonsters?.Invoke(hubMonsters);

        Close();
    }

    private void OnEnable()
    {
        MyEventManager.UI.HubUI.MonstersUI.OnOpenUI += Open;

        MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToHub += AddItemToHub;
        MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToPlayer += AddItemToPlayer;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.UI.HubUI.MonstersUI.OnOpenUI -= Open;

        MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToHub -= AddItemToHub;
        MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToPlayer -= AddItemToPlayer;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
