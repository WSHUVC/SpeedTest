using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WSH_Line))]
public class WSH_CustomEditor_Line : Editor
{
    private WSH_Line root;

    private void OnEnable()
    {
        root = (WSH_Line)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Add Point"))
        {
            root.AddPoint();
        }
    }
}

