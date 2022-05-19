using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading;


public class FightUI : MonoBehaviour
{
    [SerializeField] private GameObject mainButtons;
    [SerializeField] private GameObject backToMainMenuButton;

    [Header("Visual UI")]
    [SerializeField] private TextMeshProUGUI fightersName;
    [SerializeField] private TextMeshProUGUI fightUpdates;

    [Header("Fight Data")]
    [SerializeField] private GameObject fightUI;
    [SerializeField] private GameObject abilityMenu;
    [SerializeField] private GameObject abilityContainer;
    [SerializeField] private GameObject attackButtonPrefab;
    [SerializeField] private GameObject powerUpButtonPrefab;

    [Header("Item Data")]
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject captureUI;
    [SerializeField] private CanvasGroup captureCanvasGroup;
    [SerializeField] private GameObject locketContainer;
    [SerializeField] private GameObject itemUI;
    [SerializeField] private CanvasGroup itemCanvasGroup;
    [SerializeField] private GameObject itemContainer;

    [Header("Other Options Data")]
    [SerializeField] private GameObject otherOptionsButtons;

    [Header("Monster Swap Data")]
    [SerializeField] private GameObject swapMonsterUI;
    [SerializeField] private CanvasGroup monsterCanvasGroup;
    [SerializeField] private GameObject monsterButtonPrefab;
    [SerializeField] private GameObject playerButtonPrefab;
    [SerializeField] private GameObject monsterContainer;

    [Header("Player Data")]
    private GameObject player;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject actionPointsBar;
    [SerializeField] private GameObject magicPointsBar;

    [Header("End Of Fight Data")]
    [SerializeField] private ItemSO umbraSap;
    [SerializeField] private GameObject monsterDiedUI;
    [SerializeField] private TextMeshProUGUI monsterDiedTitle;
    [SerializeField] private TextMeshProUGUI monsterReport;

    [SerializeField] private GameObject monsterCaughtUI;
    [SerializeField] private TextMeshProUGUI monsterCaughtTitle;
    [SerializeField] private TextMeshProUGUI monsterCaughtReport;
    [SerializeField] private GameObject monsterCaughtCloseButton;
    [SerializeField] private TextMeshProUGUI monsterName;
    [SerializeField] private AudioClip twinkle;
    private EnemyManager currentMonster;
    TouchScreenKeyboard keyboard = null;

    [SerializeField] private GameObject monsterDiedRewardedAdButton;
    [SerializeField] private GameObject monsterCaughtRewardedAdButton;
    private long umbraEssence = 0;
    private int dropAmount = 0;

    private bool isStillAlive = true;

    [Header("Colours")]
    [SerializeField] private Color white;
    [SerializeField] private Color grey;
    [SerializeField] private Color green;
    [SerializeField] private Color red;
    [SerializeField] private Color blue;
    [SerializeField] private Color black;


