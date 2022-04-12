using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSH_LayoutLineDrawer : MonoBehaviour
{
    WSH_Process[] process;
    [SerializeField]
    WSH_Line prefab_Line;

    public void DrawLine()
    {
        LoadLayoutData();
        var line = Instantiate(prefab_Line);

        line.startTransform.position = process[0].transform.position;
        var firstPorts = process[0].GetPorts;
        for(int i = 0; i < firstPorts.Length; ++i)
        {
            line.AddPoint(firstPorts[i].transform);
        }

        for(int i = 1; i < process.Length; ++i)
        {
            var ports = process[i].GetPorts;
            line.AddPoint(process[i].transform);
            for(int q = 0; q < ports.Length; ++q)
            {
                line.AddPoint(ports[q].transform);
            }
        }
        line.endTransform.position = line.startTransform.position;

    }

    public void LoadLayoutData()
    {
        process = GetComponentsInChildren<WSH_Process>();
    }
}
