using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject essenceUI;

    [Header("Hub Inventory")]
    [SerializeField] private GameObject hubInventoryContatiner;
    [SerializeField] private GameObject playerInventoryContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    private Dictionary<Item, int> hubInventory;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Open()
    {
        optionsButton.SetActive(false);
        essenceUI.SetActive(false);
        hubInventory = new Dictionary<Item, int>(MyEventManager.LevelManager.OnGetHubInventory?.Invoke());

        inventoryUI.SetActive(true);

        SetUI();
    }
    public void Close()
    {
        optionsButton.SetActive(true);
        essenceUI.SetActive(true);
        Dictionary<Item, int> tempInv = MyEventManager.LevelManager.OnGetHubInventory?.Invoke();
        if (tempInv != null)
        {
            foreach (KeyValuePair<Item, int> item in tempInv)
            {
                MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item.Key, item.Value);
            }
        }
        inventoryUI.SetActive(false);
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
        Dictionary<Item, int> myInv = new Dictionary<Item, int>();
        myInv = MyEventManager.Inventory.OnGetInventory?.Invoke();

        //Creates buttons for items in player inventory
        foreach (KeyValuePair<Item, int> item in myInv)
        {
            GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, playerInventoryContainer.transform);
            newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
            newPotionButton.GetComponent<ItemSlot>().treeState = TreeState.InPlayerInventory;
        }

        //Creates buttons for items in hub inventory
        foreach (KeyValuePair<Item, int> item in hubInventory)
        {
            GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, hubInventoryContatiner.transform);
            newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
            newPotionButton.GetComponent<ItemSlot>().treeState = TreeState.InHubInventory;
        }
    }


    private void AddItemToHub(Item item, int amount)
    {
        if (hubInventory.ContainsKey(item))
        {
            hubInventory[item] += amount;
        }
        else
        {
            hubInventory.Add(item, amount);
        }
        MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item, amount);
        SetUI();
    }
    private void AddItemToPlayer(Item item, int amount)
    {
        if (hubInventory.ContainsKey(item))
        {
            if ((hubInventory[item] - amount) <= 0)
            {
                hubInventory.Remove(item);
            }
            else
            {
                hubInventory[item] -= amount;
            }
            MyEventManager.Inventory.OnAddToInventory?.Invoke(item, amount);
        }
        SetUI();
    }
    public void SaveChanges()
    {
        MyEventManager.LevelManager.OnSetHubInventory?.Invoke(hubInventory);
        inventoryUI.SetActive(false);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
    }

    private void OnEnable()
    {
        MyEventManager.UI.HubUI.InventoryUI.OnOpenUI += Open;

        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToHub += AddItemToHub;
        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToPlayer += AddItemToPlayer;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.UI.HubUI.InventoryUI.OnOpenUI -= Open;

        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToHub -= AddItemToHub;
        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToPlayer -= AddItemToPlayer;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
