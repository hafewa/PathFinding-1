using UnityEngine;


/// <summary>
/// 行为参数
/// </summary>
public class FormulaParamsPacker
{

    /// <summary>
    /// 初始位置
    /// </summary>
    public Vector3 StartPos { get; private set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 TargetPos { get; private set; }

    /// <summary>
    /// 起始单位
    /// </summary>
    public GameObject StartObj { get; private set; }

    /// <summary>
    /// 目标单位
    /// </summary>
    public GameObject TargetObj { get; private set; }

    /// <summary>
    /// 单位父级
    /// </summary>
    public Transform ItemParent { get; private set; }

    /// <summary>
    /// 缩放
    /// </summary>
    public Vector3 Scale { get; private set; }
}