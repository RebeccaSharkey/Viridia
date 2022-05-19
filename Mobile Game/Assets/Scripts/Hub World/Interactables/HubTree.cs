using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubTree : MonoBehaviour
{
    public void Interact()
    {
        MyEventManager.LevelManager.OnGoToPlayScene?.Invoke();
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
