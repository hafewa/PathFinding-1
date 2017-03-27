﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 目标选择器
/// </summary>
public class TargetSelecter
{
    // 耦合一些功能
    // 圆形,矩形,扇形, 多图形对单图形碰撞
    public static IList<T> SearchTarget<T>(T mine, IList<T> list) where T : ISelectWeightData
    {
        List<T> result = null;

        var value = Search(mine, list, (arg1, arg2) =>
        {
            // 查询对象
            // 根据对象类型
            // 获取对象图形类型
            // 由数值控制检测对象
            // 数值在对象T里
            
            return false; 
            
        });

        return result;
    }

    
    /// <summary>
    /// 查找符合条件的对向列表
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="mine">当前对象</param>
    /// <param name="list">目标对象列表</param>
    /// <param name="func">判断方法</param>
    /// <returns>符合条件的对象列表</returns>
    // 数据符合合并类型, 选择具体功能外抛
    private static IList<T> Search<T>(T mine, IList<T> list, Func<T, T, bool> func) where T : ISelectWeightData
    {
        List<T> result = null;
        if (list != null && func != null && mine != null)
        {
            result = new List<T>();
            T item;
            for (var i = 0; i < list.Count; i++)
            {
                item = list[i];
                if (func(mine, item))
                {
                    result.Add(item);
                }
            }
        }
        return result;
    }
}


/// <summary>
/// 目标列表
/// </summary>
/// <typeparam name="T"></typeparam>
public class TargetList<T> where T : IGraphical<Rectangle>
{
    /// <summary>
    /// 返回全引用列表
    /// </summary>
    public IList<T> List { get { return list; } } 

    /// <summary>
    /// 返回四叉树列表
    /// </summary>
    public QuadTree<T> QuadTree { get { return quadTree; } }

    /// <summary>
    /// 地图信息
    /// </summary>
    public MapInfo<T> MapInfo
    {
        get { return mapinfo; }
        set { mapinfo = value; }
    }


    /// <summary>
    /// 目标总列表
    /// </summary>
    private IList<T> list = null;

    /// <summary>
    /// 四叉树
    /// </summary>
    private QuadTree<T> quadTree = null;

    /// <summary>
    /// 地图信息
    /// </summary>
    private MapInfo<T> mapinfo = null;

    /// <summary>
    /// 单位格子宽度
    /// </summary>
    private int unitWidht;


    /// <summary>
    /// 创建目标列表
    /// </summary>
    /// <param name="x">地图位置x</param>
    /// <param name="y">地图位置y</param>
    /// <param name="width">地图宽度</param>
    /// <param name="height">地图高度</param>
    public TargetList(float x, float y, int width, int height, int unitWidht)
    {
        var mapRect = new Rectangle(x, y, width * unitWidht, height * unitWidht);
        this.unitWidht = unitWidht;
        quadTree = new QuadTree<T>(0, mapRect);
        list = new List<T>();
    }

    /// <summary>
    /// 添加单元
    /// </summary>
    /// <param name="t">单元对象, 类型T</param>
    public void Add(T t)
    {
        // 空对象不加入队列
        if (t == null)
        {
            return;
        }
        // 加入全局列表
        list.Add(t);
        // 加入四叉树
        quadTree.Insert(t);
    }
    /// <summary>
    /// 根据范围获取对象
    /// </summary>
    /// <param name="rect">矩形对象, 用于判断碰撞</param>
    /// <returns></returns>
    public IList<T> GetListWithRectangle(Rectangle rect)
    {
        // 返回范围内的对象列表
        return quadTree.Retrieve(rect);
    }

    /// <summary>
    /// 重新构建四叉树
    /// 使用情况: 列表中对向位置已变更时
    /// </summary>
    public void RebuildQuadTree()
    {
        quadTree.Clear();
        quadTree.Insert(list);
    }


    public void RebulidMapInfo()
    {
        if (mapinfo != null)
        {
            mapinfo.RebuildMapInfo(list);
        }
    }