    public void Initialize(List<Attack> attacks, List<PowerUp> powerUps, PlayerManager currentPlayer)
    {
        mainButtons.SetActive(true);
        abilityMenu.SetActive(false);
        otherOptionsButtons.SetActive(false);
        captureUI.SetActive(false);
        itemUI.SetActive(false);
        swapMonsterUI.SetActive(false);
        backToMainMenuButton.GetComponent<Button>().interactable = true;
        backToMainMenuButton.SetActive(false);

        player = currentPlayer.gameObject;
        fightersName.text = $"{currentPlayer.playerName}";

        float currentHealth = (float)currentPlayer.currentHealth / (float)currentPlayer.maxHealth;
        healthBar.GetComponent<Image>().fillAmount = currentHealth;
        float currentActionPoints = (float)currentPlayer.currentActionPoints / (float)currentPlayer.maxActionPoints;
        actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
        float currentMagicPoints = (float)currentPlayer.currentMagicPoints / (float)currentPlayer.maxMagicPoints;
        magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;

        //Removes all previous attacks and power ups in menu incase it has old attacks the player no longer has
        foreach (Transform child in abilityContainer.transform)
        {
            Destroy(child.gameObject);
        }

        //Adds all available abilites to menu
        foreach (Ability ability in attacks)
        {
            if (ability is Attack)
            {
                GameObject newAttackButton = Instantiate(attackButtonPrefab, transform.position, Quaternion.identity, abilityContainer.transform);
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
        //Makes sure power ups are at the bottom of the list
        foreach (Ability ability in powerUps)
        {
            if (ability is PowerUp)
            {
                GameObject newAttackButton = Instantiate(powerUpButtonPrefab, transform.position, Quaternion.identity, abilityContainer.transform);
                newAttackButton.GetComponent<PowerUpButton>().SetUp((PowerUp)ability);
            }
        }

        SetUpInventoryItems();
        SetUpParty(true);
    }
    public void Initialize(List<Attack> attacks, List<PowerUp> powerUps, FriendlyMonsterManager currentFight)
    {
        mainButtons.SetActive(true);
        abilityMenu.SetActive(false);
        otherOptionsButtons.SetActive(false);
        captureUI.SetActive(false);
        itemUI.SetActive(false);
        swapMonsterUI.SetActive(false);
        backToMainMenuButton.GetComponent<Button>().interactable = true;
        backToMainMenuButton.SetActive(false);

        fightersName.text = $"{currentFight.monster.shortName}";

        float currentHealth = (float)currentFight.monster.currentHealth / (float)currentFight.monster.maxHealth;
        healthBar.GetComponent<Image>().fillAmount = currentHealth;
        float currentActionPoints = (float)currentFight.monster.currentActionPoints / (float)currentFight.monster.maxActionPoints;
        actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
        float currentMagicPoints = (float)currentFight.monster.currentMagicPoints / (float)currentFight.monster.maxMagicPoints;
        magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;

        //Removes all previous attacks and power ups in menu incase it has old attacks the player no longer has
        foreach (Transform child in abilityContainer.transform)
        {
            Destroy(child.gameObject);
        }
        //Adds all available abilites to menu
        if (attacks.Count > 0)
        {
            foreach (Ability ability in attacks)
            {
                if (ability is Attack)
                {
                    GameObject newAttackButton = Instantiate(attackButtonPrefab, transform.position, Quaternion.identity, abilityContainer.transform);
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
        //Makes sure power ups are at the bottom of the list
        if (powerUps.Count > 0)
        {
            foreach (Ability ability in powerUps)
            {
                if (ability is PowerUp)
                {
                    GameObject newAttackButton = Instantiate(powerUpButtonPrefab, transform.position, Quaternion.identity, abilityContainer.transform);
                    newAttackButton.GetComponent<PowerUpButton>().SetUp((PowerUp)ability);
                }
            }
        }

        SetUpInventoryItems();
        SetUpParty(false);
    }

    private void SetUpInventoryItems()
    {
        StartCoroutine(ISetUpInventoryItems());
    }

    private IEnumerator ISetUpInventoryItems()
    {
        //Destorys all old buttons
        foreach (Transform child in locketContainer.transform)
        {
            Destroy(child.gameObject);
        }
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
                case ItemType.Locket:
                    GameObject newLocketButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, locketContainer.transform);
                    newLocketButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(locketContainer.GetComponent<RectTransform>());
                    yield return new WaitForEndOfFrame();
                    break;
                case ItemType.Potion:
                    GameObject newPotionButton = Instantiate(itemButtonPrefab, transform.position, Quaternion.identity, itemContainer.transform);
                    newPotionButton.GetComponent<ItemSlot>().SetUpSlot(item.Key, item.Value);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(itemContainer.GetComponent<RectTransform>());
                    yield return new WaitForEndOfFrame();
                    break;
            }
        }

        mainButtons.SetActive(false);
        captureUI.SetActive(true);
        captureCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(0.5f);
        captureCanvasGroup.alpha = 1;
        captureUI.SetActive(false);

        itemUI.SetActive(true);
        itemCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(0.5f);
        itemUI.SetActive(false);
        itemCanvasGroup.alpha = 1;
        mainButtons.SetActive(true);
    }
    private void SetUpParty(bool isPlayerCurrentFighter)
    {
        StartCoroutine(ISetUpParty(isPlayerCurrentFighter));
    }
    private IEnumerator ISetUpParty(bool isCurrentFighter)
    {
        foreach (Transform child in monsterContainer.transform)
        {
            Destroy(child.gameObject);
        }

        List<Monster> myParty = new List<Monster>();
        myParty = MyEventManager.Party.OnGetParty?.Invoke();

        foreach (Monster monster in myParty)
        {
            GameObject newMonsterButton = Instantiate(monsterButtonPrefab, transform.position, Quaternion.identity, monsterContainer.transform);
            newMonsterButton.GetComponent<PartyMonsterButton>().SetUpSlot(monster);
        }

        if (!isCurrentFighter)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, transform.position, Quaternion.identity, monsterContainer.transform);
            newPlayerButton.GetComponent<PartyPlayerButton>().SetUp(player);
        }

        mainButtons.SetActive(false);
        swapMonsterUI.SetActive(true);
        monsterCanvasGroup.alpha = 0;
        yield return new WaitForSeconds(0.5f);
        monsterCanvasGroup.alpha = 1;
        swapMonsterUI.SetActive(false);
    }

