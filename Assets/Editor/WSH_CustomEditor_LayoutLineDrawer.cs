using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WSH_LayoutLineDrawer))]
public class WSH_CustomEditor_LayoutLineDrawer : Editor
{
    WSH_LayoutLineDrawer lld;

    private void OnEnable()
    {
        lld = (WSH_LayoutLineDrawer)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Draw Layout Line"))
        {
            lld.DrawLine();
        }
    }
}
