﻿using System;

using UnityEngine;


/// <summary>
/// 目标选择单位数据
/// </summary>
public class Member : ISelectWeightData, IBaseMamber, IGraphical<Rectangle>
{
    // ----------------------------暴露接口-------------------------------

    /// <summary>
    /// 单位数据
    /// </summary>
    public VOBase MamberData { get; set; }

    /// <summary>
    /// 单位移动速度
    /// </summary>
    public float Speed
    {
        get { return MamberData.MoveSpeed; }
        set { MamberData.MoveSpeed = value; }
    }

    public float MaxHealth
    {
        get { return MamberData.TotalHp; }
        set { MamberData.TotalHp = value; }
    }

    public float Health
    {
        get { return MamberData.CurrentHP; }
        set { MamberData.CurrentHP = value; }
    }

    public float Atack
    {
        get { return MamberData.Attack1; }
        set { MamberData.Attack1 = value; }
    }

    public float Define
    {
        get { return MamberData.Defence; }
        set { MamberData.Defence = value; }
    }

    public float Diameter
    {
        get { return MamberData.SpaceSet; }
        set { MamberData.SpaceSet = value; }
    }

    public float ScanDiameter
    {
        get { return MamberData.AttackRange; }
        set { MamberData.AttackRange = value; }
    }

    /// <summary>
    /// 扫描范围类型
    /// </summary>
    public GraphicType ScanType
    {
        get { return scanType; }
        set { scanType = value; }
    }

    public float X
    {
        get { return x; }
        set { x = value; }
    }

    public float Y
    {
        get { return y; }
        set { y = value; }
    }

    /// <summary>
    /// 目标数量
    /// </summary>
    public int TargetCount { get; set; }

    /// <summary>
    /// 方向
    /// </summary>
    public Vector3 Direction { get; set; }

    // ------------------------------公有属性--------------------------------

    // TODO 持有数据对象


    public string Name = "";

    /// <summary>
    /// 是否飞行
    /// </summary>
    public bool IsAir { get; set; }

    /// <summary>
    /// 是否地面
    /// </summary>
    public bool IsSurface { get; set; }

    /// <summary>
    /// 是否建筑
    /// </summary>
    public bool IsBuild { get; set; }

    /// <summary>
    /// 第二级类型
    /// 区分步兵, 坦克, 载具, 火炮, 飞行棋
    /// </summary>
    public MemberItemType ItemType { get; set; }

    /// <summary>
    /// 是否隐形
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否嘲讽
    /// </summary>
    public bool IsTaunt { get; set; }


    // ----------------------------权重选择 Level1-----------------------------
    /// <summary>
    /// 选择地面单位权重
    /// </summary>
    public float SurfaceWeight { get; set; }

    /// <summary>
    /// 选择天空单位权重
    /// </summary>
    public float AirWeight { get; set; }

    /// <summary>
    /// 选择建筑权重
    /// </summary>
    public float BuildWeight { get; set; }


    // ----------------------------权重选择 Level2-----------------------------

    /// <summary>
    /// 选择坦克权重
    /// </summary>
    public float TankWeight { get; set; }

    /// <summary>
    /// 选择轻型载具权重
    /// </summary>
    public float LVWeight { get; set; }

    /// <summary>
    /// 选择火炮权重
    /// </summary>
    public float CannonWeight { get; set; }

    /// <summary>
    /// 选择飞行器权重
    /// </summary>
    public float AirCraftWeight { get; set; }

    /// <summary>
    /// 选择步兵权重
    /// </summary>
    public float SoldierWeight { get; set; }


    // ----------------------------权重选择 Level3-----------------------------
    /// <summary>
    /// 选择隐形单位权重
    /// </summary>
    public float HideWeight { get; set; }

    /// <summary>
    /// 选择嘲讽权重(这个值应该很大, 除非有反嘲讽效果的单位)
    /// </summary>
    public float TauntWeight { get; set; }


    // ----------------------------权重选择 Level4-----------------------------


    /// <summary>
    /// 低生命权重
    /// </summary>
    public float HealthMinWeight { get; set; }

