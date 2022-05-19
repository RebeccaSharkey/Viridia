using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrolling, Chasing, Fleeing, Fighting, Stopped }

public class EnemyMovementController : MonoBehaviour
{
    [Header("Monsters Data")]
    private Monster monster;
    [HideInInspector] public NavMeshAgent navMeshAgent;

    [Header("Generic Monsters Data")]
    //Player Detection
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private float waitForPlayerLeaveTime;
    private GameObject player;
    private Collider2D playerCollider;
    private float currentViewRadius;

    //Enemy states
    [HideInInspector] public EnemyState currentState;

    //Idle Variables
    private bool hasWaitTime;
    private float waitTime;
    private float elapseTime;

    //Patrolling Variables
    private bool hasDestination;

    //Fleeing Varaibles
    private bool hasEscaped;

    private void Awake()
    {
        //Sets Piliminary Data
        currentState = EnemyState.Stopped;
    }

    private void Start()
    {
        //Sets Piliminary Data
        player = GameObject.FindGameObjectWithTag("Player");

        //Code Adapted from h8man, 2020
        //Adjusts Nav Mesh Agent to account for NavMesh being 2D
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        //End of Adapted Code
    }

    public void SetUpMonster(Monster newMonster)
    {
        //Sets the monster class
        monster = newMonster;
    }

    private void StartMonsterRoaming(bool toggle)
    {
        if(toggle)
        {
            //Initial movement variables
            hasWaitTime = false;
            hasDestination = false;

            //Initial data for detecting player within view for chasing or fleeing
            playerCollider = null;
            currentViewRadius = monster.normalViewRadius;

            //Starts the monsters movement when everything has been set up correctly
            int randomStart = Random.Range(1, 3);
            switch (randomStart)
            {
                case 1: currentState = EnemyState.Idle; break;
                case 2: currentState = EnemyState.Patrolling; break;
            }
        }
        else
        {
            //Stops Movement
            currentState = EnemyState.Stopped;

            //Initial movement variables
            hasWaitTime = false;
            hasDestination = false;

            //Initial data for detecting player within view for chasing or fleeing
            playerCollider = null;
            currentViewRadius = monster.normalViewRadius;
        }
    }

