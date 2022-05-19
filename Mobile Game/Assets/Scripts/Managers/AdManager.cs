using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsListener
{
    private string gameID = "4716891";
    public bool testMode = true;
    private bool bAdLoaded;
    Action OnSucces;

    public static AdManager sceneManager;

    public void Awake()
    {
        if (sceneManager != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            sceneManager = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        Advertisement.Initialize(gameID, testMode, true, this);
        Advertisement.AddListener(this);
    }

    public void OnInitializationComplete()
    {
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
#if UNITY_EDITOR
        Debug.Log($"Ads Error: {error}: {message}");
#endif
    }

    private void LoadAd()
    {
        Advertisement.Load("Rewarded_Android", this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {        
        bAdLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
#if UNITY_EDITOR
        Debug.Log($"Error loading ad {placementId}: {error.ToString()}: {message}");
#endif
        bAdLoaded = false;
    }

    private void ShowAd(Action action)
    {
        if (bAdLoaded)
        {
            Advertisement.Show("Rewarded_Android");
            OnSucces = action;
        }
    }

    private bool GetAdLoaded()
    {
        return bAdLoaded;
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId == "Rewarded_Android" && showResult == ShowResult.Finished)
        {
            OnSucces?.Invoke();
            LoadAd();
        }
    }

    private void OnEnable()
    {
        MyEventManager.AdManager.OnGetIsAdLoaded += GetAdLoaded;
        MyEventManager.AdManager.OnShowRewardedAd += ShowAd;
    }

    private void OnDisable()
    {
        MyEventManager.AdManager.OnGetIsAdLoaded -= GetAdLoaded;
        MyEventManager.AdManager.OnShowRewardedAd -= ShowAd;
    }

    //IUnityAdsListener needs these to run but I don't use them within my code...
    public void OnUnityAdsReady(string placementId)
    {
        
    }
    public void OnUnityAdsDidError(string message)
    {
        
    }
    public void OnUnityAdsDidStart(string placementId)
    {

    }
}
