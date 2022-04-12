using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class WSH_TestManager : WSH_RobotManager
{
    [SerializeField]
    GameObject prefab_AGV;
    [SerializeField]
    GameObject prefab_Obstacle;

    [SerializeField]
    List<WSH_Line> unscaleLine;
    [SerializeField]
    List<WSH_Line> scaleLine;
    [SerializeField]
    List<WSH_Line> obstacleLine;

    [SerializeField]
    WSH_Line standardLine;
    [SerializeField]
    WSH_Line standardScaleLine;
    WSH_Robot unscaleRobot;
    WSH_Robot scaleRobot;

    public int unscaleCounter;
    public int scaleCounter;
    WSH_Struct_Order order;
    [SerializeField]
    WSH_SimulationSpeed speed;
    [SerializeField]
    float scaleRunTime;
    [SerializeField]
    float unscaleRunTime;
    [SerializeField]
    float startSpeed;

    private void Awake()
    {
        lines = FindObjectsOfType<WSH_Line>();
        scaler = FindObjectOfType<WSH_SpeedScaler>();
        order = new WSH_Struct_Order(WSH_Flag_RobotCommand.Move);
        int i = 0;
        foreach(var l in lines)
        {
             l.SetTriggerMask(1<<LayerMask.NameToLayer("AGV"));
            var start = l.startTransform;
            var agv = Instantiate(prefab_AGV).GetComponent<WSH_Robot>();
            //order.target = l.endTransform;
            agv.RegistManager(this);
            agv.gameObject.transform.position = start.position;
            agv.name = "AGV_" + (i++);
            order.target = l.startToEnd;
            agv.Order(order);

            if (unscaleLine.Contains(l))
            {
                agv.unscaled = true;
                if (l == standardLine)
                    unscaleRobot = agv;
                agv.SetSpeed(startSpeed);
            }
            else if (scaleLine.Contains(l))
            {
                if (l == standardScaleLine)
                    scaleRobot = agv;
                agv.SetSpeed(startSpeed);
            }
            else
                agv.SetSpeed(UnityEngine.Random.Range(0.1f, 1.8f));

            robots.Add(agv);
            readyRobotTable.Add(agv);
        }
    }

    public float scaleTime;
    public float unscaleTime;
    public float scaleMoveLength;
    public float unscaleMoveLength;
    public int fixedUpdateCount;
    private void FixedUpdate()
    {
        scaler.speed = speed;
        scaleTime = scaler.myDeltaTime;
        unscaleTime = Time.deltaTime;

        if (unscaleCounter < 1)
        {
            unscaleRunTime += unscaleTime;
            unscaleCounter = unscaleRobot.orderCompleteCounter;
            unscaleMoveLength = unscaleRobot.move;
        }

        if(scaleCounter< 1 * scaler.totalScale)
        {
            scaleRunTime += scaleTime * scaler.speedScale;
            scaleCounter = scaleRobot.orderCompleteCounter;
            scaleMoveLength = scaleRobot.move;
            fixedUpdateCount++;
        }
    }

    public override void ReceiveReport(WSH_Robot reporter, WSH_Flag_RobotReport msg)
    {
        base.ReceiveReport(reporter, msg);
        order.command = WSH_Flag_RobotCommand.Move;

        var lastLine = reporter.lastTargetPoint.parent.GetComponent<WSH_Line>();
        if (reporter.MyLine != lastLine)
        {
            reporter.OnLine(lastLine);
        }

        order.target = reporter.lastTargetPoint == reporter.MyLine.startTransform ?
                        reporter.MyLine.startToEnd : reporter.MyLine.endToStart;
        reporter.Order(order);
    }
}
