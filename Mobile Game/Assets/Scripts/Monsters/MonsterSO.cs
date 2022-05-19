using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Monster Enums
public enum MonsterAggression { Shy, Normal, Aggressive }
public enum MonsterBuild { Small, Medium, Large, Apex }

[System.Serializable]
public class Affinity
{
    public DamageType damageType;
    public float multiplier;

    public Affinity(DamageType type, float thisMultiplier)
    {
        damageType = type;
        multiplier = thisMultiplier;
    }
}

[CreateAssetMenu(fileName = "New Monster", menuName = "New Monster")]
public class MonsterSO : ScriptableObject
{
    [Header("Monsters Varaiables")]
    public Sprite sprite;

    [Header("Movement Varaibles")]
    public float maxWalkDistance;
    public float normalViewRadius;
    public float chasingViewRadius;

    [Header("Fight Variables")]
    public int baseHealth;
    public int baseAtackPower;
    public int baseAttackSpeed;
    public int baseAttackPoints;
    public int baseMagicPoints;
    public float baseLevelMultiplier;

    [Header("Allowed Attacks and Power Ups")]
    public List<AttackSO> allowedAttacks;
    public List<PowerUpSO> allowedPowerUps;

    [Header("Affinities")]
    public List<Affinity> weakness;
    public List<Affinity> immunities;

    private MonsterSO()
    {
        sprite = null;
        maxWalkDistance = 5f;
        normalViewRadius = 4f;
        chasingViewRadius = 2f;

        baseHealth = 100;
        baseAtackPower = 10;
        baseAttackSpeed = 10;

        baseLevelMultiplier = 1.2f;
    }
}

[System.Serializable]
public class Monster
{
    [Header("General Data")]
    public string shortName;
    public string fullName;
    public MonsterAggression monsterAggression;
    public MonsterBuild monsterBuild;
    public int level;
    public int catchLevel;

    //MonsterSO Data
    public Sprite _sprite;
    public Sprite sprite { get => _sprite; }
    public string spritePath;

    [Header("MonsterSO data")]
    public float maxWalkDistance;
    public float normalViewRadius;
    public float chasingViewRadius;
    public int baseHealth;
    public int baseAtackPower;
    public int baseAttackSpeed;
    public int baseAttackPoints;
    public int baseMagicPoints;
    public float baseLevelMultiplier;
    public List<Attack> allowedAttacks;
    public List<PowerUp> allowedPowerUps;

    //Fight Data
    [HideInInspector] public float buildHealthMultiplier;
    [HideInInspector] public float buildAttackPowerMultiplier;
    [HideInInspector] public float buildAttackSpeedMultiplier;
    [HideInInspector] public float buildAttackPointsMultiplier;
    [HideInInspector] public float buildMagicPointsMultiplier;

    [Header("Fight Data")]
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

    //Current Attacks and Power Ups
    [SerializeField] public List<Attack> currentAttacks;
    [SerializeField] public List<PowerUp> currentPowerUps;

    [Header("Affinities")]
    public List<Affinity> weakness;
    public List<Affinity> immunities;

