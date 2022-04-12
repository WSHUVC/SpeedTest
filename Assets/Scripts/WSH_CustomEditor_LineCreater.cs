using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
[ExecuteInEditMode]
public class WSH_CustomEditor_LineCreater : MonoBehaviour
{
    public static string Path_Prefab_Line = "Assets/Prefabs/Line.prefab";
    [MenuItem("GameObject/Draw Line")]
    public static GameObject DrawLine()
    {
        var line = AssetDatabase.LoadAssetAtPath(Path_Prefab_Line, typeof(GameObject));
        Instantiate(line);
        return line as GameObject;
    }
}
