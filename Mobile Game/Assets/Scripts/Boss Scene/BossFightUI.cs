using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossFightUI : MonoBehaviour
{
    [Header("UI GameObjects")]
    [SerializeField] private GameObject fightUI;
    [SerializeField] private GameObject fightUpdatesUI;
    [SerializeField] private GameObject bossDiedUI;
    [SerializeField] private GameObject playerDiedUI;
    [SerializeField] private GameObject fightQueueUI;

    [Header("Queue")]
    [SerializeField] private GameObject queueContentPrefab;
    [SerializeField] private GameObject queueContainer;

    [Header("Menus")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject abilityMenu;
    [SerializeField] private GameObject itemMenu;
    [SerializeField] private GameObject backToMainMenuButton;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI fightersName;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject actionPointsBar;
    [SerializeField] private GameObject magicPointsBar;

    [Header("Abilities")]
    [SerializeField] private GameObject abilityButton;
    [SerializeField] private GameObject abilityContainer;

    [Header("Items")]
    [SerializeField] private GameObject itemButton;
    [SerializeField] private GameObject itemContainer;

    [Header("Timer")]
    [SerializeField] private GameObject timer;

    [Header("Fight Updates")]
    [SerializeField] private TextMeshProUGUI fightUpdates;

    [Header("Player Died UI")]
    [SerializeField] private GameObject retryButton;

    [Header("Monster Died UI")]
    [SerializeField] private TextMeshProUGUI bossHeader;
    [SerializeField] private TextMeshProUGUI bossLoot;
    [SerializeField] private ItemSO umbraVials;
    [SerializeField] private GameObject doubleButton;
    private long umbraEssence;
    [SerializeField] private Button continueButton;

    [Header("Colours")]
    [SerializeField] private Color white;
    [SerializeField] private Color grey;
    [SerializeField] private Color green;
    [SerializeField] private Color red;
    [SerializeField] private Color blue;
    [SerializeField] private Color black;

    private void UpdateQueue(List<GameObject> queue)
    {
        if(!fightQueueUI.activeSelf)
        {
            fightQueueUI.SetActive(true);
        }

        //Resets the queue
        foreach(Transform child in queueContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach(GameObject position in queue)
        {
            GameObject newPosition = Instantiate(queueContentPrefab, transform.position, Quaternion.identity, queueContainer.transform);
            if(position.GetComponent<PlayerManager>() != null)
            {
                newPosition.GetComponent<Image>().sprite = position.GetComponent<PlayerManager>().thisSprite.sprite;
            }
            else
            {
                newPosition.GetComponent<Image>().sprite = position.GetComponent<SpriteRenderer>().sprite;
            }            
        }
    }

    private void NewCurrentFighter(GameObject newFighter)
    {
        fightUI.SetActive(true);
        fightUpdatesUI.SetActive(false);
        mainMenu.SetActive(true);
        abilityMenu.SetActive(false);
        itemMenu.SetActive(false);
        backToMainMenuButton.SetActive(false);

        if(newFighter.CompareTag("Player"))
        {
            PlayerManager playerManager = newFighter.GetComponent<PlayerManager>();
            fightersName.text = $"{playerManager.playerName}";

            float currentHealth = (float)playerManager.currentHealth / (float)playerManager.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
            float currentActionPoints = (float)playerManager.currentActionPoints / (float)playerManager.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
            float currentMagicPoints = (float)playerManager.currentMagicPoints / (float)playerManager.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
            SetUpAbilities(playerManager);
        }
        else
        {
            Monster currentMonster = newFighter.GetComponent<FriendlyMonsterManager>().monster;
            fightersName.text = $"{currentMonster.shortName}";

            float currentHealth = (float)currentMonster.currentHealth / (float)currentMonster.maxHealth;
            healthBar.GetComponent<Image>().fillAmount = currentHealth;
            float currentActionPoints = (float)currentMonster.currentActionPoints / (float)currentMonster.maxActionPoints;
            actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
            float currentMagicPoints = (float)currentMonster.currentMagicPoints / (float)currentMonster.maxMagicPoints;
            magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
            SetUpAbilities(currentMonster);
        }
        SetUpInventoryItems();
    }
    private void SetUpAbilities(PlayerManager currentFighter)
    {
        foreach (Transform child in abilityContainer.transform)
        {
            Destroy(child.gameObject);
        }
        //Adds all available abilites to menu
        if (currentFighter.attacks.Count > 0)
        {
            foreach (Ability ability in currentFighter.attacks)
            {
                if (ability is Attack)
                {
                    GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, abilityContainer.transform);
                    newAttackButton.GetComponent<AttackButton>().SetUp((Attack)ability);
                    Attack attack = (Attack)ability;
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
    private void SetUpAbilities(Monster currentFighter)
    {
        foreach (Transform child in abilityContainer.transform)
        {
            Destroy(child.gameObject);
        }
        //Adds all available abilites to menu
        if (currentFighter.currentAttacks.Count > 0)
        {
            foreach (Ability ability in currentFighter.currentAttacks)
            {
                if (ability is Attack)
                {
                    GameObject newAttackButton = Instantiate(abilityButton, transform.position, Quaternion.identity, abilityContainer.transform);
                    newAttackButton.GetComponent<AttackButton>().SetUp((Attack)ability); 
                    Attack attack = (Attack)ability;
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
            switch (item.Key.itemType)
            {
                case ItemType.Default:
#if UNITY_EDITOR
                    Debug.Log("Default item");
#endif
                    break;
                case ItemType.Potion:
                    GameObject newPotionButton = Instantiate(itemButton, transform.position, Quaternion.identity, itemContainer.transform);
                    newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
                    break;
            }
        }
    }

    private void UpdateTimer(float newTime)
    {
        timer.GetComponent<Image>().fillAmount = newTime;
    }

    private void ToggleFightUI(bool state)
    {
        fightUI.SetActive(state);
    }
    private void FightUpdates(string _fightUpdates, string textColour)
    {
        fightUpdates.text = _fightUpdates;
        if (textColour == "White")
        {
            fightUpdates.color = white;
        }
        else if(textColour == "Red")
        {
            fightUpdates.color = red;
        }
        else if(textColour == "Blue")
        {
            fightUpdates.color = blue;
        }

        fightUI.SetActive(false);
        fightUpdatesUI.SetActive(true);
    }
    public void CloseUpdates()
    {
        fightUpdates.text = "";
        fightUpdatesUI.SetActive(false);
        MyEventManager.LevelManager.BossLevelManager.OnNextFighter?.Invoke();
    }

    private void ToggleMonsterDiedUI(Monster boss)
    {
        fightUI.SetActive(false);
        fightUpdatesUI.SetActive(false);
        fightQueueUI.SetActive(false);
        playerDiedUI.SetActive(false);
        umbraEssence = boss.maxHealth * 2;
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(umbraEssence);
        Item vial = new Item(umbraVials);
        MyEventManager.Inventory.OnAddToInventory?.Invoke(vial, 5);
        bossHeader.text = $"{boss.shortName} Defeated";
        bossLoot.text = $"Umbra Vials Collected: 5\n\nUmbra Essence Collected: {umbraEssence}";
        bool temp = false;
        temp = (bool)MyEventManager.AdManager.OnGetIsAdLoaded?.Invoke();
        doubleButton.SetActive(temp);
        bossDiedUI.SetActive(true);
    }
    public void OnDoubleEssence()
    {
        doubleButton.SetActive(false);
        MyEventManager.AdManager.OnShowRewardedAd?.Invoke(Double);
    }
    private void Double()
    {
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(umbraEssence);
        bossLoot.text = $"Umbra Vials Collected: 5\n\nUmbra Essence Collected: {umbraEssence * 2}";
    }
    public void OnContinueMonsterDied()
    {
        bossDiedUI.SetActive(true);
        playerDiedUI.SetActive(false);
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI?.Invoke(false);
        MyEventManager.LevelManager.BossLevelManager.OnHubFromBossDied?.Invoke();
    }


    private void TogglePlayerDiedUI(bool canRetry)
    {
        fightUI.SetActive(false);
        fightUpdatesUI.SetActive(false);
        fightQueueUI.SetActive(false);
        bossDiedUI.SetActive(false);
        playerDiedUI.SetActive(true);
        if (canRetry)
        {
            bool temp = false;
            temp = (bool)MyEventManager.AdManager.OnGetIsAdLoaded?.Invoke();
            retryButton.SetActive(temp);
        }
        else
        {
            retryButton.SetActive(false);
        }
    }
    public void OnContinuePlayerDied()
    {
        playerDiedUI.SetActive(false);
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI?.Invoke(false);
        MyEventManager.LevelManager.BossLevelManager.OnHubFromPlayerDied?.Invoke();
    }
    public void OnRetry()
    {
        playerDiedUI.SetActive(false);
        MyEventManager.AdManager.OnShowRewardedAd?.Invoke(Retry);
    }
    private void Retry()
    {
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI?.Invoke(false);
        MyEventManager.LevelManager.BossLevelManager.OnRetryFight?.Invoke();
    }

    private void OnEnable()
    {
        MyEventManager.UI.BossUI.OnUpdateQueue += UpdateQueue;
        MyEventManager.UI.BossUI.OnSetUpNewFighter += NewCurrentFighter;
        MyEventManager.UI.BossUI.OnUpdateTimer += UpdateTimer;
        MyEventManager.UI.BossUI.OnToggleFightUI += ToggleFightUI;
        MyEventManager.UI.BossUI.OnUpdateFightUI += FightUpdates;
        MyEventManager.UI.BossUI.OnToggleBossDiedUI += ToggleMonsterDiedUI;
        MyEventManager.UI.BossUI.OnTogglePlayerDiedUI += TogglePlayerDiedUI;
    }

    private void OnDisable()
    {
        MyEventManager.UI.BossUI.OnUpdateQueue -= UpdateQueue;
        MyEventManager.UI.BossUI.OnSetUpNewFighter -= NewCurrentFighter;
        MyEventManager.UI.BossUI.OnUpdateTimer -= UpdateTimer;
        MyEventManager.UI.BossUI.OnToggleFightUI -= ToggleFightUI;
        MyEventManager.UI.BossUI.OnUpdateFightUI -= FightUpdates;
        MyEventManager.UI.BossUI.OnToggleBossDiedUI -= ToggleMonsterDiedUI;
        MyEventManager.UI.BossUI.OnTogglePlayerDiedUI -= TogglePlayerDiedUI;
    }
}