    private void Update()
    {
        if (currentState != EnemyState.Fighting || currentState != EnemyState.Stopped)
        {
            playerCollider = Physics2D.OverlapCircle(transform.position, currentViewRadius, playerLayerMask);

            if (!playerCollider && (currentState == EnemyState.Fleeing || currentState == EnemyState.Chasing))
            {
                switch (monster.monsterAggression)
                {
                    case MonsterAggression.Shy:
                        hasEscaped = true;
                        break;
                    case MonsterAggression.Aggressive:
                        player.GetComponent<PlayerControls>().isBeingChased = false;
                        navMeshAgent.ResetPath();
                        StopAllCoroutines();
                        StartCoroutine(WaitToUpdateViewRadius());
                        currentState = EnemyState.Idle;
                        break;
                }
            }

            switch (currentState)
            {
                case EnemyState.Idle:
                    if (!hasWaitTime)
                    {
                        waitTime = Random.Range(2f, 5f);
                        elapseTime = 0f;
                        hasWaitTime = true;
                    }
                    else
                    {
                        elapseTime += Time.deltaTime;
                        if (elapseTime >= waitTime)
                        {
                            hasWaitTime = false;
                            currentState = EnemyState.Patrolling;
                        }
                    }
                    break;

                case EnemyState.Patrolling:
                    if (!hasDestination)
                    {
                        Vector2 samplePosition2D = (Random.insideUnitCircle * monster.maxWalkDistance) + new Vector2(transform.position.x, transform.position.y);
                        Vector3 samplePosition = new Vector3(samplePosition2D.x, samplePosition2D.y, 0f);
                        NavMeshHit hit;
                        NavMesh.SamplePosition(samplePosition, out hit, monster.maxWalkDistance, 1);
                        navMeshAgent.SetDestination(hit.position);
                        hasDestination = true;
                    }
                    else
                    {
                        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                        {
                            hasDestination = false;
                            currentState = EnemyState.Idle;
                        }
                    }
                    break;

                case EnemyState.Chasing:
                    navMeshAgent.SetDestination(player.transform.position);
                    if (Vector3.Distance(transform.position, player.transform.position) <= 2f)
                    {
                        MyEventManager.LevelManager.OnSetUpFight?.Invoke(gameObject);
                    }
                    break;

                case EnemyState.Fleeing:
                    if (!hasDestination && currentState != EnemyState.Fighting)
                    {
                        Vector3 tempPostion = transform.position - player.transform.position;
                        tempPostion.Normalize();
                        tempPostion *= monster.maxWalkDistance;
                        NavMeshHit tempHit;
                        NavMesh.SamplePosition(tempPostion, out tempHit, monster.maxWalkDistance, 1);
                        navMeshAgent.SetDestination(tempHit.position);
                        hasDestination = true;
                    }
                    else
                    {
                        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                        {
                            hasDestination = false;
                            if (hasEscaped)
                            {
                                if(currentState != EnemyState.Fighting)
                                { 
                                    currentState = EnemyState.Idle;
                                }
                            }
                        }                        
                    }
                    break;
            }

            if (playerCollider)
            {
                switch (monster.monsterAggression)
                {
                    case MonsterAggression.Shy:
                        if (currentState != EnemyState.Fleeing && currentState != EnemyState.Fighting && currentState != EnemyState.Stopped)
                        {
                            hasDestination = false;
                            hasWaitTime = false;
                            hasEscaped = false;
                            currentState = EnemyState.Fleeing;
                        }
                        break;
                    case MonsterAggression.Aggressive:
                        if (currentState != EnemyState.Chasing)
                        {
                            if (!player.GetComponent<PlayerControls>().isBeingChased)
                            {
                                if (player.GetComponent<PlayerControls>().currentState != PlayerState.Chasing && player.GetComponent<PlayerControls>().currentState != PlayerState.Fighting)
                                {
                                    player.GetComponent<PlayerControls>().isBeingChased = true;
                                    hasDestination = false;
                                    hasWaitTime = false;
                                    currentState = EnemyState.Chasing;
                                    StopAllCoroutines();
                                    StartCoroutine(WaitToUpdateViewRadius());
                                }
                            }
                        }
                        break;
                }
            }
        }
    }

    private void OnFightStarted()
    {
        if (currentState != EnemyState.Fighting)
        {
            navMeshAgent.ResetPath();
            currentState = EnemyState.Stopped;
            gameObject.SetActive(false);
        }
    }

    private void OnFightStopped()
    {
        if(this != null)
        {
            if (currentState == EnemyState.Stopped)
            {
                gameObject.SetActive(true);
                StartMonsterRoaming(true);
            }
        }
    }

    private void ToggleMovement(bool state)
    {
        if(state)
        {
            StartMonsterRoaming(true);
        }
        else
        {
            currentState = EnemyState.Stopped;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, currentViewRadius);
    }

    IEnumerator WaitToUpdateViewRadius()
    {
        yield return new WaitForSeconds(waitTime);
        if (currentState == EnemyState.Chasing)
        {
            currentViewRadius = monster.chasingViewRadius;
        }
        else
        {
            currentViewRadius = monster.normalViewRadius;
        }
    }

    private void OnEnable()
    {
        MyEventManager.Monsters.OnToggleRoaming += StartMonsterRoaming;
        MyEventManager.Monsters.OnToggleMovement += ToggleMovement;

        MyEventManager.Monsters.OnFightStarted += OnFightStarted;
        MyEventManager.Monsters.OnFightStopped += OnFightStopped;

        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.Monsters.OnToggleRoaming -= StartMonsterRoaming;
        MyEventManager.Monsters.OnToggleMovement -= ToggleMovement;

        MyEventManager.Monsters.OnFightStarted -= OnFightStarted;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }

    private void OnDestroy()
    {
        MyEventManager.Monsters.OnToggleRoaming -= StartMonsterRoaming;
        MyEventManager.Monsters.OnToggleMovement -= ToggleMovement;

        MyEventManager.Monsters.OnFightStarted -= OnFightStarted;

        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
        MyEventManager.Monsters.OnFightStopped -= OnFightStopped;
    }
}
