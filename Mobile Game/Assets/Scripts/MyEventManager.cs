using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class MyEventManager
{
    public class SaveLoad
    {
        public static Action OnDeleteHubSaveInfo;
        public static Action<List<Monster>> OnSaveMonstersToHub;
        public static Action<Dictionary<Item, int>> OnSaveItemsToHub;
        public static Action OnLoadHub;

        public static Action OnNewPlayer;
        public static Action OnDeletePlayerSaveInfo;
        public static Action<List<Monster>> OnSaveMonstersToPlayer;
        public static Action<Dictionary<Item, int>> OnSaveItemsToPlayer;
        public static Action OnLoadPlayer;

        public static Func<List<Monster>> OnGetHubMonsters;
        public static Func<Dictionary<Item, int>> OnGetHubInventory;

        public static Func<List<Monster>> OnGetPlayerMonsters;
        public static Func<Dictionary<Item, int>> OnGetPlayerInventory;

        public static Action<int> OnSetPlayerLevel;
        public static Func<int> OnGetPlayerLevel;

        public static Action<long> OnSetUmbraEssence;
        public static Func<long> OnGetUmbraEssence;

        public static Action OnLoadBoss;
        public static Func<List<Boss>> OnGetBosses;
        public static Action<List<Boss>> OnSaveNewBosses;
        public static Action OnDeleteBossSave;
        public static Action<Boss> OnUpdateBoss;

        public static Func<List<Monster>> OnGetPartyMonsters;
        public static Action<List<Monster>> OnSaveMonstersToBoss;

        public static Func<bool> OnGetTutorialStatus;
        public static Action<bool> OnSetTutorialStatus;

        public static Func<string> OnGetPlayerName;
        public static Action<string> OnSetPlayerName;

        public static Func<string> OnGetPlayerSprite;
        public static Action<string> OnSetPlayerSprite;

        public static Action<List<Attack>> OnSetAttacks;
        public static Func<List<Attack>> OnGetAttacks;
    }

    public class SceneManagement
    {
        public static Action<string> OnLoadNewScene;
    }

    public class LevelManager
    {
        public static Action OnStopAllCoroutines;
        public static Action<AssetReference> OnTraverseNewMap;
        public static Action OnNavMeshBaked;

        public static Action<GameObject> OnSetUpFight;
        public static Action OnStopFight;

        public static Action OnGoToHubScene;
        public static Action OnGoToPlayScene;

        public static Func<List<Monster>> OnGetHubMonsters;
        public static Action<List<Monster>> OnSetHubMonsters;
        public static Func<Dictionary<Item, int>> OnGetHubInventory;
        public static Action<Dictionary<Item, int>> OnSetHubInventory;

        public class FightPhase
        {
            public static Action<Ability> OnPlayerChoseAbility;
            public static Action<Item> OnPlayerUsedCapture;
            public static Action<Item> OnPlayerUsedPotion;
            public static Action<Monster> OnSwitchToMonster;
            public static Action OnSwitchToPlayer;
        }

        public class BossLevelManager
        {
            public static Action OnStartFight;
            public static Action OnNextFighter;
            public static Action OnStopTimer;

            public static Action<int, DamageType, string> OnMonsterAttackSingleTarget;
            public static Action<int, DamageType, string> OnMonsterAttackAll;
            public static Action<string> OnMonsterNegateAllBoosts;
            public static Action<string> OnMonsterCharm;
            public static Action<Monster, string> OnMonsterSummon;
            public static Action<string> OnPotionSteal;
            public static Action<int, string> OnStealUmbraEssence;
            public static Action<string> OnUmbraEssenceAttack;

            public static Action OnRetryFight;
            public static Action OnHubFromPlayerDied;
            public static Action OnHubFromBossDied;
        }
    }

    public class ViewManager
    {
        public static Action<bool> OnSetMapViewState;
        public static Action<bool> OnSetFightViewState;
        public static Action<bool> OnSetLoadViewState;

        public static Action<AssetReference> OnChangeMap;

        public static Action OnFightCamShake;
        public static Action OnPlayerCamShake;

        public class HubViewManager
        {
            public static Action<bool> OnSetMapViewState;
            public static Action<bool> OnSetLoadViewState;
        }

        public class BossViewManager
        {
            public static Action<bool> OnSetMapViewState;
            public static Action<bool> OnSetLoadViewState;
            public static Action<bool> OnSetFightUI;
            public static Action<bool> OnSetBossUI;
        }
    }

    public class MapManager
    {
        public static Action OnSetPlayerPosition;
        public static Func<PolygonCollider2D> OnGetCameraBounds;

        public class BossMapManager
        {
            public static Action OnSetPlayerPosition;
            public static Func<PolygonCollider2D> OnGetCameraBounds;
            public static Action<List<GameObject>> OnSetFriendlyMonstersPosition;
            public static Action<GameObject> OnSetBossPositon;
        }
    }

    public class UI
    {
        public static Action OnBeginMainMenu;

        public class FightUI
        {
            public static Action<List<Attack>, List<PowerUp>, PlayerManager> OnSetUpPlayersUI;
            public static Action<List<Attack>, List<PowerUp>, FriendlyMonsterManager> OnSetUpFightersUI;

            public static Action<string, string> OnUpdateFightUpdates;
            public static Action<string> OnUpdateFightersName;
            public static Action<int> OnUpdateTotalDamageGiven;

            public static Action<PlayerManager> OnUpdatePlayerHealthUI;
            public static Action<PlayerManager> OnUpdatePlayerMagicPointsUI;
            public static Action<PlayerManager> OnUpdatePlayerActionPointsUI;
            public static Action<FriendlyMonsterManager> OnUpdateFighterHealthUI;
            public static Action<FriendlyMonsterManager> OnUpdateFighterMagicPointsUI;
            public static Action<FriendlyMonsterManager> OnUpdateFighterActionPointsUI;

            public static Action<EnemyManager> OnEnemyDied;
            public static Action<EnemyManager> OnEnemyCaptuered;

            public static Action OnUpdateButtons;

            public static Action<bool> OnToggleButtons;
        }

        public class MapUI
        {
            //Death UI
            public static Action<bool> OnToggleDeathUI;

            //Player Settings UI
            public static Action OnPlayerSettings;
            public static Action OnClosePlayerSettings;

            //Stats
            public static Action<int> OnUpdateHealth;
            public static Action<int> OnUpdateAP;
            public static Action<int> OnUpdateMP;
            public static Action<Item> OnLevelUp;

            //Switching currently Viewing
            public static Action OnSwitchToPlayer;
            public static Action<Monster> OnSwitchToMonster;
            public static Action<Monster> OnHeldMonsterInteraction;
            public static Action OnAddMonster;

            //Changing Attacks and PowerUps
            public static Action OnAddAttack;
            public static Action OnAddPowerUp;
            public static Action<Ability> OnOpenSwapAbilityUI;
            public static Action<Ability, Item> OnAddNewAbility;
        }

        public class TreeUI
        {
            public static Action<Tree> OnOpenTree;
            public static Action OnCloseTree;

            public static Action<Item, int> OnAddItemToHub;
            public static Action<Item, int> OnAddItemToPlayer;

            public static Action<Monster> OnAddMonsterToHub;
            public static Action<Monster> OnAddMonsterToPlayer;
        }

        public class HubUI
        {
            public class InventoryUI
            {
                public static Action OnOpenUI;
                public static Action<Item, int> OnAddItemToHub;
                public static Action<Item, int> OnAddItemToPlayer;
            }

            public class MonstersUI
            {
                public static Action OnOpenUI;
                public static Action<Monster> OnMonsterItemToHub;
                public static Action<Monster> OnMonsterItemToPlayer;
            }

            public class TrainingScriptShopUI
            {
                public static Action OnOpenUI;
                public static Action<Item> OnBuyItem;
            }
        }

        public class BossUI
        {
            public static Action<string> OnSetHeader;
            public static Action<char> OnSetContent;

            public static Action<List<GameObject>> OnUpdateQueue;
            public static Action<GameObject> OnSetUpNewFighter;

            public static Action<float> OnUpdateTimer;

            public static Action<bool> OnToggleFightUI;
            public static Action<bool> OnToggleFightUpdatesUI;
            public static Action<string, string> OnUpdateFightUI;

            public static Action<Monster> OnToggleBossDiedUI;
            public static Action<bool> OnTogglePlayerDiedUI;
        }
    }

    public class Player
    {
        public static Action<int> OnHeal;
        public static Action<int> OnAPIncrease;
        public static Action<int> OnMPIncrease;
        public static Func<int> OnGetPlayerLevel;
    }

    public class Monsters
    {
        public static Action<bool> OnToggleRoaming;
        public static Action<bool> OnToggleMovement;

        public static Action OnFightStarted;
        public static Action OnFightStopped;

        public class Manager
        {
            public static Action OnSetUpMonsters;
            public static Action OnDeleteAllMonsters;

            public static Action<GameObject> OnRemoveMonsterFromScene;

            public static Action<MonsterSO> OnCreateTutorialMonster;
        }
    }

    public class Inventory
    {
        public static Func<Dictionary<Item, int>> OnGetInventory;
        public static Action<Dictionary<Item, int>> OnSetInventory;
        public static Action<Item, int> OnAddToInventory;
        public static Action<Item, int> OnRemoveFromInventory;

        public static Func<long> OnGetUmbraEssence;
        public static Action<long> OnSetUmbraEssence;
        public static Action<long> OnAddUmbraEssence;
        public static Action<long> OnRemoveUmbraEssence;
        public static Action<long> OnUpdateEssenceUI;
    }

    public class Party
    {
        public static Func<List<Monster>> OnGetParty;
        public static Action<List<Monster>> OnSetParty;
        public static Action<Monster> OnAddToParty;
        public static Action<Monster> OnRemoveFromParty;

        public static Func<List<Monster>> OnGetHeldMonsters;
        public static Action<List<Monster>> OnSetHeldMonsters;
        public static Action<Monster> OnAddToHeldMonsters;
        public static Action<Monster> OnRemoveFromHeldMonsters;
    }

    public class BossManager
    {
        public static Action OnContinueStory;
        public static Action OnSkipStory;
        public static Action OnPickRandomAbility;
        public static Action<string> OnExecuteAbility;
    }

    public class AudioManager
    {
        public static Action<AudioClip> OnPlaySFX;
        public static Action<AudioClip> OnPlayThemeMusic;
    }

    public class Tutorial
    {
        public static Action OnCloseTutorialBox;
        public static Action OnTutorialSetUp;
        public static Action OnBeginTutorial;
        public static Action OnReachedDestination;
        public static Action OnTreeViewed;
        public static Action OnNewMapSetUp;
        public static Action OnEnemyFaught;
    }

    public class AdManager
    {
        public static Func<bool> OnGetIsAdLoaded;
        public static Action<Action> OnShowRewardedAd;
    }
}