    //Removed from game as it made the game too easy...
    /*public void OnRun()
    {
        int damageTaken = startHealth - endHealth;
        fightReport.text = "Player Ran From Fight";
        endOfFightUI.SetActive(true);
        fightUI.SetActive(false);
    }*/

    private void EnemyDied(EnemyManager enemyMonster)
    {
        string newText = $"Level {enemyMonster.monster.level} {enemyMonster.monster.shortName}";
        dropAmount = enemyMonster.monster.level - player.GetComponent<PlayerManager>().level;
        if (dropAmount > 0)
        {
            Item newItem = new Item(umbraSap);
            MyEventManager.Inventory.OnAddToInventory?.Invoke(newItem, dropAmount);
        }
        else
        {
            dropAmount = 0;
        }
        umbraEssence = enemyMonster.monster.maxHealth;
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(umbraEssence);
        monsterDiedTitle.text = $"{newText} Destroyed.";
        monsterReport.text = $"Umbra Sap Vials Collected: {dropAmount}.\n\nUmbra Essence Collected: {umbraEssence}.";
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(twinkle);

        bool temp = false;
        temp = (bool)MyEventManager.AdManager.OnGetIsAdLoaded?.Invoke();
        monsterDiedRewardedAdButton.SetActive(temp);

        monsterDiedUI.SetActive(true);
        fightUI.SetActive(false);
    }

    private void EnemyCaught(EnemyManager enemyMonster)
    {
        monsterName.text = "";
        currentMonster = enemyMonster;
        string newText = $"Level {enemyMonster.monster.level} {enemyMonster.monster.shortName}"; 
        dropAmount = enemyMonster.monster.level - player.GetComponent<PlayerManager>().level;
        if (dropAmount > 0)
        {
            Item newItem = new Item(umbraSap);
            MyEventManager.Inventory.OnAddToInventory?.Invoke(newItem, dropAmount);
        }
        else
        {
            dropAmount = 0;
        }
        umbraEssence = enemyMonster.monster.maxHealth;
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(umbraEssence);
        monsterCaughtTitle.text = $"{newText} Captured.";
        monsterCaughtReport.text = $"Umbra Sap Vials Collected: {dropAmount}.\n\nUmbra Essence Collected: {umbraEssence}.";
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(twinkle);

        bool temp = false;
        temp = (bool)MyEventManager.AdManager.OnGetIsAdLoaded?.Invoke();
        monsterCaughtRewardedAdButton.SetActive(temp);

        monsterCaughtCloseButton.SetActive(false);
        monsterCaughtUI.SetActive(true);
        fightUI.SetActive(false);
    }

    public void OnInputSelect(TextMeshProUGUI s)
    {
        if (keyboard == null)
        {
            keyboard = TouchScreenKeyboard.Open(s.text, TouchScreenKeyboardType.Default);
        }
        else
        {
            keyboard.active = true;
        }
    }

    public void OnInputDeselect(TextMeshProUGUI s)
    {
        if (keyboard != null)
        {
            keyboard.active = false;
        }

        if (s.text != "")
        {
            currentMonster.monster.shortName = s.text;
            monsterCaughtCloseButton.SetActive(true);
        }
    }

    public void OnComplete(TextMeshProUGUI s)
    {
        if (s.text != "")
        {
            currentMonster.monster.shortName = s.text;
            monsterCaughtCloseButton.SetActive(true);
        }
    }

    public void OnClose()
    {
        if (isStillAlive)
        {
            monsterDiedUI.SetActive(false);
            monsterCaughtUI.SetActive(false);
            fightUI.SetActive(true);
            MyEventManager.LevelManager.OnStopFight?.Invoke();
        }
    }

    private void UpdateHealth(PlayerManager currentPlayer)
    {
        float currentHealth = (float)currentPlayer.currentHealth / (float)currentPlayer.maxHealth;
        healthBar.GetComponent<Image>().fillAmount = currentHealth;
    }
    private void UpdateActionPoints(PlayerManager currentPlayer)
    {
        float currentActionPoints = (float)currentPlayer.currentActionPoints / (float)currentPlayer.maxActionPoints;
        actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
    }
    private void UpdateMagicPoints(PlayerManager currentPlayer)
    {
        float currentMagicPoints = (float)currentPlayer.currentMagicPoints / (float)currentPlayer.maxMagicPoints;
        magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
    }
    private void UpdateHealth(FriendlyMonsterManager currentPlayer)
    {
        float currentHealth = (float)currentPlayer.monster.currentHealth / (float)currentPlayer.monster.maxHealth;
        healthBar.GetComponent<Image>().fillAmount = currentHealth;
    }
    private void UpdateActionPoints(FriendlyMonsterManager currentPlayer)
    {
        float currentActionPoints = (float)currentPlayer.monster.currentActionPoints / (float)currentPlayer.monster.maxActionPoints;
        actionPointsBar.GetComponent<Image>().fillAmount = currentActionPoints;
    }
    private void UpdateMagicPoints(FriendlyMonsterManager currentPlayer)
    {
        float currentMagicPoints = (float)currentPlayer.monster.currentMagicPoints / (float)currentPlayer.monster.maxMagicPoints;
        magicPointsBar.GetComponent<Image>().fillAmount = currentMagicPoints;
    }

