using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TreeItem
{
    public ItemSO item;
    [Range(0, 1)] public float probability;
    public int maxAmount;

    public TreeItem()
    {
        item = null;
        probability = 0f;
        maxAmount = 0;
    }

    public TreeItem(TreeItem newItem)
    {
        item = newItem.item;
        probability = newItem.probability;
        maxAmount = newItem.maxAmount;
    }
}

public class TreeUI : MonoBehaviour
{
    private GameObject player;
    private Tree currentTree;

    [Header("Tree base UI")]
    [SerializeField] private GameObject treeOptionsUI;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject saveLoadMenu;

    [Header("Tree Data")]
    [SerializeField] private List<TreeItem> droppableItems;
    [SerializeField] private AudioClip twinkle;

    [Header("Save UI")]
    [SerializeField] private GameObject hubContainer;
    [SerializeField] private GameObject playerContainer;

    [Header("Save - Monster")]
    [SerializeField] private GameObject monsterButtonPrefab;
    [SerializeField] private GameObject monsterConfirmButton;
    [SerializeField] private GameObject moveAllMonsters;
    private List<Monster> monsters;

    [Header("Save - Item")]
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject itemConfirmButton;
    [SerializeField] private GameObject moveAllItems;
    private Dictionary<Item, int> hubInventory;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnOpen(Tree tree)
    {
        MyEventManager.Tutorial.OnCloseTutorialBox?.Invoke();
        MyEventManager.UI.MapUI.OnClosePlayerSettings?.Invoke();
        currentTree = tree;
        treeOptionsUI.SetActive(true);
    }
    public void OnClose()
    {
        treeOptionsUI.SetActive(false);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
        MyEventManager.Monsters.OnToggleMovement?.Invoke(true);
        MyEventManager.Tutorial.OnTreeViewed?.Invoke();
    }
    public void OnBackToMainMenu()
    {
        //Destorys all old buttons
        foreach (Transform child in playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in hubContainer.transform)
        {
            Destroy(child.gameObject);
        }

        if(hubInventory != null)
        {
            foreach (KeyValuePair<Item, int> item in hubInventory)
            {
                MyEventManager.Inventory.OnAddToInventory?.Invoke(item.Key, item.Value);
            }
            hubInventory.Clear();
        }
        if(monsters != null)
        {
            foreach (Monster monster in monsters)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(monster);
            }
            monsters.Clear();
        }
        moveAllItems.SetActive(false);
        moveAllMonsters.SetActive(false);
    }

    public void OnShakeTree()
    {
        if (!currentTree.beenShaken)
        {
            MyEventManager.ViewManager.OnPlayerCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(twinkle);
            currentTree.beenShaken = true;
            int droppedItemsAmount = Random.Range(5, 11);
            List<TreeItem> tempList = new List<TreeItem>(droppableItems);
            while (droppedItemsAmount >= 0)
            {
                float cummlativeProbability = 0f;
                foreach (TreeItem item in tempList)
                {
                    cummlativeProbability += item.probability;
                }

                float randomItem = Random.value * cummlativeProbability;

                var incrimentProbabilty = 0f;
                TreeItem treeItem = null;
                foreach (TreeItem item in tempList)
                {
                    if (incrimentProbabilty <= randomItem && randomItem < incrimentProbabilty + item.probability)
                    {
                        Item tempItem = new Item(item.item);
                        int randomAmount = Random.Range(1, item.maxAmount + 1);
                        MyEventManager.Inventory.OnAddToInventory?.Invoke(tempItem, randomAmount);
                        droppedItemsAmount--;
                        treeItem = item;
                        break;
                    }
                    incrimentProbabilty += item.probability;
                }
                //Makes sure the player can't get the same item again
                tempList.Remove(treeItem);
            }
        }
    }

    public void OnSaveItemsToHub()
    {
        mainMenu.SetActive(false);
        saveLoadMenu.SetActive(true);
        itemConfirmButton.SetActive(true);
        monsterConfirmButton.SetActive(false);

        hubInventory = new Dictionary<Item, int>();

        SetUpItemUI();
    }
    private void SetUpItemUI()
    {
        //Destorys all old buttons
        foreach (Transform child in playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in hubContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        Dictionary<Item, int> myInv = new Dictionary<Item, int>();
        myInv = MyEventManager.Inventory.OnGetInventory?.Invoke();

        //Creates buttons for items in player inventory
        foreach (KeyValuePair<Item, int> item in myInv)
        {
            GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, playerContainer.transform);
            newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
            newPotionButton.GetComponent<ItemSlot>().treeState = TreeState.InPlayerInventory;
        }

        //Creates buttons for items in hub inventory
        foreach (KeyValuePair<Item, int> item in hubInventory)
        {
            GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, hubContainer.transform);
            newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
            newPotionButton.GetComponent<ItemSlot>().treeState = TreeState.InHubInventory;
        }

        moveAllItems.SetActive(true);

    }
    private void AddItemToHub(Item item, int amount)
    {
        if(hubInventory.ContainsKey(item))
        {
            hubInventory[item] += amount;
        }
        else
        {
            hubInventory.Add(item, amount);
        }
        MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item, amount);
        SetUpItemUI();
    }
    private void AddItemToPlayer(Item item, int amount)
    {
        if (hubInventory.ContainsKey(item))
        {
            if((hubInventory[item] - amount) == 0)
            {
                hubInventory.Remove(item);
                MyEventManager.Inventory.OnAddToInventory?.Invoke(item, amount);
            }
            else if((hubInventory[item] - amount) > 0)
            {
                hubInventory[item] -= amount;
                MyEventManager.Inventory.OnAddToInventory?.Invoke(item, amount);
            }
        }
        SetUpItemUI();
    }
    public void OnMoveAllItems()
    {
        foreach(KeyValuePair<Item, int> item in MyEventManager.Inventory.OnGetInventory?.Invoke())
        {
            if (hubInventory.ContainsKey(item.Key))
            {
                hubInventory[item.Key] += item.Value;
            }
            else
            {
                hubInventory.Add(item.Key, item.Value);
            }
        }
        foreach(KeyValuePair<Item, int> item in hubInventory)
        {
            MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item.Key, item.Value);
        }
        SetUpItemUI();
    }
    public void OnSaveItems()
    {
        MyEventManager.SaveLoad.OnSaveItemsToHub?.Invoke(hubInventory);
        hubInventory.Clear();
        mainMenu.SetActive(true);
        saveLoadMenu.SetActive(false);
        moveAllItems.SetActive(false);
        moveAllMonsters.SetActive(false);
    }

    public void OnSaveMonstersToHub()
    {
        mainMenu.SetActive(false);
        saveLoadMenu.SetActive(true);
        itemConfirmButton.SetActive(false);
        monsterConfirmButton.SetActive(true);

        monsters = new List<Monster>();

        SetUpMonsterUI();
    }
    private void SetUpMonsterUI()
    {
        //Destorys all old buttons
        foreach (Transform child in playerContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in hubContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players held monsters
        List<Monster> myParty = new List<Monster>();
        myParty = MyEventManager.Party.OnGetHeldMonsters?.Invoke();

        //Creates buttons for items in player inventory
        foreach (Monster monster in myParty)
        {
            GameObject newMonsterButton = Instantiate(monsterButtonPrefab, transform.position, Quaternion.identity, playerContainer.transform);
            newMonsterButton.GetComponent<HeldMonsterButton>().SetUp(monster);
            newMonsterButton.GetComponent<HeldMonsterButton>().treeState = TreeState.InPlayerInventory;
        }

        //Creates buttons for items in hub inventory
        foreach (Monster monster in monsters)
        {
            GameObject newMonsterButton = Instantiate(monsterButtonPrefab, transform.position, Quaternion.identity, hubContainer.transform);
            newMonsterButton.GetComponent<HeldMonsterButton>().SetUp(monster);
            newMonsterButton.GetComponent<HeldMonsterButton>().treeState = TreeState.InHubInventory;
        }

        moveAllMonsters.SetActive(true);
    }
    private void AddMonsterToHub(Monster monster)
    {
        if (!monsters.Contains(monster))
        {
            monsters.Add(monster);
            MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(monster);
        }
        SetUpMonsterUI();
    }
    private void AddMonsterToPlayer(Monster monster)
    {
        if (monsters.Contains(monster))
        {
            monsters.Remove(monster);
            MyEventManager.Party.OnAddToHeldMonsters?.Invoke(monster);
        }
        SetUpMonsterUI();
    }
    public void OnMoveAllMonsters()
    {
        foreach (Monster monster in MyEventManager.Party.OnGetHeldMonsters?.Invoke())
        {
            if (!monsters.Contains(monster))
            {
                monsters.Add(monster);
            }
        }
        foreach(Monster monster1 in monsters)
        {
            MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(monster1);
        }
        SetUpMonsterUI();
    }
    public void OnSaveMonsters()
    {
        MyEventManager.SaveLoad.OnSaveMonstersToHub?.Invoke(monsters);
        monsters.Clear();
        mainMenu.SetActive(true);
        saveLoadMenu.SetActive(false);
        moveAllItems.SetActive(false);
        moveAllMonsters.SetActive(false);
    }

    private void OnEnable()
    {
        MyEventManager.UI.TreeUI.OnOpenTree += OnOpen;
        MyEventManager.UI.TreeUI.OnCloseTree += OnClose;

        MyEventManager.UI.TreeUI.OnAddItemToHub += AddItemToHub;
        MyEventManager.UI.TreeUI.OnAddItemToPlayer += AddItemToPlayer;

        MyEventManager.UI.TreeUI.OnAddMonsterToHub += AddMonsterToHub;
        MyEventManager.UI.TreeUI.OnAddMonsterToPlayer += AddMonsterToPlayer;
    }

    private void OnDisable()
    {
        MyEventManager.UI.TreeUI.OnOpenTree -= OnOpen;
        MyEventManager.UI.TreeUI.OnCloseTree -= OnClose;

        MyEventManager.UI.TreeUI.OnAddItemToHub -= AddItemToHub;
        MyEventManager.UI.TreeUI.OnAddItemToPlayer -= AddItemToPlayer;

        MyEventManager.UI.TreeUI.OnAddMonsterToHub -= AddMonsterToHub;
        MyEventManager.UI.TreeUI.OnAddMonsterToPlayer -= AddMonsterToPlayer;
    }
}
