using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Default,
    Locket,
    Potion,
    MonsterRemains,
    TrainingScripts
}

[System.Serializable]
public enum PotionType
{
    Health,
    Stamina,
    Magic,

    AttackBoost,
    MagicBoost
}

[CreateAssetMenu(fileName = "New Item", menuName = "New Item")]
[System.Serializable]
public class ItemSO : ScriptableObject
{
    [Header("Base Variables")]
    [HideInInspector] public string itemName;
    [HideInInspector] public Sprite itemIcon;
    [HideInInspector] public ItemType itemType;
    [HideInInspector] public string itemLevel;

    [Header("Locket Variables")]
    [HideInInspector] public int locketLevel;

    [Header("Potion Variables")]
    [HideInInspector] public PotionType potionType;
    [HideInInspector] public int potionIncriment;
    [HideInInspector] public float potionBoost;

    [Header("Training Script Variables")]
    [HideInInspector] public AttackSO attack;
    [HideInInspector] public PowerUpSO powerUp;
    [HideInInspector] public int cost;

    public ItemSO()
    {
        itemName = "New Item";
        itemType = ItemType.Default;
    }

}

[System.Serializable]
public class Item
{
    [Header("Base Variables")]
    [HideInInspector] public string itemName;
    private Sprite _icon;
    [HideInInspector] public Sprite itemIcon { get => _icon; }
    [HideInInspector] public string spritePath;
    [HideInInspector] public ItemType itemType;
    [HideInInspector] public string itemLevel;

    [Header("Locket Variables")]
    [HideInInspector] public int locketLevel;

    [Header("Potion Variables")]
    [HideInInspector] public PotionType potionType;
    [HideInInspector] public int potionIncriment;
    [HideInInspector] public float potionBoost;

    [Header("Training Script Variables")]
    [HideInInspector] public Attack attack;
    [HideInInspector] public PowerUp powerUp;
    [HideInInspector] public int cost;

    public Item(ItemSO referenceItem)
    {
        itemName = referenceItem.itemName;
        if(referenceItem.itemIcon != null)
        {
            _icon = referenceItem.itemIcon;
            spritePath = "";
            spritePath = _icon.name;
        }
        itemType = referenceItem.itemType;
        itemLevel = referenceItem.itemLevel;

        if (itemType == ItemType.Locket)
        {
            locketLevel = referenceItem.locketLevel;
        }

        if (itemType == ItemType.Potion)
        {
            potionType = referenceItem.potionType;
            if (potionType != PotionType.AttackBoost && potionType != PotionType.MagicBoost)
            {
                potionIncriment = referenceItem.potionIncriment;
            }
            else
            {
                potionBoost = referenceItem.potionBoost;
            }
        }

        if(itemType == ItemType.TrainingScripts)
        {
            if(referenceItem.attack != null)
            { 
                attack = new Attack(referenceItem.attack); 
            }
            else
            {
                powerUp = new PowerUp(referenceItem.powerUp);
            }
            cost = referenceItem.cost;
        }
    }
    public Item(Item referenceItem)
    {
        itemName = referenceItem.itemName;

        if (referenceItem.itemIcon != null)
        {
            _icon = referenceItem.itemIcon;
            spritePath = "";
            spritePath = _icon.name;
        }
        itemType = referenceItem.itemType;
        itemLevel = referenceItem.itemLevel;

        if(itemType == ItemType.Locket)
        {
            locketLevel = referenceItem.locketLevel;
        }

        if(itemType == ItemType.Potion)
        {
            potionType = referenceItem.potionType;
            if(potionType != PotionType.AttackBoost && potionType != PotionType.MagicBoost)
            {
                potionIncriment = referenceItem.potionIncriment;
            }
            else
            {
                potionBoost = referenceItem.potionBoost;
            }
        }

        if(itemType == ItemType.TrainingScripts)
        {
            if (referenceItem.attack != null)
            {
                attack = referenceItem.attack;
            }
            else
            {
                powerUp = referenceItem.powerUp;
            }
            cost = referenceItem.cost;
        }
    }

    public void CreateIcon()
    {
        if(itemIcon == null)
        {
            Texture2D texture = Resources.Load<Texture2D>($"Icons/Items/{spritePath}");
            if(texture != null)
            {
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                _icon = sprite;
            }
        }
    }
}
