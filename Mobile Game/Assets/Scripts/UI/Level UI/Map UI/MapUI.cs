using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapUI : MonoBehaviour
{
    private GameObject player;

    [Header("Death UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject extraLifeButton;
    private bool hasExtraLife = true;

    [Header("Player Settings base UI")]
    [SerializeField] private GameObject playerSettingsUI;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private GameObject partyMenu;
    [SerializeField] private GameObject monsterBagMenu;
    [SerializeField] private GameObject attacksMenu;
    [SerializeField] private GameObject powerUpsMenu;
    [SerializeField] private GameObject closeButton;
    [SerializeField] private GameObject mainMenuButton;

    [Header("Colours")]
    [SerializeField] private Color white;
    [SerializeField] private Color grey;
    [SerializeField] private Color green;
    [SerializeField] private Color red;
    [SerializeField] private Color blue;
    [SerializeField] private Color black;

    [Header("Player Settings - Monster UI")]
    [SerializeField] private GameObject swapMonsterButton;
    [SerializeField] private GameObject removeMonsterButton;
    [SerializeField] private GameObject partyMonsterButtonPrefab;
    [SerializeField] private GameObject bagMonsterButtonPrefab;
    [SerializeField] private GameObject playerButtonPrefab;
    [SerializeField] private GameObject freeSpaceButtonPrefab;
    private Monster currentlyViewing;

    [Header("Player Settings - Stats")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject actionPointsBar;
    [SerializeField] private GameObject magicPointsBar;
    [SerializeField] private TextMeshProUGUI currentlyViewingInfoText;

    [Header("Player Settings - Items UI")]
    [SerializeField] private GameObject itemContainer;
    [SerializeField] private GameObject itemButtonPrefab;

    [Header("Player Settings - Party UI")]
    [SerializeField] private GameObject partyContainer;

    [Header("Player Settings - Held Monsters UI")]
    [SerializeField] private GameObject heldMonstersContainer;
    public bool swapMonsterClicked;

    [Header("Player Settings - Ability UI")]
    [SerializeField] private GameObject abilityButton;
    [SerializeField] private GameObject inventoryAbilityButton;
    [SerializeField] private GameObject freeAttackSpaceButton;
    [SerializeField] private GameObject freePowerUpSpaceButton;
    [SerializeField] private GameObject attackContainer;
    [SerializeField] private GameObject powerUpContainer;
    private Ability tempAbility = null;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    //Death UI
    public void OnExtraLife()
    {
        //Reset Player
        MyEventManager.AdManager.OnShowRewardedAd?.Invoke(RewardedAdWatched);
    }
    private void ToggleDeathUI(bool newState)
    {
        deathUI.SetActive(newState);
        if (hasExtraLife)
        {
            bool temp = false;
            temp = (bool)MyEventManager.AdManager.OnGetIsAdLoaded?.Invoke();
            extraLifeButton.SetActive(temp);
        }
        else
        {
            extraLifeButton.SetActive(false);
        }
    }
    public void OnContinue()
    {
        MyEventManager.LevelManager.OnGoToHubScene?.Invoke();
    }
    public void RewardedAdWatched()
    {
        hasExtraLife = false;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerManager>().currentHealth = player.GetComponent<PlayerManager>().maxHealth;
        player.GetComponent<PlayerManager>().currentActionPoints = player.GetComponent<PlayerManager>().maxActionPoints;
        player.GetComponent<PlayerManager>().currentMagicPoints = player.GetComponent<PlayerManager>().maxMagicPoints;
        player.GetComponent<PlayerControls>().StopFighting();
        ToggleDeathUI(false);
    }

    //Player Settings UI
    private void OpenPlayerSettings()
    {
        playerSettingsUI.SetActive(true);
        currentlyViewing = null;
        swapMonsterClicked = false;
        SetUpStats();
        SetUpInventoryItems();
        SetUpPartyMenu();
        SetUpHeldMonsters();
        SetUpAbilites();
        SetUpInfo();
    }
    public void ClosePlayerSettings()
    {
        mainMenu.SetActive(true);
        closeButton.SetActive(true);
        swapMonsterButton.SetActive(false);
        removeMonsterButton.SetActive(false);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(false);
        attacksMenu.SetActive(false);
        powerUpsMenu.SetActive(false);
        mainMenuButton.SetActive(false);
        playerSettingsUI.SetActive(false);
        if (currentlyViewing != null)
        {
            MyEventManager.Party.OnAddToParty?.Invoke(currentlyViewing);
        }
    }
    public void OnBackToMainMenu()
    {
        tempAbility = null;
        SetUpAbilites();
    }

    private void SetUpStats()
    {
        if (currentlyViewing == null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            nameText.text = $"{playerManager.playerName}";
            float currentHealth = (float)playerManager.currentHealth / (float)playerManager.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
            float currentActionPoints = (float)playerManager.currentActionPoints / (float)playerManager.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
            float currentMagicPoints = (float)playerManager.currentMagicPoints / (float)playerManager.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
        }
        else
        {
            nameText.text = $"{currentlyViewing.shortName}";
            float currentHealth = (float)currentlyViewing.currentHealth / (float)currentlyViewing.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
            float currentActionPoints = (float)currentlyViewing.currentActionPoints / (float)currentlyViewing.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
            float currentMagicPoints = (float)currentlyViewing.currentMagicPoints / (float)currentlyViewing.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
        }
    }
    private void SetUpInventoryItems()
    {
        //Destorys all old buttons
        foreach (Transform child in itemContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        Dictionary<Item, int> myInv = new Dictionary<Item, int>();
        myInv = MyEventManager.Inventory.OnGetInventory?.Invoke();

        //Creates buttons for items that can be used during fight
        foreach (KeyValuePair<Item, int> item in myInv)
        {
            if (item.Key.itemType == ItemType.Potion)
            {
                if (item.Key.potionType != PotionType.AttackBoost && item.Key.potionType != PotionType.MagicBoost)
                {
                    GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, itemContainer.transform);
                    newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
                }
            }
            else if (item.Key.itemType == ItemType.MonsterRemains)
            {
                GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, itemContainer.transform);
                newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
            }
        }
    }
    private void SetUpPartyMenu()
    {
        //Destorys all old buttons
        foreach (Transform child in partyContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        List<Monster> myParty = new List<Monster>();
        myParty = MyEventManager.Party.OnGetParty?.Invoke();

        int freeSpacesCount = 4 - myParty.Count;

        if (currentlyViewing != null)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, transform.position, Quaternion.identity, partyContainer.transform);
            newPlayerButton.GetComponent<PartyPlayerButton>().SetUp(player);
            freeSpacesCount--;
        }

        //Creates buttons for items that can be used during fight
        foreach (Monster monster in myParty)
        {
            GameObject newMonsterButton = Instantiate(partyMonsterButtonPrefab, transform.position, Quaternion.identity, partyContainer.transform);
            newMonsterButton.GetComponent<PartyMonsterButton>().SetUpSlot(monster);
        }

        for (int i = 0; i < freeSpacesCount; i++)
        {
            GameObject freeSpaceButton = Instantiate(freeSpaceButtonPrefab, transform.position, Quaternion.identity, partyContainer.transform);
        }
    }
    private void SetUpHeldMonsters()
    {
        //Destorys all old buttons
        foreach (Transform child in heldMonstersContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players held monsters
        List<Monster> myParty = new List<Monster>();
        myParty = MyEventManager.Party.OnGetHeldMonsters?.Invoke();

        //Creates buttons for the monsters in the players held monsters inventory
        foreach (Monster monster in myParty)
        {
            GameObject newMonsterButton = Instantiate(bagMonsterButtonPrefab, transform.position, Quaternion.identity, heldMonstersContainer.transform);
            newMonsterButton.GetComponent<HeldMonsterButton>().SetUp(monster);
        }
    }
    private void SetUpAbilites()
    {
        //Destorys all old buttons
        foreach (Transform child in attackContainer.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in powerUpContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players/monsters attacks
        if (currentlyViewing == null)
        {
            //Sets Up Attacks
            int freeSpacesCount = 4 - player.GetComponent<PlayerManager>().attacks.Count;
            foreach (Attack attack in player.GetComponent<PlayerManager>().attacks)
            {
                GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                newAttackButton.GetComponent<AttackButton>().SetUp(attack); switch (attack.damageType)
                {
                    case DamageType.Normal:
                        newAttackButton.GetComponent<Image>().color = white;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Air:
                        newAttackButton.GetComponent<Image>().color = grey;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Earth:
                        newAttackButton.GetComponent<Image>().color = green;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Fire:
                        newAttackButton.GetComponent<Image>().color = red;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Umbra:
                        newAttackButton.GetComponent<Image>().color = black;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = white;
                        break;
                    case DamageType.Water:
                        newAttackButton.GetComponent<Image>().color = blue;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                }
            }
            for (int i = 0; i < freeSpacesCount; i++)
            {
                GameObject freeSpaceButton = Instantiate(freeAttackSpaceButton, transform.position, Quaternion.identity, attackContainer.transform);
            }

            //Power up not been added and not sure that they will be now..
            freeSpacesCount = 4 - player.GetComponent<PlayerManager>().powerUps.Count;
            foreach (PowerUp powerUp in player.GetComponent<PlayerManager>().powerUps)
            {
                GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, powerUpContainer.transform);
                newAttackButton.GetComponent<AttackButton>().SetUp(powerUp);
            }
            for (int i = 0; i < freeSpacesCount; i++)
            {
                GameObject freeSpaceButton = Instantiate(freePowerUpSpaceButton, transform.position, Quaternion.identity, powerUpContainer.transform);
            }
        }
        else
        {
            int freeSpacesCount = 4 - currentlyViewing.currentAttacks.Count;
            foreach (Attack attack in currentlyViewing.currentAttacks)
            {
                GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                newAttackButton.GetComponent<AttackButton>().SetUp(attack); switch (attack.damageType)
                {
                    case DamageType.Normal:
                        newAttackButton.GetComponent<Image>().color = white;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Air:
                        newAttackButton.GetComponent<Image>().color = grey;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Earth:
                        newAttackButton.GetComponent<Image>().color = green;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Fire:
                        newAttackButton.GetComponent<Image>().color = red;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                    case DamageType.Umbra:
                        newAttackButton.GetComponent<Image>().color = black;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = white;
                        break;
                    case DamageType.Water:
                        newAttackButton.GetComponent<Image>().color = blue;
                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                        break;
                }
            }
            for (int i = 0; i < freeSpacesCount; i++)
            {
                GameObject freeSpaceButton = Instantiate(freeAttackSpaceButton, transform.position, Quaternion.identity, attackContainer.transform);
            }

            //Power up not been added and not sure that they will be now..
            freeSpacesCount = 4 - currentlyViewing.currentPowerUps.Count;
            foreach (PowerUp powerUp in currentlyViewing.currentPowerUps)
            {
                GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, powerUpContainer.transform);
                newAttackButton.GetComponent<AttackButton>().SetUp(powerUp);
            }
            for (int i = 0; i < freeSpacesCount; i++)
            {
                GameObject freeSpaceButton = Instantiate(freePowerUpSpaceButton, transform.position, Quaternion.identity, powerUpContainer.transform);
            }
        }
    }
    private void SetUpInfo()
    {
        if (currentlyViewing == null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            currentlyViewingInfoText.text = $"{playerManager.playerName}'s Info\n\n" +
                $"Health: {playerManager.currentHealth}/{playerManager.maxHealth}\nStamina: {playerManager.currentActionPoints}/{playerManager.maxActionPoints}\nMana: {playerManager.currentMagicPoints}/{playerManager.maxMagicPoints}\n\n" +
                $"Attack Speed: {playerManager.currentAttackSpeed}\nAttack Power: {playerManager.currentAttackPower}\n\n" +
                $"Weaknesses:\n";
            foreach (Affinity affinity in playerManager.weakness)
            {
                currentlyViewingInfoText.text += affinity.damageType.ToString();
                if (affinity != playerManager.weakness[playerManager.weakness.Count - 1])
                {
                    currentlyViewingInfoText.text += ", ";
                }
                else
                {
                    currentlyViewingInfoText.text += ".\n\n";
                }
            }
            currentlyViewingInfoText.text += "Immunities:\n";
            foreach (Affinity affinity in playerManager.immunities)
            {
                currentlyViewingInfoText.text += affinity.damageType.ToString();
                if (affinity != playerManager.immunities[playerManager.immunities.Count - 1])
                {
                    currentlyViewingInfoText.text += ", ";
                }
                else
                {
                    currentlyViewingInfoText.text += ".\n\n";
                }
            }
        }
        else
        {
            currentlyViewingInfoText.text = $"{currentlyViewing.shortName}'s Info\n\n" +
                $"Level: {currentlyViewing.level}\n" +
                $"Build: {currentlyViewing.monsterBuild}\n\n" +
                $"Health: {currentlyViewing.currentHealth}/{currentlyViewing.maxHealth}\nStamina: {currentlyViewing.currentActionPoints}/{currentlyViewing.maxActionPoints}\nMana: {currentlyViewing.currentMagicPoints}/{currentlyViewing.maxMagicPoints}\n\n" +
                $"Attack Speed: {currentlyViewing.currentAttackSpeed}\nAttack Power: {currentlyViewing.currentAttackPower}\n\n" +
                $"Weaknesses:\n";
            foreach (Affinity affinity in currentlyViewing.weakness)
            {
                currentlyViewingInfoText.text += affinity.damageType.ToString();
                if (affinity != currentlyViewing.weakness[currentlyViewing.weakness.Count - 1])
                {
                    currentlyViewingInfoText.text += ", ";
                }
                else
                {
                    currentlyViewingInfoText.text += ".\n\n";
                }
            }
            currentlyViewingInfoText.text += "Immunities:\n";
            foreach (Affinity affinity in currentlyViewing.immunities)
            {
                currentlyViewingInfoText.text += affinity.damageType.ToString();
                if (affinity != currentlyViewing.immunities[currentlyViewing.immunities.Count - 1])
                {
                    currentlyViewingInfoText.text += ", ";
                }
                else
                {
                    currentlyViewingInfoText.text += ".\n\n";
                }
            }
        }
    }

    private void UpdateHealth(int points)
    {
        if (currentlyViewing == null)
        {
            MyEventManager.Player.OnHeal?.Invoke(points);
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            float currentHealth = (float)playerManager.currentHealth / (float)playerManager.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
        }
        else
        {
            currentlyViewing.Heal(points);
            float currentHealth = (float)currentlyViewing.currentHealth / (float)currentlyViewing.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
        }
        SetUpInfo();
    }
    private void UpdateActionPoints(int points)
    {
        if (currentlyViewing == null)
        {
            MyEventManager.Player.OnAPIncrease?.Invoke(points);
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            float currentActionPoints = (float)playerManager.currentActionPoints / (float)playerManager.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
        }
        else
        {
            currentlyViewing.RestoreAP(points);
            float currentActionPoints = (float)currentlyViewing.currentActionPoints / (float)currentlyViewing.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
        }
        SetUpInfo();
    }
    private void UpdateMagicPoints(int points)
    {
        if (currentlyViewing == null)
        {
            MyEventManager.Player.OnMPIncrease?.Invoke(points);
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            float currentMagicPoints = (float)playerManager.currentMagicPoints / (float)playerManager.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
        }
        else
        {
            currentlyViewing.RestoreMP(points);
            float currentMagicPoints = (float)currentlyViewing.currentMagicPoints / (float)currentlyViewing.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
        }
        SetUpInfo();
    }
    private void LevelUp(Item monsterRemains)
    {
        //Old levelling system... Player no longer levels as explained in player manager
        /*if (currentlyViewing == null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            playerManager.LevelUp();
            nameText.text = $"{playerManager.playerName} Lvl: {playerManager.level}";
            float currentHealth = (float)playerManager.currentHealth / (float)playerManager.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
            float currentActionPoints = (float)playerManager.currentActionPoints / (float)playerManager.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
            float currentMagicPoints = (float)playerManager.currentMagicPoints / (float)playerManager.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
            MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(monsterRemains, 1);
            SetUpInventoryItems();
        }
        else
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            if (currentlyViewing.level < playerManager.level)
            {
                currentlyViewing.LevelUp();
                float currentHealth = (float)currentlyViewing.currentHealth / (float)currentlyViewing.maxHealth;
                healthBar.GetComponent<Image>().fillAmount = currentHealth;
                float currentActionPoints = (float)currentlyViewing.currentActionPoints / (float)currentlyViewing.maxActionPoints;
                actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
                float currentMagicPoints = (float)currentlyViewing.currentMagicPoints / (float)currentlyViewing.maxMagicPoints;
                magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
                MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(monsterRemains, 1);
                SetUpInventoryItems();
            }
        }*/

        if (currentlyViewing != null)
        {
            if (currentlyViewing.level < 3)
            {
                currentlyViewing.LevelUp();
                float currentHealth = (float)currentlyViewing.currentHealth / (float)currentlyViewing.maxHealth;
                healthBar.GetComponent<Image>().fillAmount = currentHealth;
                float currentActionPoints = (float)currentlyViewing.currentActionPoints / (float)currentlyViewing.maxActionPoints;
                actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
                float currentMagicPoints = (float)currentlyViewing.currentMagicPoints / (float)currentlyViewing.maxMagicPoints;
                magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
                MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(monsterRemains, 1);
                SetUpInventoryItems();
            }
        }
        SetUpInfo();
    }
    private void SwitchToMonster(Monster newMonster)
    {
        //Switches Back to Main Menu
        mainMenu.SetActive(true);
        closeButton.SetActive(true);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(false);
        mainMenuButton.SetActive(false);

        //Allows Buttons only for monsters
        swapMonsterButton.SetActive(true);
        removeMonsterButton.SetActive(true);

        //Puts current Monster back into party
        if (currentlyViewing != null)
        {
            MyEventManager.Party.OnAddToParty?.Invoke(currentlyViewing);
        }

        //Switches currently viewing monster to the new one
        currentlyViewing = newMonster;

        //Removes Monster From Party
        MyEventManager.Party.OnRemoveFromParty?.Invoke(currentlyViewing);

        //Resets all UI
        SetUpStats();
        SetUpInventoryItems();
        SetUpPartyMenu();
        SetUpHeldMonsters();
        SetUpAbilites();
        SetUpInfo();
    }
    private void SwitchToPlayer()
    {
        //Switches Back to Main Menu
        mainMenu.SetActive(true);
        closeButton.SetActive(true);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(false);
        mainMenuButton.SetActive(false);

        //Deactivate Buttons only for monsters
        swapMonsterButton.SetActive(false);
        removeMonsterButton.SetActive(false);

        //Puts current Monster back into party
        if (currentlyViewing != null)
        {
            MyEventManager.Party.OnAddToParty?.Invoke(currentlyViewing);
        }

        //gets rid of currently viewing
        currentlyViewing = null;

        //Resets all UI
        SetUpStats();
        SetUpInventoryItems();
        SetUpPartyMenu();
        SetUpHeldMonsters();
        SetUpAbilites();
        SetUpInfo();
    }
    private void HeldMonsterHandle(Monster newMonster)
    {
        //Switches Back to Main Menu
        mainMenu.SetActive(true);
        closeButton.SetActive(true);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(false);
        mainMenuButton.SetActive(false);

        if (!swapMonsterClicked)
        {
            MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(newMonster);
            MyEventManager.Party.OnAddToParty?.Invoke(newMonster);
        }
        else
        {
            MyEventManager.Party.OnAddToHeldMonsters?.Invoke(currentlyViewing);
            MyEventManager.Party.OnRemoveFromHeldMonsters?.Invoke(newMonster);
            currentlyViewing = newMonster;
            swapMonsterClicked = false;
        }

        //Resets all UI
        SetUpStats();
        SetUpInventoryItems();
        SetUpPartyMenu();
        SetUpHeldMonsters();
    }
    private void AddMonsterClicked()
    {
        mainMenu.SetActive(false);
        closeButton.SetActive(false);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(true);
        mainMenuButton.SetActive(true);
    }
    public void RemoveMonsterClick()
    {
        //Switches Back to Main Menu
        mainMenu.SetActive(true);
        closeButton.SetActive(true);
        inventoryMenu.SetActive(false);
        partyMenu.SetActive(false);
        monsterBagMenu.SetActive(false);
        mainMenuButton.SetActive(false);
        swapMonsterButton.SetActive(false);
        removeMonsterButton.SetActive(false);

        MyEventManager.Party.OnAddToHeldMonsters?.Invoke(currentlyViewing);
        currentlyViewing = null;

        //Resets all UI
        SetUpStats();
        SetUpInventoryItems();
        SetUpPartyMenu();
        SetUpHeldMonsters();
    }

    public void ToggleSwapMonstersClicked(bool state)
    {
        swapMonsterClicked = state;
    }

    private void OpenAddAttackUI()
    {
        foreach (Transform child in attackContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        Dictionary<Item, int> myInv = new Dictionary<Item, int>();
        myInv = MyEventManager.Inventory.OnGetInventory?.Invoke();

        //Creates buttons 
        foreach (KeyValuePair<Item, int> item in myInv)
        {
            if (item.Key.itemType == ItemType.TrainingScripts)
            {
                if (item.Key.attack != null)
                {
                    if (currentlyViewing == null)
                    {
                        foreach (AttackSO attack in player.GetComponent<PlayerManager>().allowedAttackSOs)
                        {
                            if (item.Key.attack.abilityName == attack.abilityName)
                            {
                                GameObject newAttackButton = Instantiate(inventoryAbilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                                newAttackButton.GetComponent<AttackButton>().SetUp((Attack)item.Key.attack, item.Key);
                                switch (attack.damageType)
                                {
                                    case DamageType.Normal:
                                        newAttackButton.GetComponent<Image>().color = white;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Air:
                                        newAttackButton.GetComponent<Image>().color = grey;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Earth:
                                        newAttackButton.GetComponent<Image>().color = green;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Fire:
                                        newAttackButton.GetComponent<Image>().color = red;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Umbra:
                                        newAttackButton.GetComponent<Image>().color = black;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = white;
                                        break;
                                    case DamageType.Water:
                                        newAttackButton.GetComponent<Image>().color = blue;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (Attack attack in currentlyViewing.allowedAttacks)
                        {
                            if (item.Key.attack.abilityName == attack.abilityName)
                            {
                                GameObject newAttackButton = Instantiate(inventoryAbilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                                newAttackButton.GetComponent<AttackButton>().SetUp((Attack)item.Key.attack, item.Key);
                                switch (attack.damageType)
                                {
                                    case DamageType.Normal:
                                        newAttackButton.GetComponent<Image>().color = white;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Air:
                                        newAttackButton.GetComponent<Image>().color = grey;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Earth:
                                        newAttackButton.GetComponent<Image>().color = green;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Fire:
                                        newAttackButton.GetComponent<Image>().color = red;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                    case DamageType.Umbra:
                                        newAttackButton.GetComponent<Image>().color = black;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = white;
                                        break;
                                    case DamageType.Water:
                                        newAttackButton.GetComponent<Image>().color = blue;
                                        newAttackButton.GetComponentInChildren<TextMeshProUGUI>().color = black;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    private void OpenAddPowerUpUI()
    {
        foreach (Transform child in powerUpContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Gets the players inventory
        Dictionary<Item, int> myInv = new Dictionary<Item, int>();
        myInv = MyEventManager.Inventory.OnGetInventory?.Invoke();

        //Creates buttons for items that can be used during fight
        foreach (KeyValuePair<Item, int> item in myInv)
        {
            if (item.Key.itemType == ItemType.TrainingScripts)
            {
                if (item.Key.powerUp != null)
                {
                    if (currentlyViewing == null)
                    {
                        foreach (PowerUpSO powerUp in player.GetComponent<PlayerManager>().allowedPowerUpSOs)
                        {
                            if (item.Key.powerUp.abilityName == powerUp.abilityName)
                            {
                                GameObject newAttackButton = Instantiate(inventoryAbilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                                newAttackButton.GetComponent<AttackButton>().SetUp((PowerUp)item.Key.powerUp, item.Key);
                            }
                        }
                    }
                    else
                    {
                        foreach (PowerUp powerUp in currentlyViewing.allowedPowerUps)
                        {
                            if (item.Key.powerUp.abilityName == powerUp.abilityName)
                            {
                                GameObject newAttackButton = Instantiate(inventoryAbilityButton, transform.position, Quaternion.identity, attackContainer.transform);
                                newAttackButton.GetComponent<AttackButton>().SetUp((PowerUp)item.Key.powerUp, item.Key);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OpenSwapAbilityUI(Ability abilityToSwapOut)
    {
        tempAbility = abilityToSwapOut;
        if (abilityToSwapOut is Attack)
        {
            OpenAddAttackUI();
        }
        else if (abilityToSwapOut is PowerUp)
        {
            OpenAddPowerUpUI();
        }
    }
    private void AddNewAbility(Ability newAbility, Item tempItem)
    {
        if (tempAbility == null)
        {
            if (newAbility is Attack)
            {
                if (currentlyViewing == null)
                {
                    player.GetComponent<PlayerManager>().attacks.Add((Attack)newAbility);
                }
                else
                {
                    currentlyViewing.currentAttacks.Add((Attack)newAbility);
                }
            }
            else if (newAbility is PowerUp)
            {
                if (currentlyViewing == null)
                {
                    player.GetComponent<PlayerManager>().powerUps.Add((PowerUp)newAbility);
                }
                else
                {
                    currentlyViewing.currentPowerUps.Add((PowerUp)newAbility);
                }
            }
        }
        else
        {
            if (newAbility is Attack)
            {
                if (currentlyViewing == null)
                {
                    if (player.GetComponent<PlayerManager>().attacks.Contains((Attack)tempAbility))
                    {
                        player.GetComponent<PlayerManager>().attacks.Remove((Attack)tempAbility);
                        player.GetComponent<PlayerManager>().attacks.Add((Attack)newAbility);
                    }
                }
                else
                {
                    if (currentlyViewing.currentAttacks.Contains((Attack)tempAbility))
                    {
                        currentlyViewing.currentAttacks.Remove((Attack)newAbility);
                        currentlyViewing.currentAttacks.Add((Attack)newAbility);
                    }
                }
            }
            else if (newAbility is PowerUp)
            {
                if (currentlyViewing == null)
                {
                    if (player.GetComponent<PlayerManager>().powerUps.Contains((PowerUp)tempAbility))
                    {
                        player.GetComponent<PlayerManager>().powerUps.Remove((PowerUp)tempAbility);
                        player.GetComponent<PlayerManager>().powerUps.Add((PowerUp)newAbility);
                    }
                }
                else
                {
                    if (currentlyViewing.currentPowerUps.Contains((PowerUp)tempAbility))
                    {
                        currentlyViewing.currentPowerUps.Remove((PowerUp)newAbility);
                        currentlyViewing.currentPowerUps.Add((PowerUp)newAbility);
                    }
                }
            }

        }
        MyEventManager.Inventory.OnRemoveFromInventory?.Invoke(tempItem, 1);
        tempAbility = null;
        SetUpAbilites();
    }

    private void OnEnable()
    {
        MyEventManager.UI.MapUI.OnToggleDeathUI += ToggleDeathUI;

        MyEventManager.UI.MapUI.OnPlayerSettings += OpenPlayerSettings;
        MyEventManager.UI.MapUI.OnClosePlayerSettings += ClosePlayerSettings;

        MyEventManager.UI.MapUI.OnUpdateHealth += UpdateHealth;
        MyEventManager.UI.MapUI.OnUpdateAP += UpdateActionPoints;
        MyEventManager.UI.MapUI.OnUpdateMP += UpdateMagicPoints;
        MyEventManager.UI.MapUI.OnLevelUp += LevelUp;

        MyEventManager.UI.MapUI.OnSwitchToMonster += SwitchToMonster;
        MyEventManager.UI.MapUI.OnSwitchToPlayer += SwitchToPlayer;
        MyEventManager.UI.MapUI.OnHeldMonsterInteraction += HeldMonsterHandle;
        MyEventManager.UI.MapUI.OnAddMonster += AddMonsterClicked;

        MyEventManager.UI.MapUI.OnAddAttack += OpenAddAttackUI;
        MyEventManager.UI.MapUI.OnAddPowerUp += OpenAddPowerUpUI;
        MyEventManager.UI.MapUI.OnOpenSwapAbilityUI += OpenSwapAbilityUI;
        MyEventManager.UI.MapUI.OnAddNewAbility += AddNewAbility;
    }

    private void OnDisable()
    {
        MyEventManager.UI.MapUI.OnToggleDeathUI -= ToggleDeathUI;

        MyEventManager.UI.MapUI.OnPlayerSettings -= OpenPlayerSettings;
        MyEventManager.UI.MapUI.OnClosePlayerSettings -= ClosePlayerSettings;

        MyEventManager.UI.MapUI.OnUpdateHealth -= UpdateHealth;
        MyEventManager.UI.MapUI.OnUpdateAP -= UpdateActionPoints;
        MyEventManager.UI.MapUI.OnUpdateMP -= UpdateMagicPoints;
        MyEventManager.UI.MapUI.OnLevelUp -= LevelUp;

        MyEventManager.UI.MapUI.OnSwitchToMonster -= SwitchToMonster;
        MyEventManager.UI.MapUI.OnSwitchToPlayer -= SwitchToPlayer;
        MyEventManager.UI.MapUI.OnHeldMonsterInteraction -= HeldMonsterHandle;
        MyEventManager.UI.MapUI.OnAddMonster -= AddMonsterClicked;

        MyEventManager.UI.MapUI.OnAddAttack -= OpenAddAttackUI;
        MyEventManager.UI.MapUI.OnAddPowerUp -= OpenAddPowerUpUI;
        MyEventManager.UI.MapUI.OnOpenSwapAbilityUI -= OpenSwapAbilityUI;
        MyEventManager.UI.MapUI.OnAddNewAbility -= AddNewAbility;
    }
}
