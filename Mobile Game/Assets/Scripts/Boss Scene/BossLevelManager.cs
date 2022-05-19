using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class Boss
{
    public string bossName;
    public AssetReference boss;
    public bool isCompleted;

    public Boss()
    {
        bossName = ";";
        boss = null;
        isCompleted = false;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void LoadFromJson(string data)
    {
        JsonUtility.FromJsonOverwrite(data, this);
    }
}

public class BossLevelManager : MonoBehaviour
{
    private GameObject player;
    private List<GameObject> friendlyMonsters;
    private GameObject boss;
    [SerializeField] private AudioClip hitSound;

    [Header("Friendly Monsters Data")]
    [SerializeField] private GameObject friendlyMonsterPrefab;

    [Header("Boss Data")]
    [SerializeField] private List<Boss> bosses;
    private Boss currentBoss;

    private GameObject currentFighter;
    [SerializeField] private List<GameObject> queue;

    private bool canPlayAgain = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        friendlyMonsters = new List<GameObject>();
        StartCoroutine(SetUpScene());
    }
    private void NewSetUp()
    {
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState?.Invoke(true);
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState?.Invoke(false);

        foreach(GameObject gObject in friendlyMonsters)
        {
            Destroy(gObject);
        }
        friendlyMonsters = new List<GameObject>();

        Destroy(boss);

        canPlayAgain = false;

        StartCoroutine(SetUpScene());
    }
    IEnumerator SetUpScene()
    {
        yield return new WaitForEndOfFrame();

        //Loads in boss save and waits a second for it all to be read
        MyEventManager.SaveLoad.OnLoadBoss?.Invoke();
        yield return new WaitForSeconds(1f);

        //If monster list not empty then there is a previous save and loads in that save to work with
        if (MyEventManager.SaveLoad.OnGetBosses?.Invoke().Count != 0)
        {
            bosses.Clear();
            foreach (Boss boss in MyEventManager.SaveLoad.OnGetBosses?.Invoke())
            {
                bosses.Add(boss);
            }
        }
        //Checks if boss save is empty if so then creates a new save full of the initial monster list
        else
        {
            MyEventManager.SaveLoad.OnSaveNewBosses?.Invoke(bosses);
        }

        //Gets the next uncompleted boss
        foreach (Boss boss in bosses)
        {
            if (!boss.isCompleted)
            {
                currentBoss = boss;
                break;
            }
        }

        //If all bosses completed then picks a random boss
        if (currentBoss == null)
        {
            int index = Random.Range(0, bosses.Count);
            currentBoss = bosses[index];
        }

        //Loads Boss Monster
        
        AsyncOperationHandle<GameObject> loadOp = Addressables.LoadAssetAsync<GameObject>(currentBoss.boss);
        while (!loadOp.IsDone)
        {
            yield return null;
        }

        //Instantiates Boss and sets it's position on screen
        if (loadOp.Status == AsyncOperationStatus.Succeeded)
        {
            AsyncOperationHandle<GameObject> asycOp = Addressables.InstantiateAsync(currentBoss.boss);
            while (!asycOp.IsDone)
            {
                yield return null;
            }

            if (asycOp.Status == AsyncOperationStatus.Succeeded)
            {
                boss = asycOp.Result;
            }
            else
            {
                /*Forces game to close as this is game breaking so player will have to restart and hope it works (Never come across a problem however 
                and it always works so this is just a catch all last resort incase for some reason it doesn't work?*/
                Application.Quit();
            }
        }
        else
        {
            /*Forces game to close as this is game breaking so player will have to restart and hope it works (Never come across a problem however 
                and it always works so this is just a catch all last resort incase for some reason it doesn't work?*/
            Application.Quit();
        }

        //Loads Player and sets position
        MyEventManager.MapManager.BossMapManager.OnSetPlayerPosition?.Invoke();

        //Loads player data
        MyEventManager.SaveLoad.OnLoadPlayer?.Invoke();
        yield return new WaitForSeconds(1f);

        //Loads all data
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        yield return new WaitForSeconds(0.5f);

        //Player leveling removed from game
        /*int levelsToIncrease = (int)MyEventManager.SaveLoad.OnGetPlayerLevel?.Invoke() - 1;
        if (levelsToIncrease > 0)
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
        if(MyEventManager.SaveLoad.OnGetAttacks?.Invoke().Count > 0)
        {
            playerManager.attacks = new List<Attack>(MyEventManager.SaveLoad.OnGetAttacks?.Invoke());
        }

        //Sets all friendly Monsters
        if (MyEventManager.SaveLoad.OnGetPartyMonsters?.Invoke().Count != 0)
        {
            foreach (Monster monster in MyEventManager.SaveLoad.OnGetPartyMonsters?.Invoke())
            {
                friendlyMonsters.Add(Instantiate(friendlyMonsterPrefab, transform.position, Quaternion.identity));
                friendlyMonsters[friendlyMonsters.Count - 1].GetComponent<FriendlyMonsterManager>().Initialize(monster);
                monster.currentHealth = monster.maxHealth;
                monster.currentActionPoints = monster.maxActionPoints;
                monster.currentMagicPoints = monster.maxMagicPoints;
            }
        }
        MyEventManager.Party.OnSetParty?.Invoke(MyEventManager.SaveLoad.OnGetPartyMonsters?.Invoke());

        //Moves Camera Back to the main scene
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState?.Invoke(true);
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState?.Invoke(false);

        //Loads Boss Dialogue
        MyEventManager.MapManager.BossMapManager.OnSetFriendlyMonstersPosition?.Invoke(friendlyMonsters);
        MyEventManager.MapManager.BossMapManager.OnSetBossPositon?.Invoke(boss);
        MyEventManager.ViewManager.BossViewManager.OnSetBossUI?.Invoke(true);
        boss.GetComponent<BossManager>().StartStory();
    }

    private void StartFight()
    {
        //Populates Queue
        queue = new List<GameObject>();
        int amount = 0;
        while(amount < 10)
        {
            if(PopulateQueue())
            {
                amount++;
            }
        }
        MyEventManager.UI.BossUI.OnUpdateQueue?.Invoke(queue);
        NewFighter();
    }
    private bool PopulateQueue()
    {
        int index = Random.Range(0, 6 + friendlyMonsters.Count);
        if (index == 0 || index == 1 || index == 2)
        {
            if(queue.Count != 0)
            {
                if (queue[queue.Count - 1] == player && !boss.GetComponent<BossManager>().monster.shortName.Contains("Faust, Apotheosis of Pride"))
                {
                    return false;
                }
                else
                {
                    queue.Add(player);
                    return true;
                }
            }
            else
            {
                queue.Add(player);
                return true;
            }
        }
        else if (index == 3 || index == 4 || index == 5)
        {
            if(!boss.GetComponent<BossManager>().monster.shortName.Contains("Faust, Apotheosis of Pride"))
            {
                if(queue.Count != 0)
                {
                    if(queue[queue.Count - 1] == boss)
                    {
                        return false;
                    }
                    else
                    {
                        queue.Add(boss);
                        return true;
                    }
                }
                else
                {
                    queue.Add(boss);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            queue.Add(friendlyMonsters[index - 6]);
            return true;
        }
    }
    private void RemoveFromQueue(GameObject fighterToRemove)
    {
        queue.RemoveAll(x => x == fighterToRemove);
        while(queue.Count < 10)
        {
            PopulateQueue();
        }
    }
    private void NewFighter()
    {
        //Sets New fighter
        if (player.GetComponent<PlayerManager>().currentHealth > 0 && boss.GetComponent<BossManager>().monster.currentHealth > 0)
        {
            currentFighter = queue[0];
            queue.RemoveAt(0);
            while (queue.Count < 10)
            {
                PopulateQueue();
            }
            MyEventManager.UI.BossUI.OnUpdateQueue?.Invoke(queue);

            //Sets up the UI for current fighter
            if (!currentFighter.CompareTag("Enemy"))
            {
                MyEventManager.UI.BossUI.OnSetUpNewFighter?.Invoke(currentFighter);
                StartCoroutine(Timer(5));
            }
            else
            {
                MyEventManager.UI.BossUI.OnToggleFightUI?.Invoke(false);
                StartCoroutine(Timer(3));
            }
        }
        else if (player.GetComponent<PlayerManager>().currentHealth <= 0)
        {
            MyEventManager.UI.BossUI.OnTogglePlayerDiedUI?.Invoke(canPlayAgain);
        }
        else if (boss.GetComponent<BossManager>().monster.currentHealth <= 0)
        {
            foreach(Boss thisBoss in bosses)
            {
                if(thisBoss.bossName == currentBoss.bossName)
                {
                    thisBoss.isCompleted = true;
                }
            }
            MyEventManager.UI.BossUI.OnToggleBossDiedUI?.Invoke(boss.GetComponent<BossManager>().monster);
        }
    }

    IEnumerator Timer(int allowedTime)
    {
        float elapsedTime = allowedTime;

        while (elapsedTime >= 0f)
        {
            elapsedTime -= Time.deltaTime;
            MyEventManager.UI.BossUI.OnUpdateTimer?.Invoke(elapsedTime / allowedTime);
            yield return null;
        }

        if (currentFighter.CompareTag("Enemy"))
        {
            BossAbility();
        }
        else
        {
            NewFighter();
        }
    }

    private void BossAbility()
    {
        MyEventManager.BossManager.OnPickRandomAbility?.Invoke();
    }

    private void AbilityUsed(Ability ability)
    {
        StopAllCoroutines();
        string fightUpdate = "";
        string tempColour = "White";
        int totalAttackPower = 0;
        if (ability is Attack)
        {
            //Casts ability to attack
            Attack usedAttack = (Attack)ability;

            //If player            
            if (currentFighter.CompareTag("Player"))
            {
                PlayerManager _currentFighter = currentFighter.GetComponent<PlayerManager>();

                //Checks if attack can be performed
                if (usedAttack.attackType == AttackType.Normal)
                {
                    if (!_currentFighter.CheckAP(usedAttack.attackUsage))
                    {
                        fightUpdate = _currentFighter.playerName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");
                        return;
                    }
                }
                else
                {
                    if (!_currentFighter.CheckMP(usedAttack.attackUsage))
                    {
                        fightUpdate = _currentFighter.playerName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");
                        return;
                    }
                }

                //Figure out if crit hit or not
                float critProbabilty = Random.Range(0f, 1f);
                bool critUsed = false;

                if (critProbabilty <= usedAttack.critChance)
                {
                    critUsed = true;
                    totalAttackPower = (int)(_currentFighter.currentAttackPower * usedAttack.critAttackPower);
                }
                else
                {
                    totalAttackPower = (int)(_currentFighter.currentAttackPower * usedAttack.attackPower);
                }

                //Adds any boosts to the attack if there are any
                if (usedAttack.attackType == AttackType.Normal)
                {
                    _currentFighter.UseAP(usedAttack.attackUsage);
                    if (_currentFighter.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * _currentFighter.attackBoost);
                    }
                }
                else
                {
                    _currentFighter.UseMP(usedAttack.attackUsage);
                    if (_currentFighter.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * _currentFighter.magicBoost);
                    }
                }

                tempColour = "White";
                //Checks affinities
                foreach (Affinity affinity in boss.GetComponent<BossManager>().monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier); 
                        tempColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in boss.GetComponent<BossManager>().monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier); 
                        tempColour = "Red";
                        break;
                    }
                }

                boss.GetComponent<BossManager>().monster.TakeDamage(totalAttackPower);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
                if (critUsed)
                {
                    fightUpdate = _currentFighter.playerName + " Used: Critical " + usedAttack.abilityName;
                }
                else
                {
                    fightUpdate = _currentFighter.playerName + " Used: " + usedAttack.abilityName;
                }

                if(tempColour == "Blue")
                {
                    fightUpdate += ". Effective";
                }
                else if(tempColour == "Red")
                {
                    fightUpdate += ". Minimal Effect";
                }

                MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, tempColour);
            }

            //If Monster
            else
            {
                FriendlyMonsterManager _currentFighter = currentFighter.GetComponent<FriendlyMonsterManager>();

                //Checks if attack can be performed
                if (usedAttack.attackType == AttackType.Normal)
                {
                    if (!_currentFighter.CheckAP(usedAttack.attackUsage))
                    {
                        fightUpdate = _currentFighter.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough AP.";
                        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");
                        return;
                    }
                }
                else
                {
                    if (!_currentFighter.CheckMP(usedAttack.attackUsage))
                    {
                        fightUpdate = _currentFighter.monster.shortName + " tried to use " + usedAttack.abilityName + " but doesn't have enough MP.";
                        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");
                        return;
                    }
                }

                //Figure out if crit hit or not
                float critProbabilty = Random.Range(0f, 1f);
                bool critUsed = false;

                if (critProbabilty <= usedAttack.critChance)
                {
                    critUsed = true;
                    totalAttackPower = (int)(_currentFighter.monster.currentAttackPower * usedAttack.critAttackPower);
                }
                else
                {
                    totalAttackPower = (int)(_currentFighter.monster.currentAttackPower * usedAttack.attackPower);
                }

                //Adds any boosts to the attack if there are any
                if (usedAttack.attackType == AttackType.Normal)
                {
                    _currentFighter.UseAP(usedAttack.attackUsage);
                    if (_currentFighter.monster.attackBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * _currentFighter.monster.attackBoost);
                    }
                }
                else
                {
                    _currentFighter.UseMP(usedAttack.attackUsage);
                    if (_currentFighter.monster.magicBoost > 0)
                    {
                        totalAttackPower += (int)(totalAttackPower * _currentFighter.monster.magicBoost);
                    }
                }
                //Checks affinities
                foreach (Affinity affinity in boss.GetComponent<BossManager>().monster.weakness)
                {
                    if (affinity.damageType == usedAttack.damageType)
                    {
                        totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                        tempColour = "Blue";
                        break;
                    }
                }
                foreach (Affinity affinity1 in boss.GetComponent<BossManager>().monster.immunities)
                {
                    if (affinity1.damageType == usedAttack.damageType)
                    {
                        totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                        tempColour = "Red";
                        break;
                    }
                }

                boss.GetComponent<BossManager>().monster.TakeDamage(totalAttackPower);
                MyEventManager.ViewManager.OnFightCamShake?.Invoke();
                MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
                if (critUsed)
                {
                    fightUpdate = _currentFighter.monster.shortName + " Used: Critical " + usedAttack.abilityName;
                }
                else
                {
                    fightUpdate = _currentFighter.monster.shortName + " Used: " + usedAttack.abilityName;
                }

                if (tempColour == "Blue")
                {
                    fightUpdate += ". Effective";
                }
                else if (tempColour == "Red")
                {
                    fightUpdate += ". Minimal Effect";
                }

            }
        }

        BossManager bossManager = boss.GetComponent<BossManager>();
        if (boss.GetComponent<BossManager>().monster.shortName.Contains("Rancor, Aspect of Envy"))
        {
            EnvyAbilities envyAbilities = boss.GetComponent<EnvyAbilities>();
            if (bossManager.monster.currentHealth <= 100)
            {
                if(envyAbilities.currentAspect != DamageType.Umbra)
                {
                    envyAbilities.currentAspect = DamageType.Umbra;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Normal, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 1f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Air, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Water, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Earth, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Fire, 0.5f));
                }
            }
            else if(bossManager.monster.currentHealth <= 200)
            {
                if (envyAbilities.currentAspect != DamageType.Fire)
                {
                    envyAbilities.currentAspect = DamageType.Fire;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Air, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Water, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Fire, 1f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 0.5f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Normal, 0.5f));
                }
            }
            else if(bossManager.monster.currentHealth <= 300)
            {
                if (envyAbilities.currentAspect != DamageType.Water)
                {
                    envyAbilities.currentAspect = DamageType.Water;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Air, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Earth, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Water, 1f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 0.5f));
                }
            }
            else if(bossManager.monster.currentHealth <= 400)
            {
                if (envyAbilities.currentAspect != DamageType.Earth)
                {
                    envyAbilities.currentAspect = DamageType.Earth;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Fire, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Air, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Water, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Earth, 1f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 0.5f));
                }
            }
            else if(bossManager.monster.currentHealth <= 500)
            {
                if (envyAbilities.currentAspect != DamageType.Air)
                {
                    envyAbilities.currentAspect = DamageType.Air;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Fire, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Earth, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Normal, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Air, 1f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 0.5f));
                }
            }
            else if(bossManager.monster.currentHealth <= 600)
            {
                if (envyAbilities.currentAspect != DamageType.Normal)
                {
                    envyAbilities.currentAspect = DamageType.Normal;
                    bossManager.monster.weakness.Clear();
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Fire, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Air, 0.5f));
                    bossManager.monster.weakness.Add(new Affinity(DamageType.Water, 0.5f));
                    bossManager.monster.immunities.Clear();
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Normal, 1f));
                    bossManager.monster.immunities.Add(new Affinity(DamageType.Umbra, 0.5f));
                }
            }
            fightUpdate += $"{bossManager.monster.shortName} changed their affinity to {envyAbilities.currentAspect} type.";
        }
        else if(boss.GetComponent<BossManager>().monster.shortName.Contains("Faust, Apotheosis of Pride"))
        {
            if (currentFighter.CompareTag("Player"))
            {
                currentFighter.GetComponent<PlayerManager>().TakeDamage((int)(totalAttackPower * 1.1));
            }
            else
            {
                currentFighter.GetComponent<FriendlyMonsterManager>().TakeDamage((int)(totalAttackPower * 1.1));
            }
            fightUpdate += $"\n\n{bossManager.monster.shortName} mimiced the attack right back at you";
        }
        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, tempColour);
    }
    private void ItemUsed(Item potion)
    {
        StopAllCoroutines();
        //If Player
        if (currentFighter.CompareTag("Player"))
        {
            PlayerManager currentFightController = currentFighter.GetComponent<PlayerManager>();
            switch (potion.potionType)
            {
                case PotionType.Health:
                    currentFightController.Heal(potion.potionIncriment);
                    string fightUpdate = currentFightController.playerName + " healed for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.Stamina:
                    currentFightController.RestoreAP(potion.potionIncriment);
                    fightUpdate = currentFightController.playerName + "'s AP restored for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.Magic:
                    currentFightController.RestoreMP(potion.potionIncriment);
                    fightUpdate = currentFightController.playerName + "'s MP restored for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;

                case PotionType.AttackBoost:
                    currentFightController.attackBoost += potion.potionBoost;
                    fightUpdate = currentFightController.playerName + "'s attack boosted by " + potion.potionBoost + ".";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.MagicBoost:
                    currentFightController.magicBoost += potion.potionBoost;
                    fightUpdate = currentFightController.playerName + "'s magic attack boosted by " + potion.potionBoost + ".";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
            }
        }

        //If Monster
        else
        {
            FriendlyMonsterManager currentMonsterController = currentFighter.GetComponent<FriendlyMonsterManager>();
            switch (potion.potionType)
            {
                case PotionType.Health:
                    currentMonsterController.monster.Heal(potion.potionIncriment);
                    string fightUpdate = currentMonsterController.monster.shortName + " healed for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.Stamina:
                    currentMonsterController.monster.RestoreAP(potion.potionIncriment);
                    fightUpdate = currentMonsterController.monster.shortName + "'s AP restored for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.Magic:
                    currentMonsterController.monster.RestoreMP(potion.potionIncriment);
                    fightUpdate = currentMonsterController.monster.shortName + "'s MP restored for " + potion.potionIncriment.ToString() + " points.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;

                case PotionType.AttackBoost:
                    currentMonsterController.monster.attackBoost += potion.potionBoost;
                    fightUpdate = currentMonsterController.monster.shortName + "'s attack boosted by " + potion.potionBoost + ".";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
                case PotionType.MagicBoost:
                    currentMonsterController.monster.magicBoost += potion.potionBoost;
                    fightUpdate = currentMonsterController.monster.shortName + "'s magic attack boosted by " + potion.potionBoost + ".";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    break;
            }
        }
    }

    //All boss abilities
    private void MonsterAttackAbilitySolo(int damage, DamageType damageType, string abilityName)
    {
        //Gets random Target
        int index = Random.Range(0, friendlyMonsters.Count + 1);
        SingleAttack(index, damage, damageType, abilityName);
        
    }
    private void SingleAttack(int index, int damage, DamageType damageType, string abilityName)
    {
        string tempColour = "White";
        //If target is player
        if (index == friendlyMonsters.Count)
        {
            //Checks affinities
            foreach (Affinity affinity in player.GetComponent<PlayerManager>().weakness)
            {
                if (affinity.damageType == damageType)
                {
                    damage += (int)(damage * affinity.multiplier);
                    tempColour = "Red";
                    break;
                }
            }
            foreach (Affinity affinity1 in player.GetComponent<PlayerManager>().immunities)
            {
                if (affinity1.damageType == damageType)
                {
                    damage -= (int)(damage * affinity1.multiplier);
                    tempColour = "Blue";
                    break;
                }
            }

            player.GetComponent<PlayerManager>().TakeDamage(damage);
            MyEventManager.ViewManager.OnFightCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            string text = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on {player.GetComponent<PlayerManager>().playerName}.";
            if (tempColour == "Blue")
            {
                text += " Minimal Effect.";
            }
            else if (tempColour == "Red")
            {
                text += " Effective.";
            }
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(text, tempColour);
        }
        //If Target is monster
        else
        {
            //Checks affinities
            foreach (Affinity affinity in friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.weakness)
            {
                if (affinity.damageType == damageType)
                {
                    damage += (int)(damage * affinity.multiplier);
                    tempColour = "Red";
                    break;
                }
            }
            foreach (Affinity affinity1 in friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.immunities)
            {
                if (affinity1.damageType == damageType)
                {
                    damage -= (int)(damage * affinity1.multiplier);
                    tempColour = "Blue";
                    break;
                }
            }
            friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.TakeDamage(damage);
            MyEventManager.ViewManager.OnFightCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);

            //Checks if monster died
            if (friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.currentHealth <= 0)
            {
                MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on {friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.shortName}" +
                    $"\n\n{friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.shortName} has died.", "Red");
                MyEventManager.Party.OnRemoveFromParty?.Invoke(friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster);
                GameObject temp = friendlyMonsters[index];
                Destroy(friendlyMonsters[index]);
                friendlyMonsters.Remove(temp);
                RemoveFromQueue(temp);
            }
            else
            {
                string text = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on {friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.shortName}.";
                if (tempColour == "Blue")
                {
                    text += " Minimal Effect.";
                }
                else if (tempColour == "Red")
                {
                    text += " Effective.";
                }

                if(abilityName == "Loan Shark")
                {
                    text += $" You recieved {damage} Umbra Essence.";
                    MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(damage);
                }
                MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(text, tempColour);
            }
        }
    }
    private void MonsterAttackAbilityAll(int damage, DamageType damageType, string abilityName)
    {
        //Player First
        //Checks affinities
        foreach (Affinity affinity in player.GetComponent<PlayerManager>().weakness)
        {
            if (affinity.damageType == damageType)
            {
                damage += (int)(damage * affinity.multiplier);
                break;
            }
        }
        foreach (Affinity affinity1 in player.GetComponent<PlayerManager>().immunities)
        {
            if (affinity1.damageType == damageType)
            {
                damage -= (int)(damage * affinity1.multiplier);
                break;
            }
        }
        player.GetComponent<PlayerManager>().TakeDamage(damage);

        //Then Monsters
        foreach (GameObject monster in friendlyMonsters)
        {
            //Checks affinities
            foreach (Affinity affinity in monster.GetComponent<FriendlyMonsterManager>().monster.weakness)
            {
                if (affinity.damageType == damageType)
                {
                    damage += (int)(damage * affinity.multiplier);
                    break;
                }
            }
            foreach (Affinity affinity1 in monster.GetComponent<FriendlyMonsterManager>().monster.immunities)
            {
                if (affinity1.damageType == damageType)
                {
                    damage -= (int)(damage * affinity1.multiplier);
                    break;
                }
            }
            monster.GetComponent<FriendlyMonsterManager>().monster.TakeDamage(damage);
        }
        
        List<GameObject> tempMonsters = new List<GameObject>();
        foreach(GameObject monster in friendlyMonsters)
        {
            if(monster.GetComponent<FriendlyMonsterManager>().monster.currentHealth <= 0)
            {
                tempMonsters.Add(monster);
            }
        }
        foreach(GameObject deadMonster in tempMonsters)
        {
            MyEventManager.Party.OnRemoveFromParty?.Invoke(deadMonster.GetComponent<FriendlyMonsterManager>().monster);
            GameObject temp = deadMonster;
            Destroy(deadMonster);
            friendlyMonsters.Remove(temp);
            RemoveFromQueue(temp);
        }

        MyEventManager.ViewManager.OnFightCamShake?.Invoke();
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on everyone", "Red");
    }
    private void MonsterBoostReset(string abilityName)
    {
        int boostsUsed = 0;
        MyEventManager.ViewManager.OnFightCamShake?.Invoke();
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        if(playerManager.attackBoost > 0)
        {
            boostsUsed++;
        }
        if(playerManager.magicBoost > 0)
        {
            boostsUsed++;
        }
        playerManager.ResetBoosts();

        foreach (GameObject monster in friendlyMonsters)
        {
            if (monster.GetComponent<FriendlyMonsterManager>().monster.attackBoost > 0)
            {
                boostsUsed++;
            }
            if (monster.GetComponent<FriendlyMonsterManager>().monster.magicBoost > 0)
            {
                boostsUsed++;
            }
            monster.GetComponent<FriendlyMonsterManager>().monster.ResetBoosts();
        }

        if(boostsUsed == 0)
        {
            boss.GetComponent<BossManager>().monster.TakeDamage((int)(10 * boss.GetComponent<BossManager>().monster.baseLevelMultiplier) * boss.GetComponent<BossManager>().monster.level);
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on everyone. No" +
                $" fighters had any boosts so {boss.GetComponent<BossManager>().monster.shortName} took damage.", "Red");
        }
        else
        {
            boss.GetComponent<BossManager>().monster.Heal((int)(10 * boss.GetComponent<BossManager>().monster.baseLevelMultiplier) * boss.GetComponent<BossManager>().monster.level);
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} on everyone. Boosts" +
                $" were active so {boss.GetComponent<BossManager>().monster.shortName} healed.", "Red");
        }
        
    }
    private void MonsterCharm(string abilityName)
    {
        if(friendlyMonsters.Count <= 0)
        {
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} tried to use {abilityName} to charm a friendly Umbra but " +
                $"{player.GetComponent<PlayerManager>().playerName} has no friendly" +
                $" Umbra in the party.", "Red");
        }
        else
        {
            int index = Random.Range(0, friendlyMonsters.Count);
            FriendlyMonsterManager _currentFighter = friendlyMonsters[index].GetComponent<FriendlyMonsterManager>();
            index = Random.Range(0, _currentFighter.monster.allowedAttacks.Count);
            Attack usedAttack = _currentFighter.monster.allowedAttacks[index];

            string fightUpdate = "";
            //Checks if attack can be performed
            if (usedAttack.attackType == AttackType.Normal)
            {
                if (!_currentFighter.CheckAP(usedAttack.attackUsage))
                {
                    fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to charm {_currentFighter.monster.shortName}. {_currentFighter.monster.shortName}" +
                        $" tried to use {usedAttack.abilityName} on you but doesn't have enough AP.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    return;
                }
            }
            else
            {
                if (!_currentFighter.CheckMP(usedAttack.attackUsage))
                {
                    fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to charm {_currentFighter.monster.shortName}. {_currentFighter.monster.shortName}" +
                        $" tried to use {usedAttack.abilityName} on you but doesn't have enough MP.";
                    MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "White");
                    return;
                }
            }

            //Figure out if crit hit or not
            float critProbabilty = Random.Range(0f, 1f);
            int totalAttackPower = 0;
            bool critUsed = false;

            if (critProbabilty <= usedAttack.critChance)
            {
                critUsed = true;
                totalAttackPower = (int)(_currentFighter.monster.currentAttackPower * usedAttack.critAttackPower);
            }
            else
            {
                totalAttackPower = (int)(_currentFighter.monster.currentAttackPower * usedAttack.attackPower);
            }

            //Adds any boosts to the attack if there are any
            if (usedAttack.attackType == AttackType.Normal)
            {
                _currentFighter.UseAP(usedAttack.attackUsage);
                if (_currentFighter.monster.attackBoost > 0)
                {
                    totalAttackPower += (int)(totalAttackPower * _currentFighter.monster.attackBoost);
                }
            }
            else
            {
                _currentFighter.UseMP(usedAttack.attackUsage);
                if (_currentFighter.monster.magicBoost > 0)
                {
                    totalAttackPower += (int)(totalAttackPower * _currentFighter.monster.magicBoost);
                }
            }
            string tempColour = "White";
            //Checks affinities
            foreach (Affinity affinity in player.GetComponent<PlayerManager>().weakness)
            {
                if (affinity.damageType == usedAttack.damageType)
                {
                    totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                    tempColour = "Red";
                    break;
                }
            }
            foreach (Affinity affinity1 in player.GetComponent<PlayerManager>().immunities)
            {
                if (affinity1.damageType == usedAttack.damageType)
                {
                    totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                    tempColour = "Blue";
                    break;
                }
            }

            if (critUsed)
            {
                fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to charm {_currentFighter.monster.shortName}. {_currentFighter.monster.shortName}" +
                        $" used: Critical {usedAttack.abilityName} on you.";
            }
            else
            {
                fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to charm {_currentFighter.monster.shortName}. {_currentFighter.monster.shortName}" +
                        $" used: {usedAttack.abilityName} on you.";
            }

            if (tempColour == "Blue")
            {
                fightUpdate += ". Minimal Effect";
            }
            else if (tempColour == "Red")
            {
                fightUpdate += ". Effective";
            }

            player.GetComponent<PlayerManager>().TakeDamage(totalAttackPower);
            MyEventManager.ViewManager.OnFightCamShake?.Invoke();
            MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, tempColour);
        }
    }
    private void MonsterSummon(Monster monster, string abilityName)
    {
        Attack usedAttack = monster.allowedAttacks[Random.Range(0, monster.allowedAttacks.Count)];

        string fightUpdate = "";
        //Figure out if crit hit or not
        float critProbabilty = Random.Range(0f, 1f);
        int totalAttackPower = 0;
        bool critUsed = false;

        if (critProbabilty <= usedAttack.critChance)
        {
            critUsed = true;
            totalAttackPower = (int)(monster.currentAttackPower * usedAttack.critAttackPower);
        }
        else
        {
            totalAttackPower = (int)(monster.currentAttackPower * usedAttack.attackPower);
        }

        //Adds any boosts to the attack if there are any
        if (usedAttack.attackType == AttackType.Normal)
        {
            if (monster.attackBoost > 0)
            {
                totalAttackPower += (int)(totalAttackPower * monster.attackBoost);
            }
        }
        else
        {
            if (monster.magicBoost > 0)
            {
                totalAttackPower += (int)(totalAttackPower * monster.magicBoost);
            }
        }

        string tempColour = "White";
        //Checks affinities
        foreach (Affinity affinity in player.GetComponent<PlayerManager>().weakness)
        {
            if (affinity.damageType == usedAttack.damageType)
            {
                totalAttackPower += (int)(totalAttackPower * affinity.multiplier);
                tempColour = "Red";
                break;
            }
        }
        foreach (Affinity affinity1 in player.GetComponent<PlayerManager>().immunities)
        {
            if (affinity1.damageType == usedAttack.damageType)
            {
                totalAttackPower -= (int)(totalAttackPower * affinity1.multiplier);
                tempColour = "Blue";
                break;
            }
        }

        if (critUsed)
        {
            fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to summon a {monster.fullName}. {monster.fullName}" +
                    $" used: Critical {usedAttack.abilityName} on you.";
        }
        else
        {
            fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to summon a {monster.fullName}. {monster.fullName}" +
                    $" used: {usedAttack.abilityName} on you.";
        }

        if (tempColour == "Blue")
        {
            fightUpdate += ". Minimal Effect";
        }
        else if (tempColour == "Red")
        {
            fightUpdate += ". Effective";
        }

        player.GetComponent<PlayerManager>().TakeDamage(totalAttackPower);
        MyEventManager.ViewManager.OnFightCamShake?.Invoke();
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(hitSound);
        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, tempColour);
    }
    private void StealPotion(string abilityName)
    {
        List<Item> potions = new List<Item>();

        foreach(KeyValuePair<Item, int> item in MyEventManager.Inventory.OnGetInventory?.Invoke())
        {
            if(item.Key.itemType == ItemType.Potion)
            {
                if(item.Key.potionType == PotionType.Health)
                {
                    potions.Add(item.Key);
                }
            }
        }

        if(potions.Count <= 0)
        {
            string fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} to try and steal a health potion from you." +
                $" However, you have no health potions in your inventory to steal.";
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");
        }
        else
        {
            Item potion = potions[Random.Range(0, potions.Count)];
            MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(potion, 1);
            boss.GetComponent<BossManager>().monster.Heal(potion.potionIncriment); 
            string fightUpdate = $"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} and stole a health potion from you." +
                $" They healed for {potion.potionIncriment} points.";
            MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke(fightUpdate, "Red");            
        }
        
    }
    private void StealUmbraEssence(int amount, string abilityName)
    {
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(amount);
        MyEventManager.UI.BossUI.OnUpdateFightUI?.Invoke($"{boss.GetComponent<BossManager>().monster.shortName} used {abilityName} and stole {amount} Umbra Essence.", "Red");
    }
    private void UmbraAttack(string abilityName)
    {
        int index = Random.Range(0, friendlyMonsters.Count + 1);
        int damage = 1;
        //If target is player
        if (index == friendlyMonsters.Count)
        {
            damage = (int)Mathf.Clamp((long)(MyEventManager.SaveLoad.OnGetUmbraEssence?.Invoke() * 0.01), 1, (player.GetComponent<PlayerManager>().maxHealth / 3));
        }
        else
        {
            damage = (int)Mathf.Clamp((long)(MyEventManager.SaveLoad.OnGetUmbraEssence?.Invoke() * 0.01), 1, (friendlyMonsters[index].GetComponent<FriendlyMonsterManager>().monster.maxHealth / 3));
        }
        SingleAttack(index, damage, DamageType.Umbra, abilityName);
    }

    private void PlayerDiedHub()
    {
        //Change Views
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState?.Invoke(false);
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState?.Invoke(true);

        //Saves Player Level
        MyEventManager.SaveLoad.OnSetPlayerLevel?.Invoke(player.GetComponent<PlayerManager>().level);
        
        //Saves Umbra Essence
        MyEventManager.SaveLoad.OnSetUmbraEssence?.Invoke((long)MyEventManager.Inventory.OnGetUmbraEssence?.Invoke());

        //Saves Players Attacks
        MyEventManager.SaveLoad.OnSetAttacks?.Invoke(player.GetComponent<PlayerManager>().attacks);

        //Deletes players data as it's now lost on die
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();

        //Deletes old data (AKA players party as they all die with player)
        MyEventManager.SaveLoad.OnDeleteBossSave?.Invoke();
        //Saves the bosses back however as some bosses may have been completed and that should get remembered
        MyEventManager.SaveLoad.OnSaveNewBosses?.Invoke(bosses);

        //Load new scene
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("HubWorld");

    }
    private void BossDiedHub()
    {
        //Change Views
        MyEventManager.ViewManager.BossViewManager.OnSetMapViewState?.Invoke(false);
        MyEventManager.ViewManager.BossViewManager.OnSetLoadViewState?.Invoke(true);

        //-- Saves Boss Save --
        MyEventManager.SaveLoad.OnDeleteBossSave?.Invoke();
        //Saves bosses
        MyEventManager.SaveLoad.OnSaveNewBosses?.Invoke(bosses);
        //Saves Party
        List<Monster> partyMonsters = new List<Monster>();
        partyMonsters = MyEventManager.Party.OnGetParty.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToBoss?.Invoke(partyMonsters);

        //-- Saves Player Save --
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();
        //Saves players held monsters
        List<Monster> monsters = new List<Monster>();
        monsters = MyEventManager.Party.OnGetHeldMonsters?.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToPlayer?.Invoke(monsters);
        //Saves players Inventory
        Dictionary<Item, int> inventory = new Dictionary<Item, int>();
        inventory = MyEventManager.Inventory.OnGetInventory?.Invoke();
        MyEventManager.SaveLoad.OnSaveItemsToPlayer?.Invoke(inventory);
        //Saves Players Attacks
        MyEventManager.SaveLoad.OnSetAttacks?.Invoke(player.GetComponent<PlayerManager>().attacks);

        //-- Other Saves --
        MyEventManager.SaveLoad.OnSetPlayerLevel?.Invoke(player.GetComponent<PlayerManager>().level);
        MyEventManager.SaveLoad.OnSetUmbraEssence?.Invoke((long)MyEventManager.Inventory.OnGetUmbraEssence?.Invoke());

        //Load new scene
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("HubWorld");
    }

    private void OnEnable()
    {
        //General
        MyEventManager.LevelManager.BossLevelManager.OnStartFight += StartFight;
        MyEventManager.LevelManager.BossLevelManager.OnNextFighter += NewFighter;
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer += StopAllCoroutines;
        MyEventManager.LevelManager.BossLevelManager.OnRetryFight += NewSetUp;
        MyEventManager.LevelManager.BossLevelManager.OnHubFromBossDied += BossDiedHub;
        MyEventManager.LevelManager.BossLevelManager.OnHubFromPlayerDied += PlayerDiedHub;

        //Player Options
        MyEventManager.LevelManager.FightPhase.OnPlayerChoseAbility += AbilityUsed;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedPotion += ItemUsed;

        //Boss Options
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackSingleTarget += MonsterAttackAbilitySolo;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll += MonsterAttackAbilityAll;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterNegateAllBoosts += MonsterBoostReset;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterCharm += MonsterCharm;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterSummon += MonsterSummon;
        MyEventManager.LevelManager.BossLevelManager.OnPotionSteal += StealPotion;
        MyEventManager.LevelManager.BossLevelManager.OnStealUmbraEssence += StealUmbraEssence;
        MyEventManager.LevelManager.BossLevelManager.OnUmbraEssenceAttack += UmbraAttack;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.BossLevelManager.OnStartFight -= StartFight;
        MyEventManager.LevelManager.BossLevelManager.OnNextFighter -= NewFighter;
        MyEventManager.LevelManager.BossLevelManager.OnStopTimer -= StopAllCoroutines;
        MyEventManager.LevelManager.BossLevelManager.OnRetryFight -= NewSetUp;
        MyEventManager.LevelManager.BossLevelManager.OnHubFromBossDied -= BossDiedHub;
        MyEventManager.LevelManager.BossLevelManager.OnHubFromPlayerDied -= PlayerDiedHub;

        MyEventManager.LevelManager.FightPhase.OnPlayerChoseAbility -= AbilityUsed;
        MyEventManager.LevelManager.FightPhase.OnPlayerUsedPotion -= ItemUsed;

        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackSingleTarget -= MonsterAttackAbilitySolo;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterAttackAll -= MonsterAttackAbilityAll;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterNegateAllBoosts -= MonsterBoostReset;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterCharm -= MonsterCharm;
        MyEventManager.LevelManager.BossLevelManager.OnMonsterSummon -= MonsterSummon;
        MyEventManager.LevelManager.BossLevelManager.OnPotionSteal -= StealPotion;
        MyEventManager.LevelManager.BossLevelManager.OnStealUmbraEssence -= StealUmbraEssence;
        MyEventManager.LevelManager.BossLevelManager.OnUmbraEssenceAttack -= UmbraAttack;
    }

}
