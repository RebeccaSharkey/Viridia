using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private GameObject player;

    [Header("Start Up Variables")]
    [SerializeField] private Transform playerStartPosition;
    [SerializeField] private PolygonCollider2D mapCollider;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void SetPlayerPosition()
    {
        player.transform.position = playerStartPosition.position;
    }

    private PolygonCollider2D GetPolygonCollider2D()
    {
        return mapCollider;
    }

    private void OnEnable()
    {
        MyEventManager.MapManager.OnSetPlayerPosition += SetPlayerPosition;
        MyEventManager.MapManager.OnGetCameraBounds += GetPolygonCollider2D;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }
    private void OnDisable()
    {
        MyEventManager.MapManager.OnSetPlayerPosition -= SetPlayerPosition;
        MyEventManager.MapManager.OnGetCameraBounds -= GetPolygonCollider2D;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
