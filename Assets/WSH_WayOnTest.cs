using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WSH_WayOnTest : WSH_RobotManager
{
    public int testRobotCount;
    public WSH_Robot prefab_Robot;
    WSH_Robot currentRobot;

    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(SpawnRobot());
    }

    IEnumerator SpawnRobot()
    {
        var targets = lines[0].startToEnd;

        WSH_Struct_Order order = new WSH_Struct_Order();
        order.command = WSH_Flag_RobotCommand.Move;
        order.target = targets;

        int index = testRobotCount;
        while (index > 0)
        {
            if (currentRobot == null)
            {
                Spawn();
            }

            if(currentRobot.targetCompleteCounter > 2)
            {
                Spawn();
            }
            yield return null;
        }

        void Spawn()
        {
            var robot = SpawnRobot(index, prefab_Robot, targets[0].position);
            index--;
            robot.Order(order);
            if (currentRobot != null)
                robot.frontRobot = currentRobot.gameObject;
            currentRobot = robot;
        }
    }

}
