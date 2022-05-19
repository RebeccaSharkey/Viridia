using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(AttackSO))]
public class AttackSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AttackSO attackSO = (AttackSO)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Create/Update Training Script"))
        {
            attackSO.BuildTrainingScript();
        }
    }
}
#endif
