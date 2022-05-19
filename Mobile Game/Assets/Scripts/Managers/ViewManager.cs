using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.AI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ViewManager : MonoBehaviour
{
    [SerializeField] private GameObject mapView;
    [SerializeField] private GameObject mapViewUI;
    [SerializeField] private GameObject mapViewCamera;

    [SerializeField] private GameObject fightView;
    [SerializeField] private GameObject fightViewUI;
    [SerializeField] private GameObject fightViewCamera;

    [SerializeField] private GameObject loadView;
    [SerializeField] private GameObject loadViewUI;
    [SerializeField] private GameObject loadViewCamera;

    [SerializeField] private CinemachineVirtualCamera fightCam;
    [SerializeField] private CinemachineVirtualCamera playerCam;

    private void ChangeMapViewState(bool newState)
    {
        mapView.SetActive(newState);
        if(newState)
        {
            mapViewCamera.SetActive(true);
            mapViewUI.SetActive(true);
        }
        else
        {
            mapViewCamera.SetActive(false);
            mapViewUI.SetActive(false);
        }
    }

    private void ChangeFightViewState(bool newState)
    {
        fightView.SetActive(newState);
        if(newState)
        {
            fightViewCamera.SetActive(true);
            fightViewUI.SetActive(true);
        }
        else
        {
            fightViewCamera.SetActive(false);
            fightViewUI.SetActive(false);
        }
    }

    private void ChangeLoadViewState(bool newState)
    {
        loadView.SetActive(newState);
        if (newState)
        {
            loadViewCamera.SetActive(true);
            loadViewUI.SetActive(true);
        }
        else
        {
            loadViewCamera.SetActive(false);
            loadViewUI.SetActive(false);
        }
    }

    private void ChangeMap(AssetReference newMap)
    {
        Destroy(mapView);
        StartCoroutine(CreateMap(newMap));
    }

    IEnumerator CreateMap(AssetReference newMap)
    {
        mapViewCamera.transform.position = new Vector3(0f, 0f, -10f);

        AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>(newMap);
        while(!loadOp.IsDone)
        {
            yield return null;
        }

        if (loadOp.Status == AsyncOperationStatus.Succeeded)
        {
            AsyncOperationHandle<GameObject> asycOp = Addressables.InstantiateAsync(newMap);
            while(!asycOp.IsDone)
            {
                yield return null;
            }

            if(asycOp.Status == AsyncOperationStatus.Succeeded)
            {
                mapView = asycOp.Result;

                yield return new WaitForSeconds(1f);
                PolygonCollider2D newCameraBoudns = null;
                newCameraBoudns = (PolygonCollider2D)MyEventManager.MapManager.OnGetCameraBounds?.Invoke();
                if (newCameraBoudns)
                {
                    mapViewCamera.GetComponent<CinemachineConfiner>().m_BoundingShape2D = newCameraBoudns;
                    //Code Adapted from h8man, 2020
                    AsyncOperation asycOp2 = gameObject.GetComponent<NavMeshSurface2d>().BuildNavMeshAsync();
                    //End of Adapted code
                    while (!asycOp2.isDone)
                    {
                        yield return null;
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Could not retrieve new camera bounds");
#endif
                }
                MyEventManager.LevelManager.OnNavMeshBaked?.Invoke();
                yield return null;
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("Prefab Instantiation Unsuccessful");
#endif
            }            
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Load Unsuccessful");
#endif
        }        
    }

    private void FightCameraShake()
    {
        StartCoroutine(CameraShake(fightCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>()));
    }

    private void PlayerCameraShake()
    {
        StartCoroutine(CameraShake(playerCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>()));
    }

    IEnumerator CameraShake(CinemachineBasicMultiChannelPerlin cam)
    {
        cam.m_AmplitudeGain = 5f;
        yield return new WaitForSeconds(0.25f);
        cam.m_AmplitudeGain = 0f;

    }

    private void OnEnable()
    {
        MyEventManager.ViewManager.OnSetMapViewState += ChangeMapViewState;
        MyEventManager.ViewManager.OnSetFightViewState += ChangeFightViewState;
        MyEventManager.ViewManager.OnSetLoadViewState += ChangeLoadViewState;
        MyEventManager.ViewManager.OnChangeMap += ChangeMap;
        MyEventManager.ViewManager.OnFightCamShake += FightCameraShake;
        MyEventManager.ViewManager.OnPlayerCamShake += PlayerCameraShake;
    }

    private void OnDisable()
    {
        MyEventManager.ViewManager.OnSetMapViewState -= ChangeMapViewState;
        MyEventManager.ViewManager.OnSetFightViewState -= ChangeFightViewState;
        MyEventManager.ViewManager.OnSetLoadViewState -= ChangeLoadViewState;
        MyEventManager.ViewManager.OnChangeMap -= ChangeMap;
        MyEventManager.ViewManager.OnFightCamShake -= FightCameraShake;
        MyEventManager.ViewManager.OnPlayerCamShake -= PlayerCameraShake;
    }
}
