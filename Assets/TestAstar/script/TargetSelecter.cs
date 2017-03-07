﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    /// 目标总列表
    /// </summary>
    private List<T> list = null;

    /// <summary>
    /// 四叉树
    /// </summary>
    private QuadTree<T> quadTree = null;

    /// <summary>
    /// 创建目标列表
    /// </summary>
    /// <param name="x">地图位置x</param>
    /// <param name="y">地图位置y</param>
    /// <param name="width">地图宽度</param>
    /// <param name="height">地图高度</param>
    public TargetList(float x, float y, int width, int height)
    {
        var mapRect = new Rectangle(x, y, width, height);
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

