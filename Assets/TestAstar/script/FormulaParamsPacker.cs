using UnityEngine;


/// <summary>
/// 行为参数
/// </summary>
public class FormulaParamsPacker
{

    /// <summary>
    /// 初始位置
    /// </summary>
    public Vector3 StartPos { get; set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 TargetPos { get; set; }

    /// <summary>
    /// 起始单位
    /// </summary>
    public GameObject StartObj { get; set; }

    /// <summary>
    /// 目标单位
    /// </summary>
    public GameObject TargetObj { get; set; }

    /// <summary>
    /// 单位父级
    /// </summary>
    public Transform ItemParent { get; set; }

    /// <summary>
    /// 缩放
    /// </summary>
    public Vector3 Scale {
        get { return scale; }
        set { scale = value; } }


    /// <summary>
    /// 缩放, 默认1,1,1
    /// </summary>
    private Vector3 scale = new Vector3(1, 1, 1);
}