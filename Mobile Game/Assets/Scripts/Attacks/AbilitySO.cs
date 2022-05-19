using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;

[System.Serializable]
public class AbilitySO : ScriptableObject
{
    [Header("Ability Data")]
    public string abilityName;

    public AbilitySO()
    {
        abilityName = "Default";
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if(abilityName == "Default")
        {
            abilityName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(this.GetInstanceID()));
        }
    }
#endif
}

[System.Serializable]
public class Ability
{
    public virtual string thisName { get; protected set; }

    public Ability()
    {
        thisName = "Default";
    }

    public Ability(AbilitySO referenceAbility)
    {
        thisName = referenceAbility.abilityName;
    }

    public Ability(Ability referenceAbility)
    {
        thisName = referenceAbility.thisName;
    }
}




