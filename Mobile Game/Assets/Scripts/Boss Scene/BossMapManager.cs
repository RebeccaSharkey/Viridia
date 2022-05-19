using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMapManager : MonoBehaviour
{
    private GameObject player;

    [Header("Start Up Variables")]
    [SerializeField] private Transform playerStartPosition;
    [SerializeField] private List<Transform> friendlyMonsterStartPosition;
    [SerializeField] private Transform bossStartPosition;
    [SerializeField] private PolygonCollider2D mapCollider;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void SetPlayerPosition()
    {
        player.transform.position = playerStartPosition.position;
    }

    private void SetMonsterPostions(List<GameObject> monsters)
    {
        for(int i = 0; i < monsters.Count; i++)
        {
            monsters[i].transform.position = friendlyMonsterStartPosition[i].position;
        }
    }

    private void SetBossPosition(GameObject bossMonster)
    {
        bossMonster.transform.position = bossStartPosition.position;
    }

    private PolygonCollider2D GetPolygonCollider2D()
    {
        return mapCollider;
    }

    private void OnEnable()
    {
        MyEventManager.MapManager.BossMapManager.OnSetPlayerPosition += SetPlayerPosition;
        MyEventManager.MapManager.BossMapManager.OnGetCameraBounds += GetPolygonCollider2D;
        MyEventManager.MapManager.BossMapManager.OnSetFriendlyMonstersPosition += SetMonsterPostions;
        MyEventManager.MapManager.BossMapManager.OnSetBossPositon += SetBossPosition;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }
    private void OnDisable()
    {
        MyEventManager.MapManager.BossMapManager.OnSetPlayerPosition -= SetPlayerPosition;
        MyEventManager.MapManager.BossMapManager.OnGetCameraBounds -= GetPolygonCollider2D;
        MyEventManager.MapManager.BossMapManager.OnSetFriendlyMonstersPosition -= SetMonsterPostions;
        MyEventManager.MapManager.BossMapManager.OnSetBossPositon -= SetBossPosition;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