    public Monster(MonsterSO newMonsterSO, int newLevel, MonsterAggression aggression = MonsterAggression.Normal, MonsterBuild build = MonsterBuild.Medium)
    {
        monsterAggression = aggression;
        monsterBuild = build;

        fullName = monsterBuild.ToString() + " " + monsterAggression.ToString() + " " + newMonsterSO;
        fullName = fullName.Replace("(MonsterSO)", "");
        shortName = newMonsterSO.ToString();
        shortName = shortName.Replace("(MonsterSO)", "");

        level = newLevel;
        if((level == 1 && monsterBuild == MonsterBuild.Small) || (level == 2 && monsterBuild == MonsterBuild.Small))
        {
            catchLevel = 1;
        }
        else if((level == 1 && monsterBuild == MonsterBuild.Medium) || (level == 3 && monsterBuild == MonsterBuild.Small))
        {
            catchLevel = 2;
        }
        else if((level == 1 && monsterBuild == MonsterBuild.Large) || (level == 1 && monsterBuild == MonsterBuild.Apex) || (level == 2 && monsterBuild == MonsterBuild.Medium))
        {
            catchLevel = 3;
        }
        else if((level == 2 && monsterBuild == MonsterBuild.Large) || (level == 2 && monsterBuild == MonsterBuild.Apex) || (level == 3 && monsterBuild == MonsterBuild.Medium))
        {
            catchLevel = 4;
        }
        else if((level == 3 && monsterBuild == MonsterBuild.Large) || (level == 3 && monsterBuild == MonsterBuild.Apex))
        {
            catchLevel = 5;
        }

        //Grabs data from MonsterSO (Scriptable objects can't be saved properly in JSON)
        if (newMonsterSO.sprite != null)
        {
            _sprite = newMonsterSO.sprite;
            spritePath = "";
            spritePath = _sprite.name;
        }
        maxWalkDistance = newMonsterSO.maxWalkDistance;
        normalViewRadius = newMonsterSO.normalViewRadius;
        chasingViewRadius = newMonsterSO.chasingViewRadius;
        baseHealth = newMonsterSO.baseHealth;
        baseAtackPower = newMonsterSO.baseAtackPower;
        baseAttackSpeed = newMonsterSO.baseAttackSpeed;
        baseAttackPoints = newMonsterSO.baseAttackPoints;
        baseMagicPoints = newMonsterSO.baseMagicPoints;
        baseLevelMultiplier = newMonsterSO.baseLevelMultiplier;
        allowedAttacks = new List<Attack>();
        foreach (AttackSO attack in newMonsterSO.allowedAttacks)
        {
            allowedAttacks.Add(new Attack(attack));
        }
        allowedPowerUps = new List<PowerUp>();
        foreach (PowerUpSO powerUp in newMonsterSO.allowedPowerUps)
        {
            allowedPowerUps.Add(new PowerUp(powerUp));
        }

        //Sets the monsters attacks
        currentAttacks = new List<Attack>();
        if (newMonsterSO.allowedAttacks.Count > 3)
        {
            int randomAttacksAmount = Random.Range(1, 4);
            for (int i = 1; i <= randomAttacksAmount; i++)
            {
                bool added = false;
                while (!added)
                {
                    int randomAttack = Random.Range(0, newMonsterSO.allowedAttacks.Count);
                    if (!currentAttacks.Contains(allowedAttacks[randomAttack]))
                    {
                        currentAttacks.Add(allowedAttacks[randomAttack]);
                        added = true;
                    }
                }
            }
        }
        else if (newMonsterSO.allowedAttacks.Count > 1)
        {
            int randomAttacksAmount = Random.Range(1, newMonsterSO.allowedAttacks.Count + 1);
            for (int i = 1; i <= randomAttacksAmount; i++)
            {
                bool added = false;
                while (!added)
                {
                    int randomAttack = Random.Range(0, newMonsterSO.allowedAttacks.Count);
                    if (!currentAttacks.Contains(allowedAttacks[randomAttack]))
                    {
                        currentAttacks.Add(allowedAttacks[randomAttack]);
                        added = true;
                    }
                }
            }
        }
        else if (newMonsterSO.allowedAttacks.Count == 1)
        {
            currentAttacks.Add(allowedAttacks[0]);
        }

        //Create Powerups
        currentPowerUps = new List<PowerUp>();

        //Creates level multipliers depending on build
        switch (monsterBuild)
        {
            case MonsterBuild.Small:
                buildHealthMultiplier = 0.8f;
                buildAttackPowerMultiplier = 0.8f;
                buildAttackSpeedMultiplier = 1.4f;
                buildAttackPointsMultiplier = 0.8f;
                buildMagicPointsMultiplier = 1.2f;
                break;
            case MonsterBuild.Medium:
                buildHealthMultiplier = 1f;
                buildAttackPowerMultiplier = 1f;
                buildAttackSpeedMultiplier = 1f;
                buildAttackPointsMultiplier = 1f;
                buildMagicPointsMultiplier = 1f;
                break;
            case MonsterBuild.Large:
                buildHealthMultiplier = 1.2f;
                buildAttackPowerMultiplier = 1.2f;
                buildAttackSpeedMultiplier = 0.8f;
                buildAttackPointsMultiplier = 1.2f;
                buildMagicPointsMultiplier = 0.8f;
                break;
            case MonsterBuild.Apex:
                buildHealthMultiplier = 1.4f;
                buildAttackPowerMultiplier = 1.2f;
                buildAttackSpeedMultiplier = 1.2f;
                buildAttackPointsMultiplier = 1.2f;
                buildMagicPointsMultiplier = 1.2f;
                break;
        }

        //Sets monsters stats based on monsters level
        //Sets the start attributes of monster based on determind base health and then multiplying by it's build multiplier
        maxHealth = (int)(baseHealth * buildHealthMultiplier);
        currentAttackPower = (int)(baseAtackPower * buildAttackPowerMultiplier);
        currentAttackSpeed = (int)(baseAttackSpeed * buildAttackSpeedMultiplier);
        maxActionPoints = (int)(baseAttackPoints * buildAttackPointsMultiplier);
        maxMagicPoints = (int)(baseMagicPoints * buildMagicPointsMultiplier);

        //Incriments the attributes depending on level
        for (int i = 1; i < level; i++)
        {
            maxHealth = (int)(maxHealth * baseLevelMultiplier);
            currentAttackPower = (int)(currentAttackPower * baseLevelMultiplier);
            currentAttackSpeed = (int)(currentAttackSpeed * baseLevelMultiplier);
            maxActionPoints = (int)(maxActionPoints * baseLevelMultiplier);
            maxMagicPoints = (int)(maxMagicPoints * baseLevelMultiplier);
        }

        currentHealth = maxHealth;
        currentActionPoints = maxActionPoints;
        currentMagicPoints = maxMagicPoints;

        //Sets monsters weakness
        weakness = new List<Affinity>();
        foreach(Affinity affinity in newMonsterSO.weakness)
        {
            weakness.Add(affinity);
        }

        //Sets monsters Immunities
        immunities = new List<Affinity>();
        foreach(Affinity affinity1 in newMonsterSO.immunities)
        {
            immunities.Add(affinity1);
        }
    }

