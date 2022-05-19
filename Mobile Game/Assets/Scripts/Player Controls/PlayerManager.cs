using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] public SpriteRenderer thisSprite;

    [Header("General Data")]
    public string playerName;
    public int level;
    public float levelMultiplier;
    public int maxHealth;
    public int currentHealth;
    public int currentAttackPower;
    public int currentAttackSpeed;

    public int maxActionPoints;
    public int currentActionPoints;
    public int maxMagicPoints;
    public int currentMagicPoints;

    public float speedBoost;
    public float defenceBoost;
    public float attackBoost;
    public float magicBoost;

    [Header("Attacks and Power Ups")]
    public List<AttackSO> allowedAttackSOs = new List<AttackSO>();
    public List<PowerUpSO> allowedPowerUpSOs = new List<PowerUpSO>();
    public List<AttackSO> attackSOs = new List<AttackSO>();
    public List<PowerUpSO> powerUpSOs = new List<PowerUpSO>();
    public List<Attack> attacks = new List<Attack>();
    public List<PowerUp> powerUps = new List<PowerUp>();

    [Header("Affinities")]
    public List<Affinity> weakness;
    public List<Affinity> immunities;

    private void Start()
    {
        playerName = "Becca";

        maxHealth = 100;
        currentHealth = maxHealth;
        currentAttackPower = 20;
        currentAttackSpeed = 10;

        maxActionPoints = 100;
        currentActionPoints = maxActionPoints;
        maxMagicPoints = 100;
        currentMagicPoints = maxMagicPoints;

        speedBoost = 0f;
        defenceBoost = 0f;
        attackBoost = 0f;
        magicBoost = 0f;

        level = 1;
        levelMultiplier = 1.075f;

        foreach (AttackSO attack in attackSOs)
        {
            attacks.Add(new Attack(attack));
        }
        foreach (PowerUpSO powerUp in powerUpSOs)
        {
            powerUps.Add(new PowerUp(powerUp));
        }
    }

    public void SetSprite(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>($"Icons/Player/{spriteName}");
        if (sprite.name == spriteName)
        {
            thisSprite.sprite = sprite;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            MyEventManager.LevelManager.OnStopFight?.Invoke();
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerHealthUI?.Invoke(this);
    }
    public void UseAP(int decriment)
    {
        currentActionPoints -= decriment;
        if (currentActionPoints < 0)
        {
            currentActionPoints = 0;
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerActionPointsUI?.Invoke(this);
    }
    public bool CheckAP(int attackUsage)
    {
        if ((currentActionPoints - attackUsage) < 0)
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
        currentMagicPoints -= decriment;
        if (currentMagicPoints < 0)
        {
            currentMagicPoints = 0;
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerMagicPointsUI?.Invoke(this);
    }
    public bool CheckMP(int spellUsage)
    {
        if ((currentMagicPoints - spellUsage) < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Heal(int buff)
    {
        currentHealth += buff;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerHealthUI?.Invoke(this);
    }
    public void RestoreAP(int buff)
    {
        currentActionPoints += buff;
        if (currentActionPoints > maxActionPoints)
        {
            currentActionPoints = maxHealth;
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerActionPointsUI?.Invoke(this);
    }
    public void RestoreMP(int buff)
    {
        currentMagicPoints += buff;
        if (currentMagicPoints > maxMagicPoints)
        {
            currentMagicPoints = maxMagicPoints;
        }
        MyEventManager.UI.FightUI.OnUpdatePlayerMagicPointsUI?.Invoke(this);
    }

    public void ResetBoosts()
    {
        speedBoost = 0f;
        defenceBoost = 0f;
        attackBoost = 0f;
        magicBoost = 0f;
    }

    //Player Levelling is now depricated as after testing and re-balancing, player no longer levels within the game as scaling monsters to balance correctly and still off
    //A challenge means they scale either 1:1 with player, or they over scale or under scale which made the game too easy or unplayable, no outcomes were sufficent and so
    //Levels are kept exclusively for monsters however they can only level to level 3 and their build now acts as a sub-level
    public int GetPlayerLevel()
    {
        return level;
    }
    public void LevelUp()
    {
        level++;
        maxHealth = (int)(maxHealth * levelMultiplier);
        maxActionPoints = (int)(maxActionPoints * levelMultiplier);
        maxMagicPoints = (int)(maxMagicPoints * levelMultiplier);

        currentAttackPower = (int)(currentAttackPower * levelMultiplier);
        currentAttackSpeed = (int)(currentAttackSpeed * levelMultiplier);

        MyEventManager.UI.FightUI.OnUpdatePlayerHealthUI?.Invoke(this);
        MyEventManager.UI.FightUI.OnUpdatePlayerActionPointsUI?.Invoke(this);
        MyEventManager.UI.FightUI.OnUpdatePlayerMagicPointsUI?.Invoke(this);
    }

    private void OnEnable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines += StopAllCoroutines;

        MyEventManager.Player.OnGetPlayerLevel += GetPlayerLevel;
        MyEventManager.Player.OnHeal += Heal;
        MyEventManager.Player.OnAPIncrease += RestoreAP;
        MyEventManager.Player.OnMPIncrease += RestoreMP;
    }

    private void OnDisable()
    {
        MyEventManager.LevelManager.OnStopAllCoroutines -= StopAllCoroutines;

        MyEventManager.Player.OnGetPlayerLevel -= GetPlayerLevel;
        MyEventManager.Player.OnHeal -= Heal;
        MyEventManager.Player.OnAPIncrease -= RestoreAP;
        MyEventManager.Player.OnMPIncrease -= RestoreMP;
    }
}