    /// <summary>
    /// 清理数据
    /// </summary>
    public void Clear()
    {
        list.Clear();
        quadTree.Clear();
        if (mapinfo != null)
        {
            mapinfo = null;
        }
    }

}

/// <summary>
/// 选择目标权重抽象类
/// TODO 改成接口, 不适用抽象类
/// </summary>
public interface ISelectWeightData
{
    // 所有值都是从0-10, 0为完全不理会, 10为很重要, 并与其它权重进行合算

    /// <summary>
    /// 生命权重
    /// </summary>
    float HealthWeight { get; set; }

    /// <summary>
    /// 位置权重
    /// </summary>
    float DistanceWeight { get; set; }

    /// <summary>
    /// 角度权重
    /// </summary>
    float AngleWeight { get; set; }

    /// <summary>
    /// 类型权重
    /// </summary>
    float TypeWeight { get; set; }

    /// <summary>
    /// 等级权重
    /// </summary>
    float LevelWeight { get; set; }

    
}


/// <summary>
/// 目标选择单位数据
/// </summary>
public class Member : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
{
    // ----------------------------暴露接口-------------------------------

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    public int MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = value; }
    }

    public int Health
    {
        get { return health; }
        set { health = value; }
    }

    public int Atack
    {
        get { return atack; }
        set { atack = value; }
    }

    public int Define
    {
        get { return define; }
        set { define = value; }
    }

    public int MemberType
    {
        get { return memberType; }
        set { memberType = value; }
    }

    public int Diameter
    {
        get { return diameter; }
        set { diameter = value; }
    }

    public int ScanDiameter
    {
        get { return scanDiameter; }
        set { scanDiameter = value; }
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
    public int TargetCount
    {
        get { return targetCount; }
        set { targetCount = value; }
    }

    /// <summary>
    /// 目标点
    /// </summary>
    public Vector3 Direction
    {
        get { return direction; }
        set { direction = value; }
    }


    /// <summary>
    /// 生命权重
    /// </summary>
    public float HealthWeight
    {
        get { return healthWeight; }
        set { healthWeight = value; }
    }

    /// <summary>
    /// 位置权重
    /// </summary>
    public float DistanceWeight
    {
        get { return distanceWeight; }
        set { distanceWeight = value; }
    }

    /// <summary>
    /// 角度权重
    /// </summary>
    public float AngleWeight
    {
        get { return angleWeight; }
        set { angleWeight = value; }
    }

    /// <summary>
    /// 类型权重
    /// </summary>
    public float TypeWeight
    {
        get { return typeWeight; }
        set { typeWeight = value; }
    }

    /// <summary>
    /// 等级权重
    /// </summary>
    public float LevelWeight
    {
        get { return levelWeight; }
        set { levelWeight = value; }
    }


    // ------------------------------公有属性--------------------------------


    public string Name = "";


    // -------------------------------私有属性--------------------------------------

    private float speed = 4f;

    private int maxHealth = 100;

    private int health = 100;

    private int atack = 10;

    private int define = 10;

    private int memberType = 1;

    private int diameter = 1;

    private int scanDiameter = 40;

    private int targetCount = 10;

    private float x = 0;

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
            _hisX = X;
            _hisY = Y;
            _hisDimeter = Diameter;
            _rect = new Rectangle(X, Y, Diameter, Diameter);
        }
        return _rect;
    }
}


/// <summary>
/// Mamber基础接口
/// </summary>
public interface BaseMamber
{
    // ----------------------------------暴露接口--------------------------------------
    float Speed { get; set; }
    int MaxHealth { get; set; }
    int Health { get; set; }
    int Atack { get; set; }
    int Define { get; set; }
    int MemberType { get; set; }
    int Diameter { get; set; }
    int ScanDiameter { get; set; }
    int TargetCount { get; set; }
    float X { get; set; }
    float Y { get; set; }

    /// <summary>
    /// 目标点
    /// </summary>
    Vector3 Direction { get; set; }
    // ----------------------------------暴露接口--------------------------------------
}