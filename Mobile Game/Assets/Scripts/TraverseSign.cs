using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;

public class TraverseSign : MonoBehaviour
{
    [SerializeField] private List<AssetReference> availableNewMaps;

    public void Interact()
    {
        int randomMap = Random.Range(0, availableNewMaps.Count);
        MyEventManager.LevelManager.OnTraverseNewMap?.Invoke(availableNewMaps[randomMap]);
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
