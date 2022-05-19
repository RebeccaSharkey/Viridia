using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrainingScriptShopUI : MonoBehaviour
{
    [SerializeField] private List<Item> shopItems;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject itemButton;
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private TextMeshProUGUI umbraEssenceText;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject essenceUI;
    private GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        shopItems = new List<Item>();
        ItemSO[] items = Resources.LoadAll<ItemSO>("Training Scripts");
        foreach (ItemSO item in items)
        {
            Item tempItem = new Item(item);
            shopItems.Add(tempItem);
        }

        foreach (Item item in shopItems)
        {
            GameObject newItemButton = Instantiate(itemButton, transform.position, Quaternion.identity, itemContainer.transform);
            newItemButton.GetComponent<ShopItemButton>().SetUpSlot(item);
        }
    }

    private void Open()
    {
        optionsButton.SetActive(false);
        essenceUI.SetActive(false);
        menuUI.SetActive(true);
        umbraEssenceText.text = $"Umbra Essence: {MyEventManager.Inventory.OnGetUmbraEssence?.Invoke()}";
    }

    private void BuyItem(Item item)
    {
        if(MyEventManager.Inventory.OnGetUmbraEssence?.Invoke() >= item.cost)
        {
            MyEventManager.Inventory.OnAddToInventory?.Invoke(item, 1);
            MyEventManager.Inventory.OnRemoveUmbraEssence?.Invoke(item.cost);
            umbraEssenceText.text = $"Umbra Essence: {MyEventManager.Inventory.OnGetUmbraEssence?.Invoke()}";
        }
    }

    public void Close()
    {
        optionsButton.SetActive(true);
        essenceUI.SetActive(true);
        menuUI.SetActive(false);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
    }

    private void OnEnable()
    {
        MyEventManager.UI.HubUI.TrainingScriptShopUI.OnOpenUI += Open;
        MyEventManager.UI.HubUI.TrainingScriptShopUI.OnBuyItem += BuyItem;
    }

    private void OnDisable()
    {
        MyEventManager.UI.HubUI.TrainingScriptShopUI.OnOpenUI -= Open;
        MyEventManager.UI.HubUI.TrainingScriptShopUI.OnBuyItem -= BuyItem;
    }
}
