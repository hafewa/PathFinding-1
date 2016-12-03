using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NavMash寻路对象
/// </summary>
public class NavMeshObj : MonoBehaviour
{

    /// <summary>
    /// 所属
    /// </summary>
    public IList<NavMeshObj> SubNavMashObjList = new List<NavMeshObj>();
    
    /// <summary>
    /// 寻路目标点
    /// </summary>
    public NavMeshTarget Target { get; set; }

    /// <summary>
    /// NavMeshAgent对象
    /// </summary>
    public NavMeshAgent NavAgent
    {
        get; set;
    }

    public void FindPath()
    {
        if (Target != null)
        {
            Debug.Log("开始寻路:" + Target.TargetPoint);
            TransDirToTarger(Target.TargetPoint);
        }
    }

    /// <summary>
    /// 是否已到达目标点
    /// </summary>
    /// <returns></returns>
    public bool Arrived()
    {
        return Target == null || NavAgent.transform.localPosition == Target.TargetPoint;
    }

    /// <summary>
    /// 转向 并向目标点移动
    /// </summary>
    /// <param name="direction">目标点</param>
    private void TransDirToTarger(Vector3 direction)
    {
        // 设置NavMash目标点
        if (NavAgent != null)
        {
            NavAgent.transform.LookAt(direction);
            NavAgent.SetDestination(direction);
        }
    }

}