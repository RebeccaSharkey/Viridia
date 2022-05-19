using System.Collections;
using UnityEngine;
using Cinemachine;

public class BossViewManager : MonoBehaviour
{
    [SerializeField] private GameObject fightView;
    [SerializeField] private GameObject fightViewUI;
    [SerializeField] private GameObject fightViewCamera;

    [SerializeField] private GameObject bossUI;

    [SerializeField] private GameObject loadView;
    [SerializeField] private GameObject loadViewUI;
    [SerializeField] private GameObject loadViewCamera;

    [SerializeField] private CinemachineVirtualCamera fightCam;

    private void ChangeMapViewState(bool newState)
    {
        fightView.SetActive(newState);
        if (newState)
        {
            fightViewCamera.SetActive(true);
        }
        else
        {
            fightViewCamera.SetActive(false);
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

    private void SetFightUI(bool newState)
    {
        fightViewUI.SetActive(newState);
    }

    private void SetBossUI(bool newState)
    {
        bossUI.SetActive(newState);
    }

    private void FightCameraShake()
    {
        StartCoroutine(CameraShake(fightCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>()));
    }

    IEnumerator CameraShake(CinemachineBasicMultiChannelPerlin cam)
    {
        cam.m_AmplitudeGain = 5f;
        yield return new WaitForSeconds(0.25f);
        cam.m_AmplitudeGain = 0f;
    }

    private void OnEnable()
    {
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState += ChangeMapViewState;
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState += ChangeLoadViewState;
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI += SetFightUI;
        MyEventManager.ViewManager.BossViewManager.OnSetBossUI += SetBossUI;
        MyEventManager.ViewManager.OnFightCamShake += FightCameraShake;
    }

    private void OnDisable()
    {
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState -= ChangeMapViewState;
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState -= ChangeLoadViewState;
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI -= SetFightUI;
        MyEventManager.ViewManager.BossViewManager.OnSetBossUI -= SetBossUI;
        MyEventManager.ViewManager.OnFightCamShake -= FightCameraShake;
    }
}
