using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public enum AttackType
{
    Normal,
    Spell
}

public enum DamageType
{
    Normal,
    Fire,
    Water,
    Air,
    Earth,
    Umbra
}

[System.Serializable]
[CreateAssetMenu(fileName = "New Attack", menuName = "Abilities/New Attack")]
public class AttackSO : AbilitySO
{
    [Header("Attack Data")]
    public float attackPower;
    public AttackType attackType;
    public int attackUsage;
    public DamageType damageType;

    [Header("Crit Data")]
    public float critChance;
    public float critAttackPower;

    private ItemSO itemSO;
    public AttackSO()
    {
        attackPower = 1;

        critChance = 0.2f;
        critAttackPower = 2;

        attackType = AttackType.Normal;
        damageType = DamageType.Normal;
        attackUsage = 10;
    }

#if UNITY_EDITOR
    public void BuildTrainingScript()
    {
        if (itemSO == null)
        {
            itemSO = CreateInstance<ItemSO>();
            AssetDatabase.CreateAsset(itemSO, $"Assets/Resources/Training Scripts/{abilityName} Ability.asset");
            itemSO.itemType = ItemType.TrainingScripts;
            itemSO.attack = this;
        }
        itemSO.itemName = abilityName + " Ability";
        itemSO.itemLevel = "1";
        itemSO.cost = 1000;
        EditorUtility.SetDirty(itemSO);
    }
#endif
}

[System.Serializable]
public class Attack : Ability
{
    public string abilityName;

    [Header("Attack Data")]
    public float attackPower;
    public AttackType attackType;
    public int attackUsage;
    public DamageType damageType;

    [Header("Crit Data")]
    public float critChance;
    public float critAttackPower;

    public Attack()
    {
        abilityName = "Defualt Attack";
        attackPower = 1;

        critChance = 0.2f;
        critAttackPower = 2;

        attackType = AttackType.Normal;
        damageType = DamageType.Normal;
        attackUsage = 10;
    }

    public Attack(AttackSO refernceAttack)
    {
        abilityName = refernceAttack.abilityName;

        attackPower = refernceAttack.attackPower;

        critChance = refernceAttack.critChance;
        critAttackPower = refernceAttack.critAttackPower;

        attackType = refernceAttack.attackType;
        damageType = refernceAttack.damageType;
        attackUsage = refernceAttack.attackUsage;
    }
}
