using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeldMonsterButton : MonoBehaviour
{
    //Base Values
    [HideInInspector] public Monster monster;

    //UI
    [SerializeField] private Image monsterIcon;
    [SerializeField] private TextMeshProUGUI nameText;

    //Tree Slot UI Only
    [SerializeField] private bool isTreeUI;
    [HideInInspector] public TreeState treeState = TreeState.Default;
    [SerializeField] private AudioClip audioClip;

    public void SetUp(Monster newMonster)
    {
        monster = newMonster;
        monsterIcon.sprite = monster.sprite;
        nameText.text = monster.shortName;
    }

    public void Use()
    {
        MyEventManager.AudioManager.OnPlaySFX?.Invoke(audioClip);
        if (isTreeUI)
        {
            switch (treeState)
            {
                case TreeState.InPlayerInventory:
                    MyEventManager.UI.TreeUI.OnAddMonsterToHub?.Invoke(monster);
                    MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToHub?.Invoke(monster);
                    return;
                case TreeState.InHubInventory:
                    MyEventManager.UI.TreeUI.OnAddMonsterToPlayer?.Invoke(monster);
                    MyEventManager.UI.HubUI.MonstersUI.OnMonsterItemToPlayer?.Invoke(monster);
                    return;
                case TreeState.Default:
#if UNITY_EDITOR
                    Debug.LogWarning("State not Set");
#endif
                    return;
            }
        }
        else
        {
            MyEventManager.UI.MapUI.OnHeldMonsterInteraction?.Invoke(monster);
        }
    }
}
