using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

/// <summary>
/// 集群
/// </summary>
public class Formation {

    // 传入target , 位置, 方向, 计算转向等
    /// <summary>
    /// 计算单元位移与方向
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    /// <param name="nowPosition">当前单元位置</param>
    /// <param name="nowDirection">当前单元z轴方向</param>
    /// <param name="teamPositionList">team位置列表</param>
    /// <returns></returns>
    public Vector3 GetMovement(Vector3 targetPosition, Vector3 nowPosition, Vector3 nowDirection, Vector3[] teamPositionList)
    {
        
        return Vector3.zero;
    }


}