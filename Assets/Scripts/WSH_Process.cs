using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WSH_Process : MonoBehaviour
{
    [SerializeField]
    string code;
    [SerializeField]
    string model;
    [SerializeField]
    float procTime;
    [SerializeField]
    float doorOpenTime;
    [SerializeField]
    float doorCloseTime;
    [SerializeField]
    Vector3 layoutPosition;
    [SerializeField]
    Vector3 layoutRotation;
    [SerializeField]
    WSH_ProcessPort[] ports;

    public WSH_ProcessPort[] GetPorts => ports;

    public void SetLayoutData(WSH_Layout data, WSH_ProcessPort[] ports)
    {
        code = data.code;
        model = data.model;
        procTime = float.Parse(data.procTime);
        doorOpenTime = float.Parse(data.doorOpenTime);
        doorCloseTime = float.Parse(data.doorCloseTime);
        var lpx = float.Parse(data.posX);
        var lpy = float.Parse(data.posY);
        var lpz = float.Parse(data.posZ);
        layoutPosition = new Vector3(lpx, lpy, lpz);
        var lrx = float.Parse(data.rotX);
        var lry = float.Parse(data.rotY);
        var lrz = float.Parse(data.rotZ);
        layoutRotation = new Vector3(lrx, lry, lrz);

        transform.position = layoutPosition;
        transform.Rotate(layoutRotation);
        this.ports = ports;

        foreach(var p in ports)
        {
            p.transform.SetParent(transform);
        }
    }
}
