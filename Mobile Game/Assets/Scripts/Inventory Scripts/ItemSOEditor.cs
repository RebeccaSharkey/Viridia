using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ItemSO)), CanEditMultipleObjects]
public class ItemSOEditor : Editor
{
    //Base Values
    private SerializedProperty
        _itemName,
        _itemIcon,
        _itemType,
        _itemLevel;

    //Locket Variables
    private SerializedProperty
        _locketLevel;

    //Potion Variables
    private SerializedProperty
        _potionType,
        _potionIncriment,
        _potionBoost;

    private SerializedProperty
        _attack,
        _powerUp,
        _cost;

    void OnEnable()
    {
        _itemName = serializedObject.FindProperty("itemName");
        _itemIcon = serializedObject.FindProperty("itemIcon");
        _itemType = serializedObject.FindProperty("itemType");
        _itemLevel = serializedObject.FindProperty("itemLevel");

        _locketLevel = serializedObject.FindProperty("locketLevel");

        _potionType = serializedObject.FindProperty("potionType");
        _potionIncriment = serializedObject.FindProperty("potionIncriment");
        _potionBoost = serializedObject.FindProperty("potionBoost");

        _attack = serializedObject.FindProperty("attack");
        _powerUp = serializedObject.FindProperty("powerUp");
        _cost = serializedObject.FindProperty("cost");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(_itemName, new GUIContent("Item Name"));
        EditorGUILayout.PropertyField(_itemIcon, new GUIContent("Item Icon"));
        EditorGUILayout.PropertyField(_itemLevel, new GUIContent("Item Level"));

        EditorGUILayout.PropertyField(_itemType);
        ItemType it = (ItemType)_itemType.enumValueIndex;

        switch(it)
        {
            case ItemType.Default:
                break;
            case ItemType.Locket:
                EditorGUILayout.PropertyField(_locketLevel, new GUIContent("Locket Level"));
                break;
            case ItemType.Potion:
                EditorGUILayout.PropertyField(_potionType);
                PotionType pt = (PotionType)_potionType.enumValueIndex;
                switch(pt)
                {
                    case PotionType.Health:
                    case PotionType.Stamina:
                    case PotionType.Magic:
                        EditorGUILayout.PropertyField(_potionIncriment, new GUIContent("Potion Incriment"));
                        break;
                    case PotionType.AttackBoost:
                    case PotionType.MagicBoost:
                        EditorGUILayout.Slider(_potionBoost,0f, 1f, new GUIContent("Potion Boost"));
                        break;
                }
                break;
            case ItemType.MonsterRemains:
                break;
            case ItemType.TrainingScripts:
                EditorGUILayout.PropertyField(_attack, new GUIContent("Attack"));
                EditorGUILayout.PropertyField(_powerUp, new GUIContent("Power Up"));
                EditorGUILayout.PropertyField(_cost, new GUIContent("Cost"));
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
#endif