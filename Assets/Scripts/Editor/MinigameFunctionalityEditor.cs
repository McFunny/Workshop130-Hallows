using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MinigameFunctionality), true)]
[CanEditMultipleObjects]
public class MinigameFunctionalityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MinigameFunctionality minigameFunctionality = (MinigameFunctionality)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Set Size"))
        {
            minigameFunctionality.SetSize();
        }
    }
}
