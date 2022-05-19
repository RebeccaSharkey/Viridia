using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tree : MonoBehaviour
{
    public bool beenShaken = false;

    public void Interact()
    {
        MyEventManager.UI.TreeUI.OnOpenTree?.Invoke(this);
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
