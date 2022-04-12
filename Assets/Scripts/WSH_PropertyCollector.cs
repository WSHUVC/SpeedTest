using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ����
/// �ʿ��� Ŭ������ ����Ѵ�.<br/>
/// ��Ÿ�ӿ� �۵��Ѵ�.<br/>
/// ĳ���� �ʿ��� ��������� set�������.<br/>
/// �̶� Set �޼ҵ��� ����� �Ʒ��� ����<br/>
/// Collect_���̾��Ű���ǿ�����Ʈ�̸�<br/>
/// ���̾��Ű���� ������Ʈ �̸��� ����� �Ʒ��� ����.<br/>
/// Collect_�̸�<br/>
/// ��� ���� Ŭ���� : WSH_UI_Panel_SimulationSpeedController<br/>
/// ��� ������Ʈ �̸��� ������� �̸��� �����ؾ���. <br/>
/// ���� �̸��� �������� �ʴٸ� �׳� ��ŵ<br/>
/// </summary>
[DefaultExecutionOrder(-200)]
public class WSH_PropertyCollector : MonoBehaviour
{
    string collectTag = "Collect";
    protected virtual void Awake()
    {
        var typeData = this.GetType();
        var allProperties = typeData.GetProperties().Where(s => s.Name.Split('_')[0] == collectTag);
        var childs = GetComponentsInChildren<UIBehaviour>().Where(c => c.name.Split('_')[0] == collectTag);

        foreach (var p in allProperties)
        {
            var result = childs.Where(c => c.name == p.Name).ToArray();
            if (result == null || result.Length == 0)
                continue;
            result = new UIBehaviour[] { result[0] };
            p.SetValue(this, result);
        }
    }
}
