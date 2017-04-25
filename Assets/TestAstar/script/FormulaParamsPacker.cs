﻿using UnityEngine;


/// <summary>
/// 行为参数
/// </summary>
public class FormulaParamsPacker
{
    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillNum { get; set; }

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
        set { scale = value; }
    }


    // TODO 技能数据如何放入这里?

    // TODO 技能Level

    // --------------------技能实际数据---------------------

    /// <summary>
    /// 检测范围形状
    /// </summary>
    public GraphicType GType { get; set; }

    /// <summary>
    /// 目标数量如果为-1则选择范围内所有单位
    /// </summary>
    public int TargetCount { get; set; }

    /// <summary>
    /// 子集技能ID, 如果为-1则没有子集技能
    /// </summary>
    public int NextSkillNum { get; set; }




    
    /// <summary>
    /// 缩放, 默认1,1,1
    /// </summary>
    private Vector3 scale = new Vector3(1, 1, 1);
}

/// <summary>
/// 图形类型
/// </summary>
public enum GraphicType
{
    Rect = 0,   // 举行
    Circle,     // 圆
    Sector      // 扇形
}