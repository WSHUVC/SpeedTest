using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSH_CamMove : MonoBehaviour
{
    float h;
    float v;
    void Update()
    {
        h = 0;
        v = 0;
        if (Input.GetKey(KeyCode.A))
            h = -1;
        if (Input.GetKey(KeyCode.D))
            h = 1;
        if (Input.GetKey(KeyCode.W))
            v = 1;
        if (Input.GetKey(KeyCode.S))
            v = -1;

        transform.position += new Vector3(h, 0, v)*2 * Time.deltaTime;
    }
}
