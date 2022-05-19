using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

public class LevelManager : MonoBehaviour
{
    private GameObject player;

    [Header("Traversing Variables")]
    [SerializeField] private List<AssetReference> startUpMaps;
    private bool isNavMeshBaked;
    private bool isFirstMap;
    [SerializeField] private List<Boss> bosses;
    private int mapsTraversed;
    [SerializeField] private AudioClip gameTheme;

    [Header("Fight Phase Variables")]
    [SerializeField] private Transform playerFightPostion;
    [SerializeField] private Transform enemyMonsterFightPostion;
    private GameObject currentTargetedMonster;
    private Vector3 playersOriginalPositon;
    private GameObject currentFighter;
    [SerializeField] private GameObject friendlyMonsterPrefab;
    private bool isFighting = false;
    [SerializeField] private AudioClip fightTheme;
    [SerializeField] private AudioClip hitSound;

    private bool tutorialComplete;

    //Debug
    [SerializeField] private List<Monster> levelMonsters;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        currentTargetedMonster = null;
        playersOriginalPositon = Vector3.zero;
        isFirstMap = true;
        tutorialComplete = false;

        //Get a new random map
        int randomMap = Random.Range(0, startUpMaps.Count);

        //Call IEnumerator to set that map and play
        StartCoroutine(SetUpMapAndStart(startUpMaps[randomMap]));
    }

    #region Traversing
    private void ChangeMap(AssetReference newMap)
    {
        //Stops Players
        player.GetComponent<PlayerControls>().currentState = PlayerState.Stopped;
        MyEventManager.Monsters.OnToggleRoaming?.Invoke(false);

        //Delete all monsters in scene
        MyEventManager.Monsters.Manager.OnDeleteAllMonsters?.Invoke();

        //Switch Cam to loading screen
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(true);
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(false);

        //Stop all coroutines
        MyEventManager.LevelManager.OnStopAllCoroutines?.Invoke();

        if (isFirstMap)
        {
            mapsTraversed = 0;
        }
        else
        {
            //Incriments the amount of maps the player has gone through
            mapsTraversed++;

            //If the player has travelled through ten maps then the boss fight could be activated.
            if (mapsTraversed >= 7)
            {
                //Sends player to the boss scene
                GotToBossScene();

                //This part has been removed so that player can progress quicker may be added back if adavancing after to 7 everytime gets too boring
                /*//Random chance of if the boss area will be called next
                float probability = Random.Range(0f, 1f);
                if (probability >= 0.5f)
                {
                    //Sends player to the boss scene
                    GotToBossScene();

                    //Originally was going to do boss fights in the same scene but gave it it's own scene
                    *//*foreach(Boss boss in bosses)
                    {
                        if(!boss.isCompleted)
                        {
                            StartCoroutine(SetUpMapAndStart(boss.bossMap));
                            return;
                        }
                    }

                    //If the loop finished then all boss fights have been completed so just picks a random boss fight
                    int randomIndex = Random.Range(0, bosses.Count);
                    StartCoroutine(SetUpMapAndStart(bosses[randomIndex].bossMap));
                    return;*//*
                }*/
            }
        }

        //Call IEnumerator to set that map and play
        StartCoroutine(SetUpMapAndStart(newMap));
    }

    IEnumerator SetUpMapAndStart(AssetReference newMap)
    {
        yield return new WaitForSeconds(1f);

        //Starts methods to instantiate and and bake new maps and navmesh
        isNavMeshBaked = false;
        MyEventManager.ViewManager.OnChangeMap?.Invoke(newMap);
        while (!isNavMeshBaked)
        {
            yield return null;
        }

        player.GetComponent<NavMeshAgent>().enabled = true;

        if (isFirstMap)
        {
            isFirstMap = false;

            //Loads player data
            MyEventManager.SaveLoad.OnLoadPlayer?.Invoke();
            yield return new WaitForSeconds(1f);

            //Loads all data
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            yield return new WaitForSeconds(0.5f);

            //No longer needed as player doesn't level up anymore
            /*int levelsToIncrease = (int)MyEventManager.SaveLoad.OnGetPlayerLevel?.Invoke() - 1;
            if(levelsToIncrease > 0)
            {
                for (int i = 0; i < levelsToIncrease; i++)
                {
                    playerManager.LevelUp();
                }
            }*/

            playerManager.currentHealth = playerManager.maxHealth;
            playerManager.currentActionPoints = playerManager.maxActionPoints;
            playerManager.currentMagicPoints = playerManager.maxMagicPoints;

            MyEventManager.Inventory.OnSetUmbraEssence?.Invoke((long)MyEventManager.SaveLoad.OnGetUmbraEssence?.Invoke());
            MyEventManager.Inventory.OnSetInventory?.Invoke(MyEventManager.SaveLoad.OnGetPlayerInventory?.Invoke());
            MyEventManager.Party.OnSetHeldMonsters?.Invoke(MyEventManager.SaveLoad.OnGetPlayerMonsters?.Invoke());
            playerManager.playerName = MyEventManager.SaveLoad.OnGetPlayerName.Invoke();
            if (MyEventManager.SaveLoad.OnGetPlayerSprite.Invoke() != null)
            {
                playerManager.SetSprite(MyEventManager.SaveLoad.OnGetPlayerSprite.Invoke());
            }

            if (MyEventManager.SaveLoad.OnGetAttacks?.Invoke().Count > 0)
            {
                playerManager.attacks = new List<Attack>(MyEventManager.SaveLoad.OnGetAttacks?.Invoke());
            }

            tutorialComplete = (bool)MyEventManager.SaveLoad.OnGetTutorialStatus?.Invoke();

            //Deletes players save data
            MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();

            //Loads Boss Data FOR PARTY ONLY
            MyEventManager.SaveLoad.OnLoadBoss?.Invoke();
            yield return new WaitForSeconds(1f);
            MyEventManager.Party.OnSetParty?.Invoke(MyEventManager.SaveLoad.OnGetPartyMonsters?.Invoke());
        }

        if (tutorialComplete)
        {
            //Spawns all Monsters
            MyEventManager.Monsters.Manager.OnSetUpMonsters?.Invoke();
        }

        yield return new WaitForSeconds(0.5f);
        //Places player at new maps spawn point
        MyEventManager.MapManager.OnSetPlayerPosition?.Invoke();

        //Sets up the tutorial where required
        if (!tutorialComplete)
        {
            MyEventManager.Tutorial.OnNewMapSetUp?.Invoke();
            MyEventManager.Tutorial.OnTutorialSetUp?.Invoke();
        }

        //Moves Camera Back to the main scene
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(true);
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(false);

        //Allows player and AI movement
        yield return new WaitForSeconds(0.5f);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
        MyEventManager.Monsters.OnToggleRoaming?.Invoke(true);

        //Sends event to start tutorial but will only be performed if tutorial set up was called (^see above^)
        MyEventManager.Tutorial.OnBeginTutorial?.Invoke();
    }

    private void GoToHubScene()
    {
        //Change Views
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(false);
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(true);

        //Saves Player Level
        MyEventManager.SaveLoad.OnSetPlayerLevel?.Invoke(player.GetComponent<PlayerManager>().level);

        //Saves Umbra Essence
        MyEventManager.SaveLoad.OnSetUmbraEssence?.Invoke((long)MyEventManager.Inventory.OnGetUmbraEssence?.Invoke());

        //Saves Players Attacks
        MyEventManager.SaveLoad.OnSetAttacks?.Invoke(player.GetComponent<PlayerManager>().attacks);

        //Deletes players data as it's now lost on die
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();

        //Deletes any old monsters in party
        List<Monster> partyMonsters = new List<Monster>();
        MyEventManager.SaveLoad.OnSaveMonstersToBoss?.Invoke(partyMonsters);



        //Load new scene
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("HubWorld");
    }

    public void GotToBossScene()
    {
        //Change Views
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(false);
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(true);

        //Saves all relevent data to player
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();

        MyEventManager.SaveLoad.OnSetPlayerLevel?.Invoke(player.GetComponent<PlayerManager>().level);

        MyEventManager.SaveLoad.OnSetUmbraEssence?.Invoke((long)MyEventManager.Inventory.OnGetUmbraEssence?.Invoke());

        //Saves Players Attacks
        MyEventManager.SaveLoad.OnSetAttacks?.Invoke(player.GetComponent<PlayerManager>().attacks);

        List<Monster> partyMonsters = new List<Monster>();
        partyMonsters = MyEventManager.Party.OnGetParty.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToBoss?.Invoke(partyMonsters);

        List<Monster> monsters = new List<Monster>();
        monsters = MyEventManager.Party.OnGetHeldMonsters?.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToPlayer?.Invoke(monsters);

        Dictionary<Item, int> invnetory = new Dictionary<Item, int>();
        invnetory = MyEventManager.Inventory.OnGetInventory?.Invoke();
        MyEventManager.SaveLoad.OnSaveItemsToPlayer?.Invoke(invnetory);

        //Load new scene
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("BossFight");
    }

    private void OnNavMeshBaked()
    {
        isNavMeshBaked = true;
    }

    #endregion

    #region Fighting Phase
    private void SetUpFight(GameObject currentMonster)
    {
        if (!isFighting)
        {
            isFighting = true;
            MyEventManager.Monsters.OnToggleMovement?.Invoke(false);
            currentTargetedMonster = currentMonster;
            StartCoroutine(StartFight());
        }
    }

    IEnumerator StartFight()
    {
        if (!tutorialComplete)
        {
            tutorialComplete = true;
            MyEventManager.SaveLoad.OnSetTutorialStatus?.Invoke(true);
        }

        //Saves players postition
        playersOriginalPositon = player.transform.position;

        //Switches view to loading
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(true);
        MyEventManager.UI.MapUI.OnClosePlayerSettings?.Invoke();
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(false);
        yield return new WaitForSecondsRealtime(0.5f);

        //Set Up Player
        PlayerControls playerControls = player.GetComponent<PlayerControls>();
        playerControls.currentState = PlayerState.Stopped;
        playerControls.navMeshAgent.ResetPath();
        playerControls.followingPlayer = false;
        playerControls.thisCamera.transform.position = new Vector3(0f, 0f, -10f);
        playerControls.currentState = PlayerState.Fighting;
        player.transform.position = playerFightPostion.position;
        yield return new WaitForSecondsRealtime(0.5f);

        //Set Up Monster
        EnemyMovementController enemyMovementController = currentTargetedMonster.GetComponent<EnemyMovementController>();
        enemyMovementController.navMeshAgent.ResetPath();
        enemyMovementController.currentState = EnemyState.Fighting;
        MyEventManager.Monsters.OnFightStarted?.Invoke();
        currentTargetedMonster.transform.position = enemyMonsterFightPostion.position;
        yield return new WaitForSecondsRealtime(0.5f);

        //Sets Up UI
        currentFighter = player;
        if (currentFighter == player)
        {
            PlayerManager currentManager = currentFighter.GetComponent<PlayerManager>();
            MyEventManager.UI.FightUI.OnSetUpPlayersUI?.Invoke(currentManager.attacks, currentManager.powerUps, currentManager);
        }
        else
        {
            FriendlyMonsterManager currentManager = currentFighter.GetComponent<FriendlyMonsterManager>();
            MyEventManager.UI.FightUI.OnSetUpFightersUI?.Invoke(currentManager.monster.currentAttacks, currentManager.monster.currentPowerUps, currentManager);
        }
        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke("", "White");
        yield return new WaitForSecondsRealtime(0.5f);

        //Switches view to fight scene
        MyEventManager.AudioManager.OnPlayThemeMusic?.Invoke(fightTheme);
        MyEventManager.ViewManager.OnSetFightViewState?.Invoke(true);
        MyEventManager.ViewManager.OnSetLoadViewState?.Invoke(false);
    }

    private void SwitchFighter(Monster newMonster)
    {
        if (currentFighter == player)
        {
            player.SetActive(false);
        }
        else
        {
            MyEventManager.Party.OnAddToParty?.Invoke(currentFighter.GetComponent<FriendlyMonsterManager>().monster);
            Destroy(currentFighter);
        }
        currentFighter = Instantiate(friendlyMonsterPrefab, transform.position, Quaternion.identity);
        FriendlyMonsterManager currentManager = currentFighter.GetComponent<FriendlyMonsterManager>();
        currentManager.Initialize(newMonster);
        currentFighter.transform.position = playerFightPostion.position;
        MyEventManager.Party.OnRemoveFromParty?.Invoke(currentManager.monster);
        MyEventManager.UI.FightUI.OnSetUpFightersUI?.Invoke(currentManager.monster.currentAttacks, currentManager.monster.currentPowerUps, currentManager);
        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke("Current friendly fighter switched to " + currentManager.monster.shortName, "White");
    }
    private void SwitchToPlayer()
    {
        if (currentFighter != player)
        {
            if (currentFighter.GetComponent<FriendlyMonsterManager>().monster.currentHealth > 0)
            {
                MyEventManager.Party.OnAddToParty?.Invoke(currentFighter.GetComponent<FriendlyMonsterManager>().monster);
            }
            Destroy(currentFighter);
            currentFighter = player;
            currentFighter.transform.position = playerFightPostion.position;
            PlayerManager currentManager = currentFighter.GetComponent<PlayerManager>();
            MyEventManager.UI.FightUI.OnSetUpPlayersUI?.Invoke(currentManager.attacks, currentManager.powerUps, currentManager);
            player.SetActive(true);
            MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke("Current friendly fighter switched to " + currentManager.name, "White");
        }
    }
    private void StopFight()
    {
        StopAllCoroutines();

        //Turn On New View
        MyEventManager.ViewManager.OnSetMapViewState?.Invoke(true);

        //Deal with Enemy Monster
        MyEventManager.Monsters.OnFightStopped?.Invoke();

        MyEventManager.Monsters.Manager.OnRemoveMonsterFromScene?.Invoke(currentTargetedMonster);
        currentTargetedMonster = null;

        //Deal With Player
        player.transform.position = playersOriginalPositon;
        player.GetComponent<PlayerManager>().ResetBoosts();
        if (player.GetComponent<PlayerManager>().currentHealth <= 0)
        {
            player.GetComponent<PlayerControls>().followingPlayer = true;
            MyEventManager.UI.MapUI.OnToggleDeathUI?.Invoke(true);
        }
        else
        {
            player.GetComponent<PlayerControls>().StopFighting();
        }

        //Deals with current fighter
        if (currentFighter != player)
        {
            FriendlyMonsterManager monsterManager = currentFighter.GetComponent<FriendlyMonsterManager>();
            if (monsterManager.monster.currentHealth > 0)
            {
                MyEventManager.Party.OnAddToParty?.Invoke(monsterManager.monster);
                Destroy(currentFighter);
            }
            player.SetActive(true);
        }
        currentFighter = null;

        //Deals with party
        List<Monster> monsters = new List<Monster>(MyEventManager.Party.OnGetParty?.Invoke());
        if (monsters.Count > 0)
        {
            foreach (Monster monster in monsters)
            {
                monster.ResetBoosts();
            }
        }

        //Turn Off Old View
        MyEventManager.AudioManager.OnPlayThemeMusic?.Invoke(gameTheme);
        MyEventManager.ViewManager.OnSetFightViewState?.Invoke(false);

        //Toggles fighting phase
        isFighting = false;

        //Continues tutorial
        if (player.GetComponent<PlayerManager>().currentHealth > 0)
        {
            MyEventManager.Tutorial.OnEnemyFaught?.Invoke();
        }
    }

    private void PlayerUsedAbility(Ability usedAbility)
    {
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(false);

        EnemyManager enemyController = currentTargetedMonster.GetComponent<EnemyManager>();

        if (currentFighter == player)
        {
            PlayerManager currentFightController = player.GetComponent<PlayerManager>();

            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.currentAttackSpeed >= enemyController.monster.currentAttackSpeed)
            {
                StartCoroutine(PlayerAttackEnemyFirst(usedAbility, currentFightController, enemyController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(EnemyAttackPlayerFirst(usedAbility, currentFightController, enemyController));
            }
        }
        else
        {
            FriendlyMonsterManager currentFightController = currentFighter.GetComponent<FriendlyMonsterManager>();
            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.monster.currentAttackSpeed >= enemyController.monster.currentAttackSpeed)
            {
                StartCoroutine(MonsterAttackEnemyFirst(usedAbility, currentFightController, enemyController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(EnemyAttackMonsterFirst(usedAbility, currentFightController, enemyController));
            }
        }
    }
    private void PlayerUsedCapture(Item locket)
    {
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(false);

        EnemyManager enemyController = currentTargetedMonster.GetComponent<EnemyManager>();

        if (currentFighter == player)
        {
            PlayerManager currentFightController = player.GetComponent<PlayerManager>();

            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.currentAttackSpeed >= enemyController.monster.currentAttackSpeed)
            {
                StartCoroutine(PlayerCaptureFirst(locket, enemyController, currentFightController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(PlayerCaptureSecond(locket, enemyController, currentFightController));
            }
        }
        else
        {
            FriendlyMonsterManager currentFightController = currentFighter.GetComponent<FriendlyMonsterManager>();
            PlayerManager playerController = player.GetComponent<PlayerManager>();

            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.monster.currentAttackSpeed >= playerController.currentAttackSpeed)
            {
                StartCoroutine(MonsterCaptureFirst(locket, enemyController, currentFightController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(MonsterCaptureSecond(locket, enemyController, currentFightController));
            }
        }
    }
    private void PlayerUsedPotion(Item potion)
    {
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(false);

        EnemyManager enemyController = currentTargetedMonster.GetComponent<EnemyManager>();

        if (currentFighter == player)
        {
            PlayerManager currentFightController = player.GetComponent<PlayerManager>();

            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.currentAttackSpeed >= enemyController.monster.currentAttackSpeed)
            {
                StartCoroutine(PlayerUsesPotionFirst(potion, enemyController, currentFightController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(PlayerUsesPotionSecond(potion, enemyController, currentFightController));
            }
        }
        else
        {
            FriendlyMonsterManager currentFightController = currentFighter.GetComponent<FriendlyMonsterManager>();
            PlayerManager playerController = player.GetComponent<PlayerManager>();

            //Figure Out Who Attacks First
            //Player Goes First
            if (currentFightController.monster.currentAttackSpeed >= playerController.currentAttackSpeed)
            {
                StartCoroutine(MonsterUsesPotionFirst(potion, enemyController, currentFightController));
            }

            //Enemy goes first
            else
            {
                StartCoroutine(MonsterUsesPotionSecond(potion, enemyController, currentFightController));
            }
        }
    }

    //Attacks
    private void ExcuteFighterAbilty(Ability usedAbility, PlayerManager currentFighter, EnemyManager enemyManager)
    {
        if (usedAbility is Attack)
        {
            //Casts ability to attack
            Attack usedAttack = (Attack)usedAbility;

            if (usedAttack.attackType == AttackType.Normal)
            {
                if (!currentFighter.CheckAP(usedAttack.attackUsage))
                {
                    string fightUpdate = currentFighter.playerName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }
            else
            {
                if (!currentFighter.CheckMP(usedAttack.attackUsage))
                {
                    string fightUpdate = currentFighter.playerName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }

            //Figure out if crit hit or not
            float critProbabilty = Random.Range(0f, 1f);

            //Executes a critical attack
            if (critProbabilty <= usedAttack.critChance)
            {
                int totalAttackPower = (int)(currentFighter.currentAttackPower * usedAttack.critAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    currentFighter.UseAP(usedAttack.attackUsage);
                    if (currentFighter.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.attackBoost);
                    }
                }
                else
                {
                    currentFighter.UseMP(usedAttack.attackUsage);
                    if (currentFighter.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.magicBoost);
                    }
                }

                string tempStringColour = "White";
                foreach (Affinity affinity in enemyManager.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempStringColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in enemyManager.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempStringColour = "Red";
                        break;
                    }
                }

                MyEventManager.UI.FightUI.OnUpdateTotalDamageGiven?.Invoke(totalAttackPower);
                enemyManager.TakeDamage(totalAttackPower);
                string fightUpdate = currentFighter.playerName + " Used: Critical " + usedAttack.abilityName;
                if (tempStringColour == "Red")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempStringColour == "Blue")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempStringColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
            else
            {
                int totalAttackPower = (int)(currentFighter.currentAttackPower * usedAttack.attackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    currentFighter.UseAP(usedAttack.attackUsage);
                    if (currentFighter.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.attackBoost);
                    }
                }
                else
                {
                    currentFighter.UseMP(usedAttack.attackUsage);
                    if (currentFighter.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.magicBoost);
                    }
                }

                string tempStringColour = "White";
                foreach (Affinity affinity in enemyManager.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempStringColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in enemyManager.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempStringColour = "Red";
                        break;
                    }
                }

                MyEventManager.UI.FightUI.OnUpdateTotalDamageGiven?.Invoke(totalAttackPower);
                enemyManager.TakeDamage(totalAttackPower);
                string fightUpdate = currentFighter.playerName + " Used: " + usedAttack.abilityName;
                if (tempStringColour == "Red")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempStringColour == "Blue")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempStringColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
        }
        else
        {
            //Impliment Power Ups
        }
    }
    private void ExcuteFighterAbilty(Ability usedAbility, FriendlyMonsterManager currentFighter, EnemyManager enemyManager)
    {
        if (usedAbility is Attack)
        {
            //Casts ability to attack
            Attack usedAttack = (Attack)usedAbility;

            if (usedAttack.attackType == AttackType.Normal)
            {
                if (!currentFighter.CheckAP(usedAttack.attackUsage))
                {
                    string fightUpdate = currentFighter.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }
            else
            {
                if (!currentFighter.CheckMP(usedAttack.attackUsage))
                {
                    string fightUpdate = currentFighter.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }

            //Figure out if crit hit or not
            float critProbabilty = Random.Range(0f, 1f);
            if (critProbabilty <= usedAttack.critChance)
            {
                int totalAttackPower = (int)(currentFighter.monster.currentAttackPower * usedAttack.critAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    currentFighter.UseAP(usedAttack.attackUsage);
                    if (currentFighter.monster.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.monster.attackBoost);
                    }
                }
                else
                {
                    currentFighter.UseMP(usedAttack.attackUsage);
                    if (currentFighter.monster.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.monster.magicBoost);
                    }
                }

                string tempColour = "White";
                foreach (Affinity affinity in enemyManager.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in enemyManager.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }

                MyEventManager.UI.FightUI.OnUpdateTotalDamageGiven?.Invoke(totalAttackPower);
                enemyManager.TakeDamage(totalAttackPower);
                string fightUpdate = currentFighter.monster.shortName + " Used: Critical " + usedAttack.abilityName;
                if (tempColour == "Red")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Blue")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
            else
            {
                int totalAttackPower = (int)(currentFighter.monster.currentAttackPower * usedAttack.attackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    currentFighter.UseAP(usedAttack.attackUsage);
                    if (currentFighter.monster.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.monster.attackBoost);
                    }
                }
                else
                {
                    currentFighter.UseMP(usedAttack.attackUsage);
                    if (currentFighter.monster.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * currentFighter.monster.magicBoost);
                    }
                }

                string tempColour = "White";
                foreach (Affinity affinity in enemyManager.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in enemyManager.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }
                MyEventManager.UI.FightUI.OnUpdateTotalDamageGiven?.Invoke(totalAttackPower);
                enemyManager.TakeDamage(totalAttackPower);

                string fightUpdate = currentFighter.monster.shortName + " Used: " + usedAttack.abilityName;
                if (tempColour == "Red")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Blue")
                {
                    fightUpdate += ". Effective.";
                }

                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
        }
        else
        {
            //Impliment Power Ups
        }
    }
    private void ExecuteEnemyMonsterAbility(Ability usedAbility, EnemyManager enemyManager, PlayerManager currentFighter)
    {
        //Executes Ability
        if (usedAbility is Attack)
        {
            //Casts ability to attack
            Attack usedAttack = (Attack)usedAbility;

            if (usedAttack.attackType == AttackType.Normal)
            {
                if (!enemyManager.CheckAP(usedAttack.attackUsage))
                {
                    string fightUpdate = enemyManager.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }
            else
            {
                if (!enemyManager.CheckMP(usedAttack.attackUsage))
                {
                    string fightUpdate = enemyManager.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }

            //Figure out if crit hit or not
            float critProbabilty = Random.Range(0f, 1f);
            if (critProbabilty <= usedAttack.critChance)
            {
                int totalAttackPower = (int)(enemyManager.monster.currentAttackPower * usedAttack.critAttackPower);

                string tempColour = "White";
                foreach (Affinity affinity in currentFighter.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }
                foreach (Affinity affinity1 in currentFighter.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                currentFighter.TakeDamage(totalAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    enemyManager.UseAP(usedAttack.attackUsage);
                }
                else
                {
                    enemyManager.UseMP(usedAttack.attackUsage);
                }
                string fightUpdate = enemyManager.monster.shortName + " Used: Critical " + usedAttack.abilityName;
                if (tempColour == "Blue")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Red")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
            else
            {
                int totalAttackPower = (int)(enemyManager.monster.currentAttackPower * usedAttack.attackPower);

                string tempColour = "White";
                foreach (Affinity affinity in currentFighter.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }
                foreach (Affinity affinity1 in currentFighter.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                currentFighter.TakeDamage(totalAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    enemyManager.UseAP(usedAttack.attackUsage);
                }
                else
                {
                    enemyManager.UseMP(usedAttack.attackUsage);
                }
                string fightUpdate = enemyManager.monster.shortName + " Used: " + usedAttack.abilityName;
                if (tempColour == "Blue")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Red")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
        }
        else
        {
            //Impliment Power Ups
        }
    }
    private void ExecuteEnemyMonsterAbility(Ability usedAbility, EnemyManager enemyManager, FriendlyMonsterManager currentFighter)
    {
        //Executes Ability
        if (usedAbility is Attack)
        {
            //Casts ability to attack
            Attack usedAttack = (Attack)usedAbility;

            if (usedAttack.attackType == AttackType.Normal)
            {
                if (!enemyManager.CheckAP(usedAttack.attackUsage))
                {
                    string fightUpdate = enemyManager.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }
            else
            {
                if (!enemyManager.CheckMP(usedAttack.attackUsage))
                {
                    string fightUpdate = enemyManager.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                    return;
                }
            }

            //Figure out if crit hit or not
            float critProbabilty = Random.Range(0f, 1f);
            if (critProbabilty <= usedAttack.critChance)
            {
                int totalAttackPower = (int)(enemyManager.monster.currentAttackPower * usedAttack.critAttackPower);

                string tempColour = "White";
                foreach (Affinity affinity in currentFighter.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }
                foreach (Affinity affinity1 in currentFighter.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                currentFighter.TakeDamage(totalAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    enemyManager.UseAP(usedAttack.attackUsage);
                }
                else
                {
                    enemyManager.UseMP(usedAttack.attackUsage);
                }
                string fightUpdate = enemyManager.monster.shortName + " Used: Critical " + usedAttack.abilityName;
                if (tempColour == "Blue")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Red")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
            else
            {
                int totalAttackPower = (int)(enemyManager.monster.currentAttackPower * usedAttack.attackPower);

                string tempColour = "White";
                foreach (Affinity affinity in currentFighter.monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }
                foreach (Affinity affinity1 in currentFighter.monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                currentFighter.TakeDamage(totalAttackPower);
                if (usedAttack.attackType == AttackType.Normal)
                {
                    enemyManager.UseAP(usedAttack.attackUsage);
                }
                else
                {
                    enemyManager.UseMP(usedAttack.attackUsage);
                }

                string fightUpdate = enemyManager.monster.shortName + " Used: " + usedAttack.abilityName;
                if (tempColour == "Blue")
                {
                    fightUpdate += ". Minimal Effect.";
                }
                else if (tempColour == "Red")
                {
                    fightUpdate += ". Effective.";
                }
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, tempColour);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            }
        }
        else
        {
            //Impliment Power Ups
        }
    }

    //Items
    private bool ExcuteCapture(Item locket, EnemyManager enemyManager)
    {
        
        //--------------Capturing used to work purely off level of the monster and whether the lockets where of high enough level to capture them----------------------------------------
        /*if (enemyManager.monster.level <= locket.locketLevel)
        {
            float healthPercentage = enemyManager.monster.currentHealth / enemyManager.monster.maxHealth;
            int result = locket.locketLevel - enemyManager.monster.level;
            switch (result)
            {
                case int n when n >= 5:
                    MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                    MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                    return true;
                case 4:
                    if(healthPercentage <= 0.84f)
                    {
                        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                        MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                        return true;
                    }
                    else
                    {
                        string fightUpdate = "";
                        if (currentFighter == player)
                        {
                            fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        else
                        {
                            fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                        return false;
                    }
                case 3:
                    if (healthPercentage <= 0.68f)
                    {
                        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                        MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                        return true;
                    }
                    else
                    {
                        string fightUpdate = "";
                        if (currentFighter == player)
                        {
                            fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        else
                        {
                            fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                        return false;
                    }
                case 2:
                    if (healthPercentage <= 0.52f)
                    {
                        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                        MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                        return true;
                    }
                    else
                    {
                        string fightUpdate = "";
                        if (currentFighter == player)
                        {
                            fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        else
                        {
                            fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                        return false;
                    }
                case 1:
                    if (healthPercentage <= 0.36f)
                    {
                        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                        MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                        return true;
                    }
                    else
                    {
                        string fightUpdate = "";
                        if (currentFighter == player)
                        {
                            fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        else
                        {
                            fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                        return false;
                    }
                case 0:
                    if (healthPercentage <= 0.2f)
                    {
                        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                        MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                        return true;
                    }
                    else
                    {
                        string fightUpdate = "";
                        if (currentFighter == player)
                        {
                            fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        else
                        {
                            fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
                        }
                        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
                        return false;
                    }
                default:
                    Debug.LogWarning("Result out of range");
                    MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke("Error", "Red");
                    return false;
            }
        }
        else
        {
            string fightUpdate = "";
            if (currentFighter == player)
            {
                fightUpdate = currentFighter.GetComponent<PlayerManager>().playerName + " failed to capture " + enemyManager.gameObject.name;
            }
            else
            {
                fightUpdate = currentFighter.GetComponent<FriendlyMonsterManager>().monster.shortName + " failed to capture " + enemyManager.gameObject.name;
            }
            MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "Red");
            return false;
        }*/


        //However after changing the level system capture system was also changed and the new method now works off setting a capture level for the monsters based off their level and build (sub-level)
        //This method is also far more easier to balance than previous method
        float healthPercentage = enemyManager.monster.currentHealth / enemyManager.monster.maxHealth;
        if (locket.locketLevel == 1)
        {
            if (enemyManager.monster.catchLevel == 1 && healthPercentage <= 0.6)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
        }
        else if (locket.locketLevel == 2)
        {
            if (enemyManager.monster.catchLevel == 1 && healthPercentage <= 0.8)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 2 && healthPercentage <= 0.6)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
        }
        else if (locket.locketLevel == 3)
        {
            if (enemyManager.monster.catchLevel == 1 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 2 && healthPercentage <= 0.8)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 3 && healthPercentage <= 0.6)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
        }
        else if (locket.locketLevel == 4)
        {
            if (enemyManager.monster.catchLevel == 1 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 2 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 3 && healthPercentage <= 0.8)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 4 && healthPercentage <= 0.6)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
        }
        else if (locket.locketLevel == 5)
        {
            if (enemyManager.monster.catchLevel == 1 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 2 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 3 && healthPercentage <= 1)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 4 && healthPercentage <= 0.8)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
            else if (enemyManager.monster.catchLevel == 5 && healthPercentage <= 0.6)
            {
                MyEventManager.Party.OnAddToHeldMonsters?.Invoke(enemyManager.monster);
                MyEventManager.UI.FightUI.OnEnemyCaptuered?.Invoke(enemyManager);
                //MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
                return true;
            }
        }
        else
        {
            MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke($"Cheating in Lockets?? You Naughty Naughty ;P", "Red");
            MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
            return false;
        }
        MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke($"Failed to Capture {enemyManager.monster.shortName}", "Red");
        MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
        return false;
    }
    private void ExecutePotion(Item potion, PlayerManager currentFightController)
    {
        MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
        switch (potion.potionType)
        {
            case PotionType.Health:
                int incriment = potion.potionIncriment;
                if (potion.itemLevel == "Max")
                {
                    incriment = currentFightController.maxHealth;
                }
                currentFightController.Heal(incriment);
                string fightUpdate = currentFightController.playerName + " healed for " + incriment.ToString() + " points.";
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.Stamina:
                int incriment1 = potion.potionIncriment;
                if (potion.itemLevel == "Max")
                {
                    incriment1 = currentFightController.maxActionPoints;
                }
                currentFightController.RestoreAP(incriment1);
                fightUpdate = currentFightController.playerName + "'s AP restored for " + incriment1.ToString() + " points.";
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.Magic:
                int incriment2 = potion.potionIncriment;
                if (potion.itemLevel == "Max")
                {
                    incriment2 = currentFightController.maxMagicPoints;
                }
                currentFightController.RestoreMP(incriment2);
                fightUpdate = currentFightController.playerName + "'s MP restored for " + incriment2.ToString() + " points.";
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;

            case PotionType.AttackBoost:
                currentFightController.attackBoost += potion.potionBoost;
                fightUpdate = currentFightController.playerName + "'s attack boosted by " + potion.potionBoost + ".";
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.MagicBoost:
                currentFightController.magicBoost += potion.potionBoost;
                fightUpdate = currentFightController.playerName + "'s magic attack boosted by " + potion.potionBoost + ".";
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
        }
    }
    private void ExecutePotion(Item potion, FriendlyMonsterManager currentFightController)
    {
        MyEventManager.UI.FightUI.OnUpdateButtons?.Invoke();
        switch (potion.potionType)
        {
            case PotionType.Health:
                currentFightController.Heal(potion.potionIncriment);
                string fightUpdate = currentFightController.monster.shortName + " healed for " + potion.potionIncriment.ToString() + " points.";
                fightUpdate = fightUpdate.Replace("(MonsterSO)", "");
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.Stamina:
                currentFightController.RestoreAP(potion.potionIncriment);
                fightUpdate = currentFightController.monster.shortName + "'s AP restored for " + potion.potionIncriment.ToString() + " points.";
                fightUpdate = fightUpdate.Replace("(MonsterSO)", "");
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.Magic:
                currentFightController.RestoreMP(potion.potionIncriment);
                fightUpdate = currentFightController.monster.shortName + "'s MP restored for " + potion.potionIncriment.ToString() + " points.";
                fightUpdate = fightUpdate.Replace("(MonsterSO)", "");
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;

            case PotionType.AttackBoost:
                currentFightController.monster.attackBoost += potion.potionBoost;
                fightUpdate = currentFightController.monster.shortName + "'s attack boosted by " + potion.potionBoost + ".";
                fightUpdate = fightUpdate.Replace("(MonsterSO)", "");
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
            case PotionType.MagicBoost:
                currentFightController.monster.magicBoost += potion.potionBoost;
                fightUpdate = currentFightController.monster.shortName + "'s magic attack boosted by " + potion.potionBoost + ".";
                fightUpdate = fightUpdate.Replace("(MonsterSO)", "");
                MyEventManager.UI.FightUI.OnUpdateFightUpdates?.Invoke(fightUpdate, "White");
                break;
        }
    }

    //Attack vs Enemy
    IEnumerator PlayerAttackEnemyFirst(Ability usedAbility, PlayerManager currentFighter, EnemyManager enemyManager)
    {
        yield return new WaitForSeconds(1);
        //Executes Players Ability First
        ExcuteFighterAbilty(usedAbility, currentFighter, enemyManager);

        yield return new WaitForSeconds(2f);

        //If Enemy Didn't Die
        if (enemyManager.monster.currentHealth > 0)
        {
            //Gets a random move from the monster
            Ability ennemyAbility = enemyManager.PickMove();
            ExecuteEnemyMonsterAbility(ennemyAbility, enemyManager, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator EnemyAttackPlayerFirst(Ability usedAbility, PlayerManager currentFighter, EnemyManager enemyManager)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability ennemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(ennemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.currentHealth > 0)
        {
            //Executes Players Ability Second
            ExcuteFighterAbilty(usedAbility, currentFighter, enemyManager);

        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator MonsterAttackEnemyFirst(Ability usedAbility, FriendlyMonsterManager currentFighter, EnemyManager enemyManager)
    {
        yield return new WaitForSeconds(1);
        //Executes Players Ability First
        ExcuteFighterAbilty(usedAbility, currentFighter, enemyManager);

        yield return new WaitForSeconds(2f);

        //If Enemy Didn't Die
        if (enemyManager.monster.currentHealth > 0)
        {
            //Gets a random move from the monster
            Ability ennemyAbility = enemyManager.PickMove();

            //Need to add whether the move can be played due to AP

            ExecuteEnemyMonsterAbility(ennemyAbility, enemyManager, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator EnemyAttackMonsterFirst(Ability usedAbility, FriendlyMonsterManager currentFighter, EnemyManager enemyManager)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability ennemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(ennemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.monster.currentHealth > 0)
        {
            //Executes Players Ability Second
            ExcuteFighterAbilty(usedAbility, currentFighter, enemyManager);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }

    //Capture vs Enemy
    IEnumerator PlayerCaptureFirst(Item locket, EnemyManager enemyManager, PlayerManager currentFighter)
    {
        yield return new WaitForEndOfFrame();
        //Executes capture
        bool enemyCaptured = ExcuteCapture(locket, enemyManager);
        MyEventManager.ViewManager.OnFightCamShake?.Invoke();
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);

        yield return new WaitForSeconds(2f);

        //Checks if monster was captured if not attacks
        if (!enemyCaptured)
        {
            //Gets a random move from the monster
            Ability enemyAbility = enemyManager.PickMove();

            //Need to add whether the move can be played due to AP

            ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator MonsterCaptureFirst(Item locket, EnemyManager enemyManager, FriendlyMonsterManager currentFighter)
    {
        yield return new WaitForEndOfFrame();
        //Executes capture
        bool enemyCaptured = ExcuteCapture(locket, enemyManager);
        MyEventManager.ViewManager.OnFightCamShake?.Invoke();
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);

        yield return new WaitForSeconds(2f);

        //Checks if monster was captured if not attacks
        if (!enemyCaptured)
        {
            //Gets a random move from the monster
            Ability enemyAbility = enemyManager.PickMove();

            //Need to add whether the move can be played due to AP

            ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator PlayerCaptureSecond(Item locket, EnemyManager enemyManager, PlayerManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability enemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.currentHealth > 0)
        {
            //Executes Players Ability Second
            ExcuteCapture(locket, enemyManager);
            MyEventManager.ViewManager.OnFightCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator MonsterCaptureSecond(Item locket, EnemyManager enemyManager, FriendlyMonsterManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability enemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.monster.currentHealth > 0)
        {
            //Executes Players Ability Second
            ExcuteCapture(locket, enemyManager);
            MyEventManager.ViewManager.OnFightCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }

    //Potion vs Enemy
    IEnumerator PlayerUsesPotionFirst(Item potion, EnemyManager enemyManager, PlayerManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Executes capture
        ExecutePotion(potion, currentFighter);

        yield return new WaitForSeconds(2f);

        //Gets a random move from the monster
        Ability enemyAbility = enemyManager.PickMove();

        //Need to add whether the move can be played due to AP

        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator MonsterUsesPotionFirst(Item potion, EnemyManager enemyManager, FriendlyMonsterManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Executes capture
        ExecutePotion(potion, currentFighter);

        yield return new WaitForSeconds(2f);

        //Gets a random move from the monster
        Ability enemyAbility = enemyManager.PickMove();

        //Need to add whether the move can be played due to AP

        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator PlayerUsesPotionSecond(Item potion, EnemyManager enemyManager, PlayerManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability enemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.currentHealth > 0)
        {
            //Executes Players action Second
            ExecutePotion(potion, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    IEnumerator MonsterUsesPotionSecond(Item potion, EnemyManager enemyManager, FriendlyMonsterManager currentFighter)
    {
        yield return new WaitForSeconds(1);
        //Gets a random move from the monster and execucutes
        Ability enemyAbility = enemyManager.PickMove();
        ExecuteEnemyMonsterAbility(enemyAbility, enemyManager, currentFighter);

        yield return new WaitForSeconds(2f);

        //If Player Didn't Die
        if (currentFighter.monster.currentHealth > 0)
        {
            //Executes Players action Second
            ExecutePotion(potion, currentFighter);
        }
        yield return new WaitForSeconds(1);
        MyEventManager.UI.FightUI.OnToggleButtons?.Invoke(true);
    }
    #endregion

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;

        MyEventManager.LevelManager.OnTraverseNewMap += ChangeMap;
        MyEventManager.LevelManager.OnNavMeshBaked += OnNavMeshBaked;

        MyEventManager.LevelManager.OnSetUpFight += SetUpFight;
        MyEventManager.LevelManager.OnStopFight += StopFight;

        MyEventManager.LevelManager.OnGoToHubScene += GoToHubScene;

        MyEventManager.LevelManager.FightPhase.OnPlayerChoseAbility += PlayerUsedAbility;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedCapture += PlayerUsedCapture;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedPotion += PlayerUsedPotion;

        MyEventManager.LevelManager.FightPhase.OnSwitchToMonster += SwitchFighter;
        MyEventManager.LevelManager.FightPhase.OnSwitchToPlayer += SwitchToPlayer;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;

        MyEventManager.LevelManager.OnTraverseNewMap -= ChangeMap;
        MyEventManager.LevelManager.OnNavMeshBaked -= OnNavMeshBaked;

        MyEventManager.LevelManager.OnSetUpFight -= SetUpFight;
        MyEventManager.LevelManager.OnStopFight -= StopFight;

        MyEventManager.LevelManager.OnGoToHubScene -= GoToHubScene;

        MyEventManager.LevelManager.FightPhase.OnPlayerChoseAbility -= PlayerUsedAbility;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedCapture -= PlayerUsedCapture;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedPotion -= PlayerUsedPotion;

        MyEventManager.LevelManager.FightPhase.OnSwitchToMonster -= SwitchFighter;
        MyEventManager.LevelManager.FightPhase.OnSwitchToPlayer -= SwitchToPlayer;
    }
}
