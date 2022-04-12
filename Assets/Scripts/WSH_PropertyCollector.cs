using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 사용법
/// 필요한 클래스에 상속한다.<br/>
/// 런타임에 작동한다.<br/>
/// 캐싱이 필요한 멤버변수의 set을만든다.<br/>
/// 이때 Set 메소드의 양식은 아래와 같다<br/>
/// Collect_하이어라키상의오브젝트이름<br/>
/// 하이어라키상의 오브젝트 이름의 양식은 아래와 같다.<br/>
/// Collect_이름<br/>
/// 사용 예시 클래스 : WSH_UI_Panel_SimulationSpeedController<br/>
/// 대신 오브젝트 이름과 멤버변수 이름이 동일해야함. <br/>
/// 변수 이름이 동일하지 않다면 그냥 스킵<br/>
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
