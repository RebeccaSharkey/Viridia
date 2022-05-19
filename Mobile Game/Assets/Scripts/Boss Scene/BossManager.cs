using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Story
{
    public string speaker;
    public List<string> dialogue;
}

public class BossManager : MonoBehaviour
{
    [Header("Monster")]
    [SerializeField] private MonsterSO monsterSO;
    [SerializeField] public Monster monster;

    [Header("Story Data")]
    [SerializeField] private List<Story> story;
    private int storyIndex;
    private int dialogueIndex;

    [Header("FightData")]
    public List<string> abilities;

    private void Start()
    {
        monster = new Monster(monsterSO, 1);
        gameObject.GetComponent<SpriteRenderer>().sprite = monster.sprite;
    }

    public void StartStory()
    {
        storyIndex = 0;
        dialogueIndex = 0;
        ContinueStory();
    }

    private void ContinueStory()
    {
        StopAllCoroutines();
        if (story[storyIndex].dialogue.Count > dialogueIndex)
        {
            MyEventManager.UI.BossUI.OnSetHeader?.Invoke(story[storyIndex].speaker);
            StartCoroutine(PlayText(story[storyIndex].dialogue[dialogueIndex]));
            dialogueIndex++;
        }
        else
        {
            storyIndex++;
            if (story.Count > storyIndex)
            {
                dialogueIndex = 0;
                MyEventManager.UI.BossUI.OnSetHeader?.Invoke(story[storyIndex].speaker);
                StartCoroutine(PlayText(story[storyIndex].dialogue[dialogueIndex]));
                dialogueIndex++;
            }
            else
            {
                EndStory();
            }
        }
    }

    IEnumerator PlayText(string display)
    {
        foreach(char c in display)
        {
            MyEventManager.UI.BossUI.OnSetContent?.Invoke(c);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EndStory()
    {
        StopAllCoroutines();
        MyEventManager.ViewManager.BossViewManager.OnSetBossUI?.Invoke(false);
        MyEventManager.ViewManager.BossViewManager.OnSetFightUI?.Invoke(true);
        MyEventManager.LevelManager.BossLevelManager.OnStartFight?.Invoke();
    }

    private void ExecuteAbility()
    {
        int index = Random.Range(0, abilities.Count);
        MyEventManager.BossManager.OnExecuteAbility?.Invoke(abilities[index]);
    }

    private void OnEnable()
    {
        MyEventManager.BossManager.OnContinueStory += ContinueStory;
        MyEventManager.BossManager.OnSkipStory += EndStory;
        MyEventManager.BossManager.OnPickRandomAbility += ExecuteAbility;
    }

    private void OnDisable()
    {
        MyEventManager.BossManager.OnContinueStory -= ContinueStory;
        MyEventManager.BossManager.OnSkipStory -= EndStory;
        MyEventManager.BossManager.OnPickRandomAbility -= ExecuteAbility;
    }
}
