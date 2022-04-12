using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSH_ProcessPort : MonoBehaviour
{
    [SerializeField]
    string code;
    [SerializeField]
    string model;
    [SerializeField]
    Vector3 layoutPosition;
    [SerializeField]
    Vector3 layoutRotation;
    public void SetLayoutData(WSH_Layout_Port port)
    {
        code = port.code;
        model = port.model;
        var px = float.Parse(port.posX);
        var py = float.Parse(port.posY);
        var pz = float.Parse(port.posZ);
        layoutPosition = new Vector3(px, py, pz);
        var rx = float.Parse(port.rotX);
        var ry = float.Parse(port.rotY);
        var rz = float.Parse(port.rotZ);
        layoutRotation = new Vector3(rx, ry, rz);

        transform.position = layoutPosition;
        transform.Rotate(layoutRotation);
    }
}
