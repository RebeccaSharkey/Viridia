using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    //Base Values
    [HideInInspector] public Item item;
    [HideInInspector] public int cost;

    //UI
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private AudioClip audioClip;

    public void SetUpSlot(Item newItem)
    {
        item = newItem;
        cost = newItem.cost;

        itemIcon.sprite = item.itemIcon;
        costText.text = $"Price: {cost}";
        nameText.text = item.itemName;
    }

    public void OnUse()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        switch (item.itemType)
        {
            case ItemType.TrainingScripts:
                MyEventManager.UI.HubUI.TrainingScriptShopUI.OnBuyItem?.Invoke(item);
                break;
        }
    }
}
