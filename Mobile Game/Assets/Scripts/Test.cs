using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private List<ItemSO> itemsToAddAtTheStart;

    public void OnPress()
    {
        foreach (ItemSO item in itemsToAddAtTheStart)
        {
            Item temp = new Item(item);
            MyEventManager.Inventory.OnAddToInventory?.Invoke(temp, 1);
        }
    }
}
