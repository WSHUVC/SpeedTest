using UnityEngine;
using System.Collections.Generic;

public enum WSH_Flag_RobotCommand
{
    Move,
    Stop,
    Ready,
}

public enum WSH_Flag_RobotState
{
    Stop,
    Ready,
    Move,
    Rotate,
    None,
}

public struct WSH_Struct_Order
{
    public WSH_Flag_RobotCommand command;
    public List<Transform> target;

    public WSH_Struct_Order(WSH_Flag_RobotCommand com, List<Transform> t = null)
    {
        command = com;
        target = t;
    }
}

public class WSH_Robot : MonoBehaviour
{
    #region Variables
    [Header("Spec")]
    [SerializeField]
    float stopErrorRange = 0.01f;   //제동 오차 거리
    [SerializeField]
    float rotateErrorRange = 1f;    //회전 오차 각도
    [SerializeField]
    float speed;
    [SerializeField]
    float turnSpeed;

    [Header("AI")]
    [SerializeField]
    WSH_Flag_RobotState flag_CurrentState;
    [SerializeField]
    WSH_RobotManager myManager;
    [SerializeField]
    WSH_Line myLine;
    [SerializeField]
    Transform currentTargetPoint;
    public Transform lastTargetPoint;
    public int targetCompleteCounter = 0;
    public int orderCompleteCounter = 0;
    Queue<Transform> targetPointQueue = new Queue<Transform>();

    bool targetSet;
    float turnTimer;
    bool rotateCheck;
    public bool unscaled;
    WSH_Struct_Order selfOrder;
    #endregion

    #region Properties
    public WSH_Line MyLine => myLine;
    public void OffLine() => myLine = null;
    public void OnLine(WSH_Line line) => myLine = line;
    public void SetSpeed(float value) => speed = value;
    public void RegistManager(WSH_RobotManager manager) => myManager = manager;
    Vector3 targetDirection => (currentTargetPoint.position - transform.position).normalized;
    float scaleSpeed => speed * myManager.scaler.speedScale * myManager.scaler.myDeltaTime;// * myManager.scaler.errorScaler;
    float speedPerTime => unscaled ? speed * Time.deltaTime : scaleSpeed;
    #endregion

    private void FixedUpdate()
    {
        Action();
    }

    void AddTargetPoint(List<Transform> t)
    {
        foreach (var i in t)
            targetPointQueue.Enqueue(i);
    }
    public void Order(WSH_Struct_Order order)
    {
        WSH_Logger.Log("Order Receive : " + name + ", " + order.command);
        switch (order.command)
        {
            case WSH_Flag_RobotCommand.Move:
                turnTimer = 0f;
                AddTargetPoint(order.target);
                flag_CurrentState = WSH_Flag_RobotState.Rotate;
                break;

            case WSH_Flag_RobotCommand.Stop:
                flag_CurrentState = WSH_Flag_RobotState.Stop;
                break;

            case WSH_Flag_RobotCommand.Ready:
                flag_CurrentState = WSH_Flag_RobotState.Ready;
                break;
        }
    }
    void SendReport(WSH_Flag_RobotReport msg)
    {
        myManager.ReceiveReport(this, msg);
    }

    #region RobotAction
    void Action()
    {
        switch (flag_CurrentState)
        {
            case WSH_Flag_RobotState.Move:
                Move();
                break;

            case WSH_Flag_RobotState.Rotate:
                Rotate();
                break;

            case WSH_Flag_RobotState.Ready:
                Ready();
                break;

            case WSH_Flag_RobotState.Stop:
                Stop();
                break;

            default:
                WSH_Logger.Log("ERROR : " + name + ", StateError");
                break;
        }

    }

    void Stop()
    {
        speed = 0f;
    }

    void Ready()
    {
    }

