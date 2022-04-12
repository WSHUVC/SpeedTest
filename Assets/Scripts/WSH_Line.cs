using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum WSH_Enum_LineType
{
    Straight,
    Arc
}

[DefaultExecutionOrder(-2)]
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class WSH_Line : MonoBehaviour, IWSH_LineTriggerReceiver
{
    #region Variables
    LineRenderer line;

    [Header("Setting Values")]
    [SerializeField]
    WSH_Enum_LineType type_Line;
    [Range(20, 100)]
    [SerializeField]
    int detail = 20;
    [Range(1, 50)]
    [SerializeField]
    float radius = 1;

    [Range(0f, 1f)]
    [SerializeField]
    float lineSize = 0.2f;

    [SerializeField]
    bool clockwise;
    [SerializeField]
    float startAngle;
    [SerializeField]
    float degrees = 90;

    [SerializeField]
    LayerMask lineTriggerMask;
    public void SetTriggerMask(LayerMask mask) => lineTriggerMask = mask;

    List<RaycastHit> hitInfo = new List<RaycastHit>();
    List<Transform> stopOverPoints = new List<Transform>();
    HashSet<WSH_Robot> onLineRobotTable = new HashSet<WSH_Robot>();
    HashSet<WSH_LineTrigger> triggers = new HashSet<WSH_LineTrigger>();
    #endregion

    #region Properties
    public static string endTransformName
    {
        get => "EndPosition";
        private set { }
    }

    public static string startTransformName
    {
        get => "StartPosition";
        private set { }
    }
    public static string centerTransformName
    {
        get => "CenterPosition";
        private set { }
    }
    public static string stopOverTransformName
    {
        get => "StopOverPosition";
        private set { }
    }
    public Transform startTransform
    {
        get;
        private set;
    }

    public Transform endTransform
    {
        get;
        private set;
    }

    public Transform centerTransform
    {
        get;
        private set;
    }
    public List<Transform> startToEnd
    {
        get
        {
            var result = new List<Transform>();
            result.Add(startTransform);
            result.AddRange(stopOverPoints);
            result.Add(endTransform);
            return result;
        }
    }
    public List<Transform> endToStart
    {
        get
        {
            var result = new List<Transform>();
            result.Add(endTransform);
            var temp = new List<Transform>();
            temp.AddRange(stopOverPoints);
            temp.Reverse();
            result.AddRange(temp);
            result.Add(startTransform);
            return result;
        }
    }
    public Vector3 centerPos
    {
        get
        {
            if (line.positionCount <= 2)
                return startPos - (startPos - endPos) * 0.5f;

            Vector3 result = startPos;
            var half = lineLength * 0.5f;
            float temp = 0;

            var pos = new Vector3[line.positionCount];
            line.GetPositions(pos);

            for (int i = 0; i < pos.Length - 1; ++i)
            {
                var p1 = pos[i];
                var p2 = pos[i + 1];
                var dis = Vector3.Distance(p1, p2);
                temp += dis;
                //절반을 넘어가는 시점에서
                if (temp>=half)
                {
                    //넘어간 만큼을 구한다
                    //총거리가 10, 절반이 5, 포인트가 0, 3, 6, 10 에 찍혔다고 가정.
                    //0번 3번 길이 = 3 계산거리 3
                    //3번 6번 길이 = 3 계산거리 6
                    //절반을 넘어갔으므로 3과 6 사이에 중점이 존재
                    //따라서 6번에서 넘어간 만큼을 빼주면 됨.
                    temp -= half;
                    var dir = p2 - p1;
                    var remain = Mathf.Abs(temp) / dis;
                    dir *= remain;
                    result = pos[i+1] - dir;
                    break;
                }
            }

            return result;
        }
    }
    public Vector3 startPos => startTransform.position;
    public Vector3 endPos => endTransform.position;

    float lineLength
    {
        get
        {
            float result = 0f;
            Vector3[] pos = new Vector3[line.positionCount];
            line.GetPositions(pos);
            for (int i = 0; i < pos.Length - 1; ++i)
            {
                result += Vector3.Distance(pos[i], pos[i + 1]);
            }
            return result;
        }
    }
    public float arcLength
    {
        get
        {
            float total = 0f;
            for (int i = 0; i < line.positionCount - 1; ++i)
            {
                var p1 = line.GetPosition(i);
                var p2 = line.GetPosition(i + 1);
                var pDis = Vector3.Distance(p1, p2);
                total += pDis;
            }
            return total;
        }
    }
    #endregion

    #region UNITY Methods
    private void Awake()
    {
        if (endTransform == null)
            endTransform = transform.Find(endTransformName);
        if (centerTransform == null)
            centerTransform = transform.Find(centerTransformName);
        if (startTransform == null)
            startTransform = transform.Find(startTransformName);
        if (line == null)
            line = GetComponent<LineRenderer>();

        stopOverPoints = transform.GetComponentsInChildren<Transform>().Where(t => t.name.Split('_')[0] == stopOverTransformName).ToList();

        line.useWorldSpace = false;
    }

    private void Update()
    {
        Draw();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// 경유지점을 하나 추가한다.<br/>
    /// </summary>
    public void AddPoint()
    {
        NewPoint().transform.position = 
            stopOverPoints.Count == 0 ? centerPos : stopOverPoints[stopOverPoints.Count - 1].position;
    }

    public void AddPoint(Transform pos)
    {
        var p = NewPoint();
        p.transform.position = pos.position;
        p.GetComponent<WSH_LineTrigger>().Connect(pos);
    }

    GameObject NewPoint()
    {
        var p = new GameObject();
        p.transform.SetParent(transform);
        p.name = stopOverTransformName + "_" + stopOverPoints.Count;
        p.AddComponent<WSH_LineTrigger>().SetLayer(lineTriggerMask);
        stopOverPoints.Add(p.transform);
        return p;
    }

    public void Online(WSH_Robot robot)
    {
        if (onLineRobotTable.Add(robot))
        {
            robot.OnLine(this);
            WSH_Logger.Log("OnLine : " + name + ", " + robot.name);
        }
    }
    public void OffLine(WSH_Robot robot)
    {
        if (onLineRobotTable.Remove(robot))
        {
            robot.OffLine();
            WSH_Logger.Log("OffLine : " + name + ", " + robot.name);
        }
    }
    public void TriggerEnterReceive(WSH_LineTrigger reporter, WSH_Robot obj)
    {
        LineCast(obj);
    }
    public void TriggerExitReceive(WSH_LineTrigger reporter, WSH_Robot obj)
    {
        LineCast(obj);
    }
    public bool isLineTrigger(WSH_LineTrigger trig) => triggers.Contains(trig);

    public void Setting(WSH_LineTrigger trig)
    {
        triggers.Add(trig);
        trig.SetLayer(lineTriggerMask);
    }

    public void Connect(WSH_LineTrigger trig)
    {
        connectTrigger= trig;
    }

    public void Draw(WSH_LineTrigger start, Vector3 end)
    {
        startTransform.position = start.transform.position;
        endTransform.position = end;
    }

    public void Draw(Vector3 start, Vector3 end)
    {
        startTransform.position = start;
        endTransform.position = end;
    }
    #endregion

    #region Private Methods
    [SerializeField]
    WSH_LineTrigger connectTrigger;

    void Draw()
    {
        line.endWidth = lineSize;
        line.startWidth = lineSize;
        switch (type_Line)
        {
            case WSH_Enum_LineType.Straight:
                DrawLine();
                break;
            case WSH_Enum_LineType.Arc:
                DrawArc();
                break;
        }
    }

    void DrawLine()
    {
        line.useWorldSpace = true;
        line.positionCount = startToEnd.Count;
        
        for(int i = 0; i < line.positionCount; ++i)
        {
            line.SetPosition(i, startToEnd[i].position);
        }

        if (connectTrigger != null)
            startTransform.position = connectTrigger.transform.position;
        //centerTransform.localPosition = startTransform.localPosition;
        centerTransform.position = centerPos;
    }
    void DrawArc()
    {
        line.useWorldSpace = false;
        float angle = 0;
        var segments = detail;
        var pointCount = segments + 1;
        var points = new Vector3[pointCount + 1];

        line.endWidth = lineSize;
        line.startWidth = lineSize;
        line.positionCount = pointCount;

        for (int i = 0; i <= pointCount; i++)
        {
            Vector3 rotatepoint = GetPathAnglePos(angle);
            points[i] = rotatepoint;
            angle += (degrees / segments);
        }

        line.SetPositions(points);
        endTransform.localPosition = line.GetPosition(pointCount-1);
        startTransform.localPosition = Vector3.zero;
        startTransform.transform.localRotation = Quaternion.identity;
        centerTransform.localPosition = line.GetPosition(line.positionCount / 2);
    }

    Vector3 GetPathAnglePos(float angle)
    {
        Vector3 startvector = GetCirclePos(0);
        Vector3 circlepos = GetCirclePos(angle);
        Vector3 point = circlepos - startvector + startTransform.localPosition;
        if (!clockwise)
            point = new Vector3(point.x, -point.y, point.z);
        // rotate Point by startangle
        Vector3 rotatepoint = RotatePointAroundPivot(point, startTransform.localPosition, new Vector3(90, 0, -startAngle));
        return rotatepoint;
    }
    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
    {
        Vector3 dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angle) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }

    Vector3 GetCirclePos(float angle)
    {
        float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
        float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
        float z = 0;
        var vec = new Vector3(x, y, z);
        return vec;
    }

    void LineCast(WSH_Robot obj)
    {
        hitInfo.Clear();

        for (int i = 0; i < line.positionCount - 1; ++i)
        {
            var sp = line.GetPosition(i);
            var ep = line.GetPosition(i + 1);
            var hits = Physics.RaycastAll(sp, ep - sp, Vector3.Distance(sp, ep), lineTriggerMask);
            hitInfo.AddRange(hits);
        }

        foreach (var h in hitInfo)
        {
            var robot = h.collider.GetComponent<WSH_Robot>();
            if (robot == null)
                continue;
            Online(robot);
        }

        if (!onLineRobotTable.Contains(obj))
            OffLine(obj);
    }
    #endregion
}
