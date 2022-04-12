using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WSH_LineTrigger))]
public class WSH_CustomEditor_LineTrigger : Editor
{
    WSH_LineTrigger lt;

    private void OnEnable()
    {
        lt = (WSH_LineTrigger)target;   
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Create Connect Line"))
        {
            lt.DrawConnectedLine();
        }
    }
}
