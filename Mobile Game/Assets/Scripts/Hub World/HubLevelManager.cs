using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubLevelManager : MonoBehaviour
{
    private GameObject player;
    private Dictionary<Item, int> hubInventory;
    [SerializeField] private List<Monster> hubMonsters;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        hubInventory = new Dictionary<Item, int>();
        hubMonsters = new List<Monster>();
        StartCoroutine(SetUpScene());
    }

    IEnumerator SetUpScene()
    {
        yield return new WaitForSeconds(1f);

        player.SetActive(true);
        yield return new WaitForSeconds(0.5f);

        //Sets player in correct position
        MyEventManager.MapManager.OnSetPlayerPosition?.Invoke();

        //loads all Hub data data
        MyEventManager.SaveLoad.OnLoadHub?.Invoke();
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerManager>().level = (int)MyEventManager.SaveLoad.OnGetPlayerLevel?.Invoke();
        MyEventManager.Inventory.OnSetUmbraEssence?.Invoke((long)MyEventManager.SaveLoad.OnGetUmbraEssence?.Invoke());
        hubInventory = MyEventManager.SaveLoad.OnGetHubInventory?.Invoke();
        hubMonsters = MyEventManager.SaveLoad.OnGetHubMonsters?.Invoke();

        //Loads Player Data (should be empty if they didn't win the boss fight)
        MyEventManager.SaveLoad.OnLoadPlayer?.Invoke();
        yield return new WaitForSeconds(1f);
        player.GetComponent<PlayerManager>().currentHealth = player.GetComponent<PlayerManager>().maxHealth;
        player.GetComponent<PlayerManager>().currentActionPoints = player.GetComponent<PlayerManager>().maxActionPoints;
        player.GetComponent<PlayerManager>().currentMagicPoints = player.GetComponent<PlayerManager>().maxMagicPoints;
        player.GetComponent<PlayerManager>().currentMagicPoints = player.GetComponent<PlayerManager>().maxMagicPoints;
        MyEventManager.Inventory.OnSetInventory?.Invoke(MyEventManager.SaveLoad.OnGetPlayerInventory?.Invoke());
        MyEventManager.Party.OnSetHeldMonsters?.Invoke(MyEventManager.SaveLoad.OnGetPlayerMonsters?.Invoke());
        player.GetComponent<PlayerManager>().playerName = MyEventManager.SaveLoad.OnGetPlayerName.Invoke();
        if (MyEventManager.SaveLoad.OnGetPlayerSprite.Invoke() != null)
        {
            player.GetComponent<PlayerManager>().SetSprite(MyEventManager.SaveLoad.OnGetPlayerSprite.Invoke());
        }

        //Loads Boss Data (for players party should be empty if the player didn't win the boss fight)
        MyEventManager.SaveLoad.OnLoadBoss?.Invoke();
        yield return new WaitForSeconds(1f);
        MyEventManager.Party.OnSetParty?.Invoke(MyEventManager.SaveLoad.OnGetPartyMonsters?.Invoke());

        //Switches screen from loading to scene
        MyEventManager.ViewManager.HubViewManager.OnSetMapViewState?.Invoke(true);
        MyEventManager.ViewManager.HubViewManager.OnSetLoadViewState?.Invoke(false);

        //Allow player movement
        yield return new WaitForSeconds(0.5f);
        player.GetComponent<PlayerControls>().currentState = PlayerState.Roaming;
    }

    private void GoToPlayScene()
    {
        StartCoroutine(SwitchToPlay());
    }
    IEnumerator SwitchToPlay()
    {
        yield return null;

        //Change Views
        MyEventManager.ViewManager.HubViewManager.OnSetMapViewState?.Invoke(false);
        MyEventManager.ViewManager.HubViewManager.OnSetLoadViewState?.Invoke(true);
        yield return new WaitForSeconds(2f);

        MyEventManager.SaveLoad.OnDeleteHubSaveInfo?.Invoke();
        MyEventManager.SaveLoad.OnSaveItemsToHub?.Invoke(hubInventory);
        MyEventManager.SaveLoad.OnSaveMonstersToHub?.Invoke(hubMonsters);

        //-- Saves all player data --
        MyEventManager.SaveLoad.OnDeletePlayerSaveInfo?.Invoke();
        MyEventManager.SaveLoad.OnSetPlayerLevel?.Invoke(player.GetComponent<PlayerManager>().level);
        MyEventManager.SaveLoad.OnSetUmbraEssence?.Invoke((long)MyEventManager.Inventory.OnGetUmbraEssence?.Invoke());
        //Saves held Monsters
        List<Monster> monsters = new List<Monster>();
        monsters = MyEventManager.Party.OnGetHeldMonsters?.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToPlayer?.Invoke(monsters);
        //Saves Inventory
        Dictionary<Item, int> invnetory = new Dictionary<Item, int>();
        invnetory = MyEventManager.Inventory.OnGetInventory?.Invoke();
        MyEventManager.SaveLoad.OnSaveItemsToPlayer?.Invoke(invnetory);
        //Saves Party
        List<Monster> party = new List<Monster>();
        party = MyEventManager.Party.OnGetParty?.Invoke();
        MyEventManager.SaveLoad.OnSaveMonstersToBoss?.Invoke(party);


        yield return new WaitForSeconds(2f);

        //Load new scene
        MyEventManager.SceneManagement.OnLoadNewScene?.Invoke("GameScene");
    }

    private List<Monster> GetHubMonsters()
    {
        return hubMonsters;
    }
    private void SetHubMonsters(List<Monster> newMonsters)
    {
        hubMonsters = newMonsters;
    }
    private Dictionary<Item, int> GetHubInventory()
    {
        return hubInventory;
    }
    private void SetHubInventory(Dictionary<Item, int> newInventory)
    {
        hubInventory = newInventory;
    }

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnGoToPlayScene += GoToPlayScene;

        MyEventManager.LevelManager.OnGetHubMonsters += GetHubMonsters;
        MyEventManager.LevelManager.OnSetHubMonsters += SetHubMonsters;
        MyEventManager.LevelManager.OnGetHubInventory += GetHubInventory;
        MyEventManager.LevelManager.OnSetHubInventory += SetHubInventory;
    }
    private void OnDisable()
    {
        MyEventManager.LevelManager.OnGoToPlayScene -= GoToPlayScene;

        MyEventManager.LevelManager.OnGetHubMonsters -= GetHubMonsters;
        MyEventManager.LevelManager.OnSetHubMonsters -= SetHubMonsters;
        MyEventManager.LevelManager.OnGetHubInventory -= GetHubInventory;
        MyEventManager.LevelManager.OnSetHubInventory -= SetHubInventory;
    }
}