    void Move()
    {
        if (currentTargetPoint == null)
        {
            WSH_Logger.Log("TargetPoint NULL ERROR : " + name);
            return;
        }

        if (ObstacleCheck())
            return;

        if (ReachCheck())
        {
            rotateCheck = false;
            if (SetNextTarget())
            {
                targetSet = true;
                targetCompleteCounter++;
                flag_CurrentState = WSH_Flag_RobotState.Rotate;
            }
            else
            {
                targetSet = false;
                orderCompleteCounter++;
                flag_CurrentState = WSH_Flag_RobotState.Ready;
                SendReport(WSH_Flag_RobotReport.MoveEnd);
                WSH_Logger.Log("TargetPoint Reached : " + name);
            }
            return;
        }
        var temp = transform.position;
        transform.position += targetDirection * speedPerTime;
        moveLength += Vector3.Distance(temp, transform.position);
    }
    //이동한거리
    public float moveLength;

    //이동하기 전 호출됩니다.
    bool ReachCheck()
    {
        var distance = Vector3.Distance(transform.position, currentTargetPoint.position);
        //1차 거리 계산.
        //stopRange의 경우 제동 오차 범위입니다.
        if (Mathf.Abs(distance) <= stopErrorRange)
            return true;

        //현재 로봇이 목표지점을 초과해서 이동했는지를 판별합니다,
        //로봇은 이동하기전 자신의 forward를 목표지점을 향하도록 되어있습니다.
        var dot = Vector3.Dot(transform.forward, targetDirection);
        if (dot < 0)
        {
            //초과한경우 로봇의 위치를 강제로 목표지점에 맞춰줍니다.
            transform.position = currentTargetPoint.position;
            return true;
        }
        return false;
    }

    void Rotate()
    {
        if (ObstacleCheck())
            return;

        if (!targetSet)
        {
            SetNextTarget();
            targetSet = true;
        }

        //if(rotateCheck)
        //{
        //    transform.forward = targetDirection;
        //    flag_CurrentState = WSH_Flag_RobotState.Move;
        //    return;
        //}

        turnTimer += speedPerTime;
        if (turnTimer < turnSpeed)
            return;

        rotateCheck = true;
        transform.forward = targetDirection;
        flag_CurrentState = WSH_Flag_RobotState.Move;
        WSH_Logger.Log("Forward Setting Complete: " + name);
    }

    bool SetNextTarget()
    {
        if (targetPointQueue.Count == 0)
        {
            WSH_Logger.Log("Target Queue is Empty.");
            return false;
        }
        currentTargetPoint = targetPointQueue.Dequeue();
        lastTargetPoint = currentTargetPoint;
        WSH_Logger.Log("Set Next Target Line : " + myLine + ", Pos : " + currentTargetPoint);
        return true;
    }

    [SerializeField]
    float obstacleCheckRange => speed*2;
    Ray ray0;
    Ray ray1;
    Ray ray2;
    RaycastHit hit0;
    RaycastHit hit1;
    RaycastHit hit2;

    float obstacleTimer;
    float ignoreTime;

    public GameObject frontRobot;

    bool ObstacleCheck()
    {
        var offset = transform.position;
        offset.y += 0.2f;
        ray0 = new Ray(offset, transform.forward);
        ray1 = new Ray(offset, transform.forward+transform.right);
        ray2 = new Ray(offset, transform.forward-transform.right);

        bool result = false;

        if (Physics.Raycast(ray0, out hit0, obstacleCheckRange) && hit0.collider.gameObject == frontRobot)
        {
            if (hit0.collider.gameObject.layer == LayerMask.NameToLayer("AGV"))
                result = true;
        }

        if (Physics.Raycast(ray1, out hit1, obstacleCheckRange) && hit1.collider.gameObject == frontRobot)
        {
            if (hit1.collider.gameObject.layer == LayerMask.NameToLayer("AGV"))
                result = true;
        }

        if (Physics.Raycast(ray2, out hit2, obstacleCheckRange) && hit2.collider.gameObject == frontRobot)
        {
            if (hit2.collider.gameObject.layer == LayerMask.NameToLayer("AGV"))
                result = true;
        }
        //obstacleTimer += Time.deltaTime;

        //if (obstacleTimer >= 5f)
        //{
        //    obstacleTimer = 0f;
        //    result = false;
        //}
        return result;
    }
    #endregion
    
}
