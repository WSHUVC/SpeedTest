using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WSH_LayoutLoader))]
public class WSH_CustomEditor_LayoutLoader : Editor
{
    WSH_LayoutLoader s;
    private void OnEnable()
    {
        s = (WSH_LayoutLoader)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Load"))
        {
            s.Load();
        }
    }
}
