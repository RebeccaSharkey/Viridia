using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public Monster monster;

    //Movement Data
    private EnemyMovementController movementController;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image healthImage;
    [SerializeField] private Canvas canvas;


    private void Awake()
    {
        canvas.worldCamera = Camera.main;
    }

    public void Initialize(Monster newMonster)
    {
        monster = newMonster;

        //Sets up name and sprite
        gameObject.name = newMonster.fullName;
        gameObject.name = gameObject.name.Replace("(MonsterSO)", "");
        gameObject.GetComponent<SpriteRenderer>().sprite = monster.sprite;

        //Sets up the monsters movement
        movementController = gameObject.GetComponent<EnemyMovementController>();
        movementController.SetUpMonster(newMonster);

        levelText.text = $"Level: {monster.level}"; 
        float currentHealth = (float)monster.currentHealth / (float)monster.maxHealth;
        healthImage.fillAmount = currentHealth;
    }

    public void TakeDamage(int damage)
    {
        monster.TakeDamage(damage);
        if (monster.currentHealth <= 0)
        {
            float currentHealth = (float)monster.currentHealth / (float)monster.maxHealth;
            healthImage.fillAmount = currentHealth;
            MyEventManager.UI.FightUI.OnEnemyDied?.Invoke(this);
        }
        else
        {
            float currentHealth = (float)monster.currentHealth / (float)monster.maxHealth;
            healthImage.fillAmount = currentHealth;
        }
    }
    public void UseAP(int decriment)
    {
        monster.UseAP(decriment);
    }
    public bool CheckAP(int attackUsage)
    {
        if((monster.currentActionPoints - attackUsage) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public void UseMP(int decriment)
    {
        monster.UseMP(decriment);
    }
    public bool CheckMP(int spellUsage)
    {
        if ((monster.currentMagicPoints - spellUsage) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Heal(int buff, bool isCurrentFighter = true)
    {
        monster.Heal(buff);
    }
    public void RestoreAP(int buff, bool isCurrentFighter = true)
    {
        monster.RestoreAP(buff);
    }
    public void RestoreMP(int buff, bool isCurrentFighter = true)
    {
        monster.RestoreMP(buff);
    }

    public Ability PickMove()
    {
        return monster.currentAttacks[Random.Range(0, monster.currentAttacks.Count)];
    }

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;
    }
}
