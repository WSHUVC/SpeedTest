using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WSH_Flag_RobotReport
{
    MoveEnd,    //목적지 도달
    Move,       //현재 이동중
}

public class WSH_RobotManager : MonoBehaviour
{
    public WSH_SpeedScaler scaler;
    public List<WSH_Robot> robots;
    public HashSet<WSH_Robot> readyRobotTable = new HashSet<WSH_Robot>();
    public HashSet<WSH_Robot> movingRobotTable = new HashSet<WSH_Robot>();
    public WSH_Line[] lines;
    public Dictionary<WSH_Robot, WSH_Line> robotPerLineTable = new Dictionary<WSH_Robot, WSH_Line>();

    protected virtual void Awake()
    {
        scaler = FindObjectOfType<WSH_SpeedScaler>();
        lines = FindObjectsOfType<WSH_Line>();
    }
    protected WSH_Robot SpawnRobot(int index, WSH_Robot prefab, Vector3 pos)
    {
        var robot = Instantiate(prefab);
        robots.Add(robot);
        robot.RegistManager(this);
        robot.name = "AGV_" + (index++);
        robot.gameObject.transform.position = pos;
        robot.SetSpeed(Random.Range(0.1f, 1.8f));
        return robot;
    }

    protected virtual void OnlineRobot(WSH_Robot robot, WSH_Line line)
    {
        robotPerLineTable.Add(robot, line);
    }
    
    protected virtual void ReceiveOrder()
    {
    }

    protected virtual void SendOrder()
    {
    }

    protected virtual void SendReport()
    {

    }

    public virtual void ReceiveReport(WSH_Robot reporter, WSH_Flag_RobotReport msg)
    {
        switch (msg)
        {
            case WSH_Flag_RobotReport.MoveEnd:
                readyRobotTable.Add(reporter);
                movingRobotTable.Remove(reporter);
                break;

            case WSH_Flag_RobotReport.Move:
                movingRobotTable.Add(reporter);
                readyRobotTable.Remove(reporter);
                break;
        }
    }
}