    /// <summary>
    /// 高生命权重
    /// </summary>
    public float HealthMaxWeight { get; set; }

    /// <summary>
    /// 近位置权重
    /// </summary>
    public float DistanceMinWeight { get; set; }

    /// <summary>
    /// 远位置权重
    /// </summary>
    public float DistanceMaxWeight { get; set; }

    /// <summary>
    /// 角度权重
    /// </summary>
    public float AngleWeight { get; set; }



    /// <summary>
    /// 精准度
    /// </summary>
    public float Accuracy { get; set; }
    /// <summary>
    /// 散射半径
    /// </summary>
    public float ScatteringRadius { get; set; }


    // -------------------------------私有属性--------------------------------------

    //private float speed = 4f;

    //private int maxHealth = 100;

    //private int health = 100;

    //private int atack = 10;

    /// <summary>
    /// 受击半径
    /// </summary>
    //private int diameter = 1;

    /// <summary>
    /// 搜索半径
    /// </summary>
    //private int scanDiameter = 40;

    /// <summary>
    /// 攻击范围形状
    /// </summary>
    private GraphicType scanType = GraphicType.Circle;

    /// <summary>
    /// 目标数量
    /// </summary>
    //private int targetCount = 10;

    /// <summary>
    /// 当前位置x
    /// </summary>
    private float x = 0;

    /// <summary>
    /// 当前位置y
    /// </summary>
    private float y = 0;

    /// <summary>
    /// 目标点
    /// </summary>
    private Vector3 direction;


    private float healthWeight = 100;

    private float distanceWeight = 0.2f;

    private float angleWeight = 1;

    private float typeWeight;

    private float levelWeight;




    /// <summary>
    /// 单位矩形占位
    /// </summary>
    private Rectangle _rect = null;

    private float _hisX = 0;

    private float _hisY = 0;

    private int _hisDimeter = 0;
    


    // ------------------------------公有方法-------------------------------------

    /// <summary>
    /// 获得单位矩形占位
    /// </summary>
    /// <returns></returns>
    public Rectangle GetGraphical()
    {
        // 当rect不存在或位置大小发生变更时创建新Rect
        if (_hisDimeter != Diameter || Math.Abs(_hisX - X) > 0.0001f || Math.Abs(_hisY - Y) > 0.0001f || _rect == null)
        {
            //_hisX = X;
            //_hisY = Y;
            //_hisDimeter = Diameter;
            //_rect = new Rectangle(X, Y, Diameter, Diameter);
        }
        return _rect;
    }
}


/// <summary>
/// Mamber基础接口
/// </summary>
public interface IBaseMamber
{
    // ----------------------------------暴露接口--------------------------------------

    VOBase MamberData { get; }
    /// <summary>
    /// 是否飞行
    /// </summary>
    bool IsAir { get; set; }

    /// <summary>
    /// 是否地面
    /// </summary>
    bool IsSurface { get; set; }

    /// <summary>
    /// 是否建筑
    /// </summary>
    bool IsBuild { get; set; }

    /// <summary>
    /// 第二级类型
    /// 区分步兵, 坦克, 载具, 火炮, 飞行棋
    /// </summary>
    MemberItemType ItemType { get; set; }

    /// <summary>
    /// 是否隐形
    /// </summary>
    bool IsHide { get; set; }

    /// <summary>
    /// 是否嘲讽
    /// </summary>
    bool IsTaunt { get; set; }
    float Speed { get; set; }
    float MaxHealth { get; set; }
    float Health { get; set; }
    float Atack { get; set; }
    float Define { get; set; }
    float Diameter { get; set; }
    float ScanDiameter { get; set; }
    GraphicType ScanType { get; set; }
    int TargetCount { get; set; }
    float X { get; set; }
    float Y { get; set; }

    /// <summary>
    /// 方向
    /// </summary>
    Vector3 Direction { get; set; }

    /// <summary>
    /// 精准度
    /// </summary>
    float Accuracy { get; set; }
    /// <summary>
    /// 散射半径
    /// </summary>
    float ScatteringRadius { get; set; }
    // ----------------------------------暴露接口--------------------------------------
}
