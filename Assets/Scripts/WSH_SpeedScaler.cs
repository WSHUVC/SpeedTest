using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public enum WSH_SimulationSpeed
{
    x1, //1
    x10,
    x20,
    x30,
    x50,
    x100,   //100
}

[DefaultExecutionOrder(-100)]
public class WSH_SpeedScaler : MonoBehaviour
{
    public static WSH_SpeedScaler instance
    {
        get;
        private set;
    }

    protected void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public WSH_SimulationSpeed speed;

    public float timeScale;

    public float speedScale;
    public float myDeltaTime => Time.fixedDeltaTime * timeScale;
    public float totalScale => timeScale * speedScale;
    public float errorScaler => timeScale == 1f ? 1f : (1 + timeScale / (totalScale*2));

    private void Update()
    {
        switch (speed)
        {
            case WSH_SimulationSpeed.x1:
                timeScale = 1f;
                speedScale = 1f;
                break;

            case WSH_SimulationSpeed.x10:
                timeScale = 1f;
                speedScale = 10f;
                break;

            case WSH_SimulationSpeed.x20:
                timeScale = 2f;
                speedScale = 10f;
                break;

            case WSH_SimulationSpeed.x30:
                timeScale = 3f;
                speedScale = 10f;
                break;

            case WSH_SimulationSpeed.x50:
                timeScale = 5f;
                speedScale = 10f;
                break;

            case WSH_SimulationSpeed.x100:
                timeScale = 10f;
                speedScale = 10f;
                break;
        }

    }
}
