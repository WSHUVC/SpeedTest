using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IWSH_LineTriggerReceiver
{
    public void TriggerEnterReceive(WSH_LineTrigger reporter, WSH_Robot obj);
    public void TriggerExitReceive(WSH_LineTrigger reporter, WSH_Robot obj);

    /// <summary>
    /// 마스킹할 레이어 설정
    /// </summary>
    /// <param name="trig"></param>
    public void Setting(WSH_LineTrigger trig);
}
[ExecuteInEditMode]
[RequireComponent(typeof(BoxCollider))]
public class WSH_LineTrigger : MonoBehaviour
{
    public enum LineTriggerType
    {
        IN,
        OUT,
        NONE,
    }

    [SerializeField]
    public LineTriggerType triggerType;
    [SerializeField]
    LayerMask mask;
    [SerializeField]
    List<WSH_Line> connectLineList = new List<WSH_Line>();
    public void SetLayer(LayerMask value) => mask = value;
    IWSH_LineTriggerReceiver[] receivers;
    private void Awake()
    {
        receivers = GetComponentsInParent<IWSH_LineTriggerReceiver>();
        for(int i = 0; i < receivers.Length; ++i)
        {
            receivers[i].Setting(this);
        }

        if (name == WSH_Line.startTransformName)
            triggerType = LineTriggerType.IN;
        else if (name == WSH_Line.endTransformName)
            triggerType = LineTriggerType.OUT;
        else
            triggerType = LineTriggerType.NONE;
    }

    private void Update()
    {
        if (connected != null)
            transform.position = connected.position;
    }

    Transform connected;
    public void Connect(Transform trs)
    {
        connected = trs;
    }

    private void OnTriggerEnter(Collider other)
    {
        TriggerEvent(other);
    }

    private void OnTriggerExit(Collider other)
    {
        TriggerEvent(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TriggerEvent(other);
    }

    void TriggerEvent(Collider other)
    {
        if (((1 << other.gameObject.layer) & mask) == 0)
            return;

        var robot = other.GetComponent<WSH_Robot>();
        if (robot == null)
            return;

        for (int i = 0; i < receivers.Length; ++i)
        {
            receivers[i].TriggerExitReceive(this, robot);
        }
    }

    public void DrawConnectedLine()
    {
        var obj = WSH_CustomEditor_LineCreater.DrawLine();
        var line = obj.GetComponent<WSH_Line>();
        line.SetTriggerMask(mask);
        line.Setting(this);
        line.Connect(this);
        connectLineList.Add(line);
    }
}
