using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySort : MonoBehaviour
{
    public void Interact()
    {
        MyEventManager.UI.HubUI.InventoryUI.OnOpenUI?.Invoke();
    }

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