    public void CreateIcon()
    {        
        if (_sprite == null)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Icons/Monsters/{spritePath}");
            if (texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
                _sprite = sprite;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }
    public void UseAP(int decriment)
    {
        currentActionPoints -= decriment;
        if (currentActionPoints < 0)
        {
            currentActionPoints = 0;
        }
    }
    public void UseMP(int decriment)
    {
        currentMagicPoints -= decriment;
        if (currentMagicPoints < 0)
        {
            currentMagicPoints = 0;
        }
    }

    public void Heal(int buff)
    {
        currentHealth += buff;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    public void RestoreAP(int buff)
    {
        currentActionPoints += buff;
        if (currentActionPoints > maxActionPoints)
        {
            currentActionPoints = maxHealth;
        }
    }
    public void RestoreMP(int buff)
    {
        currentMagicPoints += buff;
        if (currentMagicPoints > maxMagicPoints)
        {
            currentMagicPoints = maxMagicPoints;
        }
    }

    public void ChangeName(string newName)
    {
        shortName = newName;
    }

    public void ResetBoosts()
    {
        speedBoost = 0f;
        defenceBoost = 0f;
        attackBoost = 0f;
        magicBoost = 0f;
    }

    public void LevelUp()
    {
        level++;
        maxHealth = (int)(maxHealth * baseLevelMultiplier);
        currentAttackPower = (int)(currentAttackPower * baseLevelMultiplier);
        currentAttackSpeed = (int)(currentAttackSpeed * baseLevelMultiplier);
        maxActionPoints = (int)(maxActionPoints * baseLevelMultiplier);
        maxMagicPoints = (int)(maxMagicPoints * baseLevelMultiplier);
    }


    public void LearnNewAbility(Ability ability)
    {
        if (ability is Attack)
        {
            if (currentAttacks.Count < 4)
            {
                currentAttacks.Add((Attack)ability);
            }
        }
        else if(ability is PowerUp)
        {
            if(currentPowerUps.Count < 4)
            {
                currentPowerUps.Add((PowerUp)ability);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Couldn't determine ability type");
#endif
        }
    }
    public void ForgetAbility(Ability ability)
    {
        if (ability is Attack)
        {
            if (currentAttacks.Contains((Attack)ability))
            {
                currentAttacks.Remove((Attack)ability);
            }
        }
        else if (ability is PowerUp)
        {
            if (currentPowerUps.Contains((PowerUp)ability))
            {
                currentPowerUps.Remove((PowerUp)ability);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Couldn't determine ability type");
#endif
        }
    }


    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public void LoadFromJson(string data)
    {
        JsonUtility.FromJsonOverwrite(data, this);
    }
}