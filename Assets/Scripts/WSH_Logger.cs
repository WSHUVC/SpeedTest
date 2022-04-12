using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSH_Logger : MonoBehaviour
{
    public static WSH_Logger instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if(instance!=null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public static void Log(string msg)
    {
        //Debug.Log(msg);
    }

    public static void Log(string msg, params object[] values)
    {
    }
}
