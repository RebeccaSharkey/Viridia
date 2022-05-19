using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    //Player Save
    [SerializeField] private List<Monster> hubMonsters;
    [SerializeField] private List<Monster> GetHubMonsters() { return hubMonsters; }
    [SerializeField] private List<Monster> playerMonsters;
    [SerializeField] private List<Monster> GetPlayerMonsters() { return playerMonsters; }
    [SerializeField] private bool isTutorialComplete;
    [SerializeField] private bool GetTutorialStatus() { return isTutorialComplete; }
    [SerializeField] private string playerName;
    [SerializeField] private string GetPlayerName() { return playerName; }
    [SerializeField] private string playerSpritePath;
    [SerializeField] private string GetPlayerSpritePath() { return playerSpritePath; }
    [SerializeField] private List<Attack> playerAttacks;
    [SerializeField] private List<Attack> GetPlayerAttacks() { return playerAttacks; }

    //Hub Save
    [SerializeField] private Dictionary<Item, int> hubInventory;
    [SerializeField] private Dictionary<Item, int> GetHubInventory() { return hubInventory; }
    [SerializeField] private Dictionary<Item, int> playerInventory;
    [SerializeField] private Dictionary<Item, int> GetPlayerInventory() { return playerInventory; }

    //Player and Hub Save
    [SerializeField] private int playerLevel;
    [SerializeField] private int GetPlayerLevel() { return playerLevel; }
    [SerializeField] private long umbraEssence;
    [SerializeField] private long GetUmbraEssence() { return umbraEssence; }

    //Boss Save
    [SerializeField] private List<Boss> bosses;
    [SerializeField] private List<Boss> GetBosses() { return bosses;  }
    /*After though - Originally the party was only saved to the boss save as this was added some time after the player and hub save
     was added, the party was only saved so that the party could be used in boss fights, however after the boss fight is completed
     and the player wins the party gets saved as they didn't die, therefore I should have actually put this in the player save so boss save didn't
     have to be saved on loaded in the hub or game scene, however as this would have changed too many scripts already created I just loaded the
     boss save at the start of the hub and game scene and got the party that way as that needed to be added anyway and wouldn't have required
     changes. On official release I will come back to this and start saving the party in the player save for ease.*/
    [SerializeField] private List<Monster> partyMonsters;
    [SerializeField] private List<Monster> GetPartyMonsters() { return partyMonsters; }

    private void Start()
    {
        hubMonsters = new List<Monster>();
        playerMonsters = new List<Monster>();
        hubInventory = new Dictionary<Item, int>();
        playerInventory = new Dictionary<Item, int>();
        playerLevel = 1;
        umbraEssence = 0;
        bosses = new List<Boss>();
        partyMonsters = new List<Monster>();
        isTutorialComplete = false;
        playerAttacks = new List<Attack>();
        MyEventManager.UI.OnBeginMainMenu?.Invoke();
    }

    public void SaveToHub()
    {
        HubSave hubSave = new HubSave();
        hubSave.monsters = hubMonsters;
        foreach (KeyValuePair<Item, int> item in hubInventory)
        {
            hubSave.inventoryItems.Add(item.Key);
            hubSave.inventoryAmounts.Add(item.Value);
        }
        hubSave.playerLevel = playerLevel;
        hubSave.umbraEssence = umbraEssence;
        var myPath = Path.Combine(Application.persistentDataPath, "HubSave.dat");
        try
        {
            File.WriteAllText(myPath, hubSave.ToJson());
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't write to: {myPath} - with exception {e}");
#endif
        }
    }
    public void LoadHub()
    {
        string _data;
        var myPath = Path.Combine(Application.persistentDataPath, "HubSave.dat");
        HubSave hubSave = new HubSave();
        try
        {
            _data = File.ReadAllText(myPath);
            hubSave.LoadFromJson(_data);
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't read from: {myPath} - with exception {e}");
#endif
            _data = "";
        }

        hubInventory.Clear();
        foreach (Item item in hubSave.inventoryItems)
        {
            int tempAmount = hubSave.inventoryAmounts[hubSave.inventoryItems.IndexOf(item)];
            item.CreateIcon();
            hubInventory.Add(item, tempAmount);
        }

        hubMonsters.Clear();
        foreach (Monster monster in hubSave.monsters)
        {
            monster.CreateIcon();
            hubMonsters.Add(monster);
        }

        playerLevel = hubSave.playerLevel;
        umbraEssence = hubSave.umbraEssence;
    }

    public void SaveToPlayer()
    {
        PlayerSave playerSave = new PlayerSave();
        playerSave.monsters = playerMonsters;

        foreach (KeyValuePair<Item, int> item in playerInventory)
        {
            playerSave.inventoryItems.Add(item.Key);
            playerSave.inventoryAmounts.Add(item.Value);
        }
        playerSave.playerLevel = playerLevel;
        playerSave.umbraEssence = umbraEssence;
        playerSave.isTutorialComplete = isTutorialComplete;
        playerSave.playerName = playerName;
        playerSave.spritePath = playerSpritePath;
        playerSave.attacks = playerAttacks;

        var myPath = Path.Combine(Application.persistentDataPath, "PlayerSave.dat");
        try
        {
            File.WriteAllText(myPath, playerSave.ToJson());
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't write to: {myPath} - with exception {e}");
#endif
        }
    }
    public void LoadPlayer()
    {
        string _data;
        var myPath = Path.Combine(Application.persistentDataPath, "PlayerSave.dat");
        PlayerSave playerSave = new PlayerSave();
        try
        {
            _data = File.ReadAllText(myPath);
            playerSave.LoadFromJson(_data);
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't read from: {myPath} - with exception {e}");
#endif
            _data = "";
        }

        playerInventory.Clear();
        foreach (Item item in playerSave.inventoryItems)
        {
            int tempAmount = playerSave.inventoryAmounts[playerSave.inventoryItems.IndexOf(item)];
            item.CreateIcon();
            playerInventory.Add(item, tempAmount);
        }

        playerMonsters.Clear();
        foreach (Monster monster in playerSave.monsters)
        {
            monster.CreateIcon();
            playerMonsters.Add(monster);
        }

        playerLevel = playerSave.playerLevel;
        umbraEssence = playerSave.umbraEssence;
        isTutorialComplete = playerSave.isTutorialComplete;
        playerName = playerSave.playerName;
        playerSpritePath = playerSave.spritePath;

        playerAttacks.Clear();
        if(playerSave.attacks.Count > 0)
        {
            foreach(Attack attack in playerSave.attacks)
            {
                playerAttacks.Add(attack);
            }
        }
    }

    public void SaveToBoss()
    {
        BossSave bossSave = new BossSave();
        bossSave.bosses = bosses;
        bossSave.monsters = partyMonsters;
        var myPath = Path.Combine(Application.persistentDataPath, "BossSave.dat");
        try
        {
            File.WriteAllText(myPath, bossSave.ToJson());
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't write to: {myPath} - with exception {e}");
#endif
        }
    }
    public void LoadBoss()
    {
        string _data;
        var myPath = Path.Combine(Application.persistentDataPath, "BossSave.dat");
        BossSave bossSave = new BossSave();
        try
        {
            _data = File.ReadAllText(myPath);
            bossSave.LoadFromJson(_data);
        }
        catch (System.Exception e)
        {
#if UNITY_EDITOR
            Debug.Log($"Couldn't read from: {myPath} - with exception {e}");
#endif
            _data = "";
        }

        if (_data != "")
        {
            bosses.Clear();
            foreach (Boss boss in bossSave.bosses)
            {
                //monster.CreateIcon();
                bosses.Add(boss);
            }

            partyMonsters.Clear();
            foreach (Monster monster in bossSave.monsters)
            {
                monster.CreateIcon();
                partyMonsters.Add(monster);
            }
        }
    }

    private void DeleteHubSaveData()
    {
        hubMonsters = new List<Monster>();
        hubInventory = new Dictionary<Item, int>();
        playerLevel = 1;
        umbraEssence = 0;
        SaveToHub();
    }
    private void AddMonstersToHub(List<Monster> newMonsters)
    {
        LoadHub();
        foreach (Monster monster in newMonsters)
        {
            if (!hubMonsters.Contains(monster))
            {
                hubMonsters.Add(monster);
            }
        }
        SaveToHub();
    }
    private void AddItemsToHub(Dictionary<Item, int> newItems)
    {
        LoadHub();
        foreach (KeyValuePair<Item, int> item in newItems)
        {
            if (hubInventory.ContainsKey(item.Key))
            {
                hubInventory[item.Key] += item.Value;
            }
            else
            {
                hubInventory.Add(item.Key, item.Value);
            }
        }
        SaveToHub();
    }

    private void DeletePlayerSaveData()
    {
        playerMonsters = new List<Monster>();
        playerInventory = new Dictionary<Item, int>();
        playerLevel = 1;
        umbraEssence = 0;
        SaveToPlayer();
    }
    private void NewPlayerSaveData()
    {
        playerMonsters = new List<Monster>();
        playerInventory = new Dictionary<Item, int>();
        playerLevel = 1;
        umbraEssence = 0;
        isTutorialComplete = false;
        playerName = "";
        playerSpritePath = "";
        playerAttacks = new List<Attack>();
        SaveToPlayer();
    }
    private void AddMonstersToPlayer(List<Monster> newMonsters)
    {
        LoadPlayer();
        foreach (Monster monster in newMonsters)
        {
            if (!playerMonsters.Contains(monster))
            {
                playerMonsters.Add(monster);
            }
        }
        SaveToPlayer();
    }
    private void AddItemsToPlayer(Dictionary<Item, int> newItems)
    {
        LoadPlayer();
        foreach (KeyValuePair<Item, int> item in newItems)
        {
            if (playerInventory.ContainsKey(item.Key))
            {
                playerInventory[item.Key] += item.Value;
            }
            else
            {
                playerInventory.Add(item.Key, item.Value);
            }
        }
        SaveToPlayer();
    }
    private void ChangeTutorialStatus(bool newState)
    {
        LoadPlayer();
        isTutorialComplete = newState;
        SaveToPlayer();
    }
    private void SetPlayerName(string newName)
    {
        LoadPlayer();
        playerName = newName;
        SaveToPlayer();
    }
    private void SetPlayerSpritePath(string newPath)
    {
        LoadPlayer();
        playerSpritePath = newPath;
        SaveToPlayer();
    }
    private void SetPlayerAttacks(List<Attack> newAttacks)
    {
        LoadPlayer();
        playerAttacks = new List<Attack>(newAttacks);
        SaveToPlayer();
    }

    private void DeleteBossSave()
    {
        bosses = new List<Boss>();
        partyMonsters = new List<Monster>();
        SaveToBoss();
    }
    private void OnSaveBosses(List<Boss> newBosses)
    {
        bosses = new List<Boss>(newBosses);   
        SaveToBoss();
    }
    private void UpdateBossesCompletion(Boss updatedBoss)
    {
        LoadBoss();
        foreach (Boss oldBoss in bosses)
        {
            if (oldBoss.bossName == updatedBoss.bossName)
            {
                oldBoss.isCompleted = updatedBoss.isCompleted;
            }
        }
        SaveToBoss();
    }
    private void AddMonstersToParty(List<Monster> newMonsters)
    {
        LoadBoss();
        partyMonsters.Clear();
        foreach (Monster monster in newMonsters)
        {
            if (!partyMonsters.Contains(monster))
            {
                partyMonsters.Add(monster);
            }
        }
        SaveToBoss();
    }


    private void SetPlayersLevel(int newLevel)
    {
        LoadHub();
        LoadPlayer();
        playerLevel = newLevel;
        SaveToHub();
        SaveToPlayer();
    }

    private void SetUmbraEssence(long newAmount)
    {
        LoadHub();
        LoadPlayer();
        umbraEssence = newAmount;
        SaveToHub();
        SaveToPlayer();
    }


    private void OnEnable()
    {
        MyEventManager.SaveLoad.OnDeleteHubSaveInfo += DeleteHubSaveData;
        MyEventManager.SaveLoad.OnSaveMonstersToHub += AddMonstersToHub;
        MyEventManager.SaveLoad.OnSaveItemsToHub += AddItemsToHub;
        MyEventManager.SaveLoad.OnLoadHub += LoadHub;

        MyEventManager.SaveLoad.OnNewPlayer += NewPlayerSaveData;
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo += DeletePlayerSaveData;
        MyEventManager.SaveLoad.OnSaveMonstersToPlayer += AddMonstersToPlayer;
        MyEventManager.SaveLoad.OnSaveItemsToPlayer += AddItemsToPlayer;
        MyEventManager.SaveLoad.OnLoadPlayer += LoadPlayer;

        MyEventManager.SaveLoad.OnGetHubMonsters += GetHubMonsters;
        MyEventManager.SaveLoad.OnGetHubInventory += GetHubInventory;
        MyEventManager.SaveLoad.OnGetPlayerMonsters += GetPlayerMonsters;
        MyEventManager.SaveLoad.OnGetPlayerInventory += GetPlayerInventory;

        MyEventManager.SaveLoad.OnGetPlayerLevel += GetPlayerLevel;
        MyEventManager.SaveLoad.OnSetPlayerLevel += SetPlayersLevel;

        MyEventManager.SaveLoad.OnSetUmbraEssence += SetUmbraEssence;
        MyEventManager.SaveLoad.OnGetUmbraEssence += GetUmbraEssence;

        MyEventManager.SaveLoad.OnLoadBoss += LoadBoss;
        MyEventManager.SaveLoad.OnGetBosses += GetBosses;
        MyEventManager.SaveLoad.OnSaveNewBosses += OnSaveBosses;
        MyEventManager.SaveLoad.OnUpdateBoss += UpdateBossesCompletion;
        MyEventManager.SaveLoad.OnDeleteBossSave += DeleteBossSave;

        MyEventManager.SaveLoad.OnGetPartyMonsters += GetPartyMonsters;
        MyEventManager.SaveLoad.OnSaveMonstersToBoss += AddMonstersToParty;

        MyEventManager.SaveLoad.OnGetTutorialStatus += GetTutorialStatus;
        MyEventManager.SaveLoad.OnSetTutorialStatus += ChangeTutorialStatus;

        MyEventManager.SaveLoad.OnGetPlayerName += GetPlayerName;
        MyEventManager.SaveLoad.OnSetPlayerName += SetPlayerName;
        MyEventManager.SaveLoad.OnGetPlayerSprite += GetPlayerSpritePath;
        MyEventManager.SaveLoad.OnSetPlayerSprite += SetPlayerSpritePath;

        MyEventManager.SaveLoad.OnSetAttacks += SetPlayerAttacks;
        MyEventManager.SaveLoad.OnGetAttacks += GetPlayerAttacks;
    }
    private void OnDisable()
    {
        MyEventManager.SaveLoad.OnDeleteHubSaveInfo -= DeleteHubSaveData;
        MyEventManager.SaveLoad.OnSaveMonstersToHub -= AddMonstersToHub;
        MyEventManager.SaveLoad.OnSaveItemsToHub -= AddItemsToHub;
        MyEventManager.SaveLoad.OnLoadHub -= LoadHub;

        MyEventManager.SaveLoad.OnNewPlayer -= NewPlayerSaveData;
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo -= DeletePlayerSaveData;
        MyEventManager.SaveLoad.OnSaveMonstersToPlayer -= AddMonstersToPlayer;
        MyEventManager.SaveLoad.OnSaveItemsToPlayer -= AddItemsToPlayer;
        MyEventManager.SaveLoad.OnLoadPlayer -= LoadPlayer;

        MyEventManager.SaveLoad.OnGetHubMonsters -= GetHubMonsters;
        MyEventManager.SaveLoad.OnGetHubInventory -= GetHubInventory;
        MyEventManager.SaveLoad.OnGetPlayerMonsters -= GetPlayerMonsters;
        MyEventManager.SaveLoad.OnGetPlayerInventory -= GetPlayerInventory;

        MyEventManager.SaveLoad.OnGetPlayerLevel -= GetPlayerLevel;
        MyEventManager.SaveLoad.OnSetPlayerLevel -= SetPlayersLevel;

        MyEventManager.SaveLoad.OnSetUmbraEssence -= SetUmbraEssence;
        MyEventManager.SaveLoad.OnGetUmbraEssence -= GetUmbraEssence;

        MyEventManager.SaveLoad.OnLoadBoss -= LoadBoss;
        MyEventManager.SaveLoad.OnGetBosses -= GetBosses;
        MyEventManager.SaveLoad.OnSaveNewBosses -= OnSaveBosses;
        MyEventManager.SaveLoad.OnUpdateBoss -= UpdateBossesCompletion;
        MyEventManager.SaveLoad.OnDeleteBossSave -= DeleteBossSave;

        MyEventManager.SaveLoad.OnGetPartyMonsters -= GetPartyMonsters;
        MyEventManager.SaveLoad.OnSaveMonstersToBoss -= AddMonstersToParty;

        MyEventManager.SaveLoad.OnGetTutorialStatus -= GetTutorialStatus;
        MyEventManager.SaveLoad.OnSetTutorialStatus -= ChangeTutorialStatus;

        MyEventManager.SaveLoad.OnGetPlayerName -= GetPlayerName;
        MyEventManager.SaveLoad.OnSetPlayerName -= SetPlayerName;
        MyEventManager.SaveLoad.OnGetPlayerSprite -= GetPlayerSpritePath;
        MyEventManager.SaveLoad.OnSetPlayerSprite -= SetPlayerSpritePath;

        MyEventManager.SaveLoad.OnSetAttacks -= SetPlayerAttacks;
        MyEventManager.SaveLoad.OnGetAttacks -= GetPlayerAttacks;
    }
}
