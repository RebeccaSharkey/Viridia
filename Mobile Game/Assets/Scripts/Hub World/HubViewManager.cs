using UnityEngine;

public class HubViewManager : MonoBehaviour
{
    [SerializeField] private GameObject mapView;
    [SerializeField] private GameObject mapViewUI;
    [SerializeField] private GameObject mapViewCamera;

    [SerializeField] private GameObject loadView;
    [SerializeField] private GameObject loadViewUI;
    [SerializeField] private GameObject loadViewCamera;

    private void ChangeMapViewState(bool newState)
    {
        mapView.SetActive(newState);
        if (newState)
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

    private void OnEnable()
    {
        MyEventManager.ViewManager.HubViewManager.OnSetMapViewState += ChangeMapViewState;
        MyEventManager.ViewManager.HubViewManager.OnSetLoadViewState += ChangeLoadViewState;
    }

    private void OnDisable()
    {
        MyEventManager.ViewManager.HubViewManager.OnSetMapViewState -= ChangeMapViewState;
        MyEventManager.ViewManager.HubViewManager.OnSetLoadViewState -= ChangeLoadViewState;
    }
}
