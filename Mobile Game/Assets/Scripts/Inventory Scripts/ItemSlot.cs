using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TreeState { InPlayerInventory, InHubInventory, Default}

public class ItemSlot : MonoBehaviour
{
    //Base Values
    [HideInInspector] public Item item;
    [HideInInspector] public int amount;

    //UI
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private TextMeshProUGUI nameText;

    //Fight Slot UI only
    [SerializeField] private bool isFightButton;

    //Tree Slot UI Only
    [SerializeField] private bool isTreeUI;
    [HideInInspector] public TreeState treeState = TreeState.Default;

    private bool isPressed;
    [SerializeField] private AudioClip audioClip;

    public void SetUpSlot(Item newItem, int newAmount)
    {
        item = newItem;
        amount = newAmount;

        itemIcon.sprite = item.itemIcon;
        amountText.text = amount.ToString();
        nameText.text = item.itemName;

        isPressed = false;
    }

    public void Use()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if (isFightButton)
        {
            switch (item.itemType)
            {
                case ItemType.Locket:
                    MyEventManager.LevelManager.FightPhase.OnPlayerUsedCapture?.Invoke(item);
                    break;
                case ItemType.Potion:
                    MyEventManager.LevelManager.FightPhase.OnPlayerUsedPotion?.Invoke(item);
                    break;
                case ItemType.Default:
                    break;
            }
            MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item, 1);
        }
        else if(isTreeUI)
        {
            switch (treeState)
            {
                case TreeState.InPlayerInventory:
                    if (!isPressed)
                    {
                        isPressed = true;
                        MyEventManager.UI.TreeUI.OnAddItemToHub?.Invoke(item, amount);
                        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToHub?.Invoke(item, amount);
                    }
                    return;
                case TreeState.InHubInventory:
                    if(!isPressed)
                    {
                        isPressed = true;
                        MyEventManager.UI.TreeUI.OnAddItemToPlayer?.Invoke(item, amount);
                        MyEventManager.UI.HubUI.InventoryUI.OnAddItemToPlayer?.Invoke(item, amount);
                    }
                    return;
                case TreeState.Default:
#if UNITY_EDITOR

                    Debug.LogWarning("State not Set");
#endif
                    return;
            }
        }
        else
        {
            switch (item.itemType)
            {
                case ItemType.Potion:
                    switch (item.potionType)
                    {
                        case PotionType.Health:
                            MyEventManager.UI.MapUI.OnUpdateHealth?.Invoke(item.potionIncriment);
                            break;
                        case PotionType.Stamina:
                            MyEventManager.UI.MapUI.OnUpdateAP?.Invoke(item.potionIncriment);
                            break;
                        case PotionType.Magic:
                            MyEventManager.UI.MapUI.OnUpdateMP?.Invoke(item.potionIncriment);
                            break;
                    }

                    MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(item, 1);
                    amount--;
                    if (amount <= 0)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        amountText.text = amount.ToString();
                    }
                    break;
                case ItemType.MonsterRemains:
                    MyEventManager.UI.MapUI.OnLevelUp?.Invoke(item);
                    break;
                case ItemType.Default:
                    break;
            }
        }
    }
}