    private void UpdateFightText(string newUpdate, string updateColor)
    {
        fightUpdates.text = newUpdate;
        if (updateColor == "White")
        {
            fightUpdates.color = white;
        }
        else if (updateColor == "Red")
        {
            fightUpdates.color = red;
        }
        else if (updateColor == "Blue")
        {
            fightUpdates.color = blue;
        }
    }
    private void UpdateFightersName(string newName)
    {
        fightersName.text = newName;
    }

    public void OnRewardedAd()
    {
        MyEventManager.AdManager.OnShowRewardedAd?.Invoke(DoubleEssence);
        monsterDiedRewardedAdButton.SetActive(false);
        monsterCaughtRewardedAdButton.SetActive(false);
    }
    private void DoubleEssence()
    {
        MyEventManager.Inventory.OnAddUmbraEssence?.Invoke(umbraEssence);
        monsterReport.text = $"Umbra Sap Vials Collected: {dropAmount}.\n\nUmbra Essence Collected: {umbraEssence * 2}.";
        monsterCaughtReport.text = $"Umbra Sap Vials Collected: {dropAmount}.\n\nUmbra Essence Collected: {umbraEssence * 2}.";
    }

    private void OnEnable()
    {
        MyEventManager.UI.FightUI.OnSetUpFightersUI += Initialize;
        MyEventManager.UI.FightUI.OnSetUpPlayersUI += Initialize;

        MyEventManager.UI.FightUI.OnUpdateFightUpdates += UpdateFightText;
        MyEventManager.UI.FightUI.OnUpdateFightersName += UpdateFightersName;

        MyEventManager.UI.FightUI.OnUpdatePlayerHealthUI += UpdateHealth;
        MyEventManager.UI.FightUI.OnUpdatePlayerActionPointsUI += UpdateActionPoints;
        MyEventManager.UI.FightUI.OnUpdatePlayerMagicPointsUI += UpdateMagicPoints;
        MyEventManager.UI.FightUI.OnUpdateFighterHealthUI += UpdateHealth;
        MyEventManager.UI.FightUI.OnUpdateFighterActionPointsUI += UpdateActionPoints;
        MyEventManager.UI.FightUI.OnUpdateFighterMagicPointsUI += UpdateMagicPoints;

        MyEventManager.UI.FightUI.OnEnemyDied += EnemyDied;
        MyEventManager.UI.FightUI.OnEnemyCaptuered += EnemyCaught;

        MyEventManager.UI.FightUI.OnUpdateButtons += SetUpInventoryItems;
    }

    private void OnDisable()
    {
        MyEventManager.UI.FightUI.OnSetUpFightersUI -= Initialize;
        MyEventManager.UI.FightUI.OnSetUpPlayersUI -= Initialize;

        MyEventManager.UI.FightUI.OnUpdateFightUpdates -= UpdateFightText;
        MyEventManager.UI.FightUI.OnUpdateFightersName -= UpdateFightersName;

        MyEventManager.UI.FightUI.OnUpdatePlayerHealthUI -= UpdateHealth;
        MyEventManager.UI.FightUI.OnUpdatePlayerActionPointsUI -= UpdateActionPoints;
        MyEventManager.UI.FightUI.OnUpdatePlayerMagicPointsUI -= UpdateMagicPoints;
        MyEventManager.UI.FightUI.OnUpdateFighterHealthUI -= UpdateHealth;
        MyEventManager.UI.FightUI.OnUpdateFighterActionPointsUI -= UpdateActionPoints;
        MyEventManager.UI.FightUI.OnUpdateFighterMagicPointsUI -= UpdateMagicPoints;

        MyEventManager.UI.FightUI.OnEnemyDied -= EnemyDied;
        MyEventManager.UI.FightUI.OnEnemyCaptuered -= EnemyCaught;

        MyEventManager.UI.FightUI.OnUpdateButtons -= SetUpInventoryItems;
    }
}
