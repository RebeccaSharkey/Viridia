using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public enum PlayerState { Roaming, Chasing, Fighting, Stopped, MovingTo };

public class PlayerControls : MonoBehaviour
{
    [HideInInspector] public Camera thisCamera;
    [HideInInspector] public NavMeshAgent navMeshAgent;

    //Tap || Hold || Double Tap Variables
    private Touch currentTouch;

    //Camera Variables
    public bool followingPlayer;

    //State Variables
    [HideInInspector] public PlayerState currentState;
    private GameObject currentTarget;

    //Variables for monster chasing
    [HideInInspector] public bool isBeingChased;
    private float distanceToObject;

    private void Start()
    {
        currentState = PlayerState.Stopped;

        thisCamera = Camera.main;
        followingPlayer = true;
        isBeingChased = false;
        currentTarget = null;
        distanceToObject = 1.25f;

        //Code Adapted from h8man, 2020
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        //End of Adapted Code
    }

    private void Update()
    {
        if (currentState != PlayerState.Fighting && currentState != PlayerState.Stopped)
        {
            //Handles Player's Tap also overrides chasing a monster
            if (Input.touchCount == 1)
            {
                currentTouch = Input.GetTouch(0);
                if (!EventSystem.current.IsPointerOverGameObject(currentTouch.fingerId))
                {
                    if (currentTouch.phase == TouchPhase.Began)
                    {
                        HandleTap();
                    }
                }
            }

            //Chases current target
            if (currentState == PlayerState.Chasing)
            {
                navMeshAgent.SetDestination(currentTarget.transform.position);

                //If the player is within reach of the target
                if (Vector3.Distance(transform.position, currentTarget.transform.position) <= distanceToObject)
                {
                    switch (currentTarget.tag)
                    {
                        case "Enemy":
                            MyEventManager.LevelManager.OnSetUpFight?.Invoke(currentTarget.gameObject);
                            break;
                        case "Traverse Sign":
                            currentState = PlayerState.Stopped;
                            currentTarget.GetComponent<TraverseSign>().Interact();
                            break;
                        case "Tree":
                            currentState = PlayerState.Stopped;
                            MyEventManager.Monsters.OnToggleMovement?.Invoke(false);
                            currentTarget.GetComponent<Tree>().Interact();
                            break;
                        case "HubInventory":
                            currentState = PlayerState.Stopped;
                            currentTarget.GetComponent<InventorySort>().Interact();
                            break;
                        case "HubHeldMonsters":
                            currentState = PlayerState.Stopped;
                            currentTarget.GetComponent<MonsterSort>().Interact();
                            break;
                        case "HubTree":
                            currentState = PlayerState.Stopped;
                            currentTarget.GetComponent<HubTree>().Interact();
                            break;
                        case "Shop":
                            currentState = PlayerState.Stopped;
                            currentTarget.GetComponent<ShopInventory>().Interact();
                            break;
                    }
                }
            }

            if(currentState == PlayerState.MovingTo)
            {                
                if (Vector3.Distance(navMeshAgent.destination, gameObject.transform.position) <= 1.82f)
                {
                    currentState = PlayerState.Roaming;
                    MyEventManager.Tutorial.OnReachedDestination?.Invoke();
                }
            }
        }

    }

    public void StopFighting()
    {
        currentState = PlayerState.Roaming;
        followingPlayer = true;
        isBeingChased = false;
        currentTarget = null;
    }

    private void HandleTap()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(currentTouch.position), Vector2.zero);
        if(hit)
        {
            switch (hit.collider.gameObject.tag)
            {
                case "Ground":
                    currentState = PlayerState.MovingTo;
                    navMeshAgent.SetDestination(hit.point);
                    distanceToObject = 1.25f;
                    currentTarget = null;
                    break;
                case "Enemy":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "Traverse Sign":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "Tree":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "Player":
                    MyEventManager.UI.MapUI.OnPlayerSettings?.Invoke();
                    break;
                case "HubInventory":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "HubHeldMonsters":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "HubTree":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
                case "Shop":
                    currentState = PlayerState.Chasing;
                    currentTarget = hit.collider.gameObject;
                    distanceToObject = 2.5f;
                    break;
            }
        }
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
