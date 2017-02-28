﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

/// <summary>
/// 四叉树
/// 用于分割地图
/// 提高碰撞检测效率
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class QuadTree<T> where T : IGraphical<Rectangle>
{

    /// <summary>
    /// 当前节点最大单元数量
    /// 如果当前节点为最深节点则不受此数量限制
    /// </summary>
    private int maxItemCount = 10;

    /// <summary>
    /// 最大树深度
    /// </summary>
    private int maxLevel = 5;

    /// <summary>
    /// 当前四叉树所在等级
    /// </summary>
    private int level;

    /// <summary>
    /// 对象列表
    /// </summary>
    private IList<T> itemsList;

    /// <summary>
    /// 当前四叉树节点编辑
    /// </summary>
    private Rectangle rect;

    /// <summary>
    /// 子树节点列表
    /// </summary>
    private QuadTree<T>[] nodes;

    /// <summary>
    /// 初始化四叉树
    /// </summary>
    /// <param name="level">当前四叉树所在位置</param>
    /// <param name="rect">当前四叉树的位置与宽度大小</param>
    public QuadTree(int level, Rectangle rect)
    {
        this.level = level;
        itemsList = new List<T>();
        this.rect = rect;
        nodes = new QuadTree<T>[4];
    }

    /// <summary>
    /// 清除四叉树
    /// </summary>
    public void Clear()
    {
        itemsList.Clear();

        for (var i = 0; i < nodes.Length; i++)
        {
            var quadTree = nodes[i];
            if (quadTree == null) { continue;}
            quadTree.Clear();
            nodes[i] = null;
        }
    }


    /// <summary>
    /// 根据rect获取该rect所在的节点
    /// </summary>
    /// <param name="item">对象</param>
    /// <returns>节点编号</returns>
    public int GetIndex(T item)
    {
        var result = -1;
        // 获得当前节点rect的中心点
        var midPointX = this.rect.X + this.rect.Width/2;
        var midPointY = this.rect.Y + this.rect.Height/2;

        var pRect = item.GetGraphical();
        // TODO 判断当前分割大小是否比目标大, 否则无意义
        // 0点在左下角
        var topContians = (pRect.Y > midPointY); 
        var bottomContians = (pRect.Y < midPointY - pRect.Height);

        if (pRect.X < midPointX - pRect.Width)
        {
            if (topContians)
            {
                // 左上角
                result = 0;
            }
            else if (bottomContians)
            {
                // 左下角
                result = 2;
            }
        }
        else if(pRect.X > midPointX)
        {
            if (topContians)
            {
                // 右上角
                result = 1;
            }
            else if (bottomContians)
            {
                // 右下角
                result = 3;
            }
        }

        return result;
    }

    /// <summary>
    /// 插入对象
    /// </summary>
    /// <param name="item">对象</param>
    public void Insert(T item)
    {
        // 有子节点
        if (nodes[0] != null)
        {
            // 判断插入哪个节点
            var index = GetIndex(item);

            // 子节点是否可以插入
            if (index != -1)
            {
                nodes[index].Insert(item);
                return;
            }
        }

        // 不能插入子节点当前节点插入
        itemsList.Add(item);


        // 判断是否item数量大于maxCount, 并且level小于maxLevel
        if (itemsList.Count > maxItemCount && level < maxLevel)
        {
            // 大于则创建子节点
            Split();
            // 将节点挨个加入子节点, 不能放入子节点的继续保留
            for (var i = 0; i < itemsList.Count; i++)
            {
                var index = GetIndex(itemsList[i]);
                if (index != -1)
                {
                    nodes[index].Insert(itemsList[i]);
                    // 从当前列表中删除该节点
                    itemsList.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    /// <summary>
    /// 插入列表
    /// </summary>
    /// <param name="list">对向列表</param>
    public void Insert(IList<T> list)
    {
        foreach (var item in list)
        {
            Insert(item);
        }
    }

    /// <summary>
    /// 返回传入对象可能会有碰撞的对向列表
    /// </summary>
    /// <param name="item">碰撞对象</param>
    /// <returns>可能碰撞的列表, 对量性质: 在传入rect所在的最底层自己点的对量+其上各个父级的边缘节点</returns>
    public List<T> Retrieve(T item)
    {
        var result = new List<T>();

        // 获得该列表所在子节点
        var index = GetIndex(item);
        // 如果未在子节点则从当前节点返回所有对象
        if (index != -1 && nodes[0] != null)
        {
            result.AddRange(nodes[index].Retrieve(item));
        }

        result.AddRange(itemsList);
        return result;
    }


    /// <summary>
    /// 拆分当前四叉树节点, 增加四个字节点
    /// 并将当前节点内的的对象进行分类转移至子节点
    /// </summary>
    private void Split()
    {
        // 获得当前四叉树的一半宽度高度
        var subQuadTreeWidth = rect.Width / 2;
        var subQuadTreeHeight = rect.Height / 2;

        // 创建四个子节点
        nodes[0] = new QuadTree<T>(level + 1, new Rectangle(rect.X, rect.Y, subQuadTreeWidth, subQuadTreeHeight));
        nodes[1] = new QuadTree<T>(level + 1, new Rectangle(rect.X + subQuadTreeWidth, rect.Y, subQuadTreeWidth, subQuadTreeHeight));
        nodes[2] = new QuadTree<T>(level + 1, new Rectangle(rect.X, rect.Y + subQuadTreeHeight, subQuadTreeWidth, subQuadTreeHeight));
        nodes[3] = new QuadTree<T>(level + 1, new Rectangle(rect.X + subQuadTreeWidth, rect.Y + subQuadTreeHeight, subQuadTreeWidth, subQuadTreeHeight));
    }
}

/// <summary>
/// 矩形类
/// </summary>
public class Rectangle : GraphicalItem<Rectangle>
{

    /// <summary>
    /// 宽度
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// 高度
    /// </summary>
    public float Height { get; set; }


    public Rectangle(float x, float y, float w, float h)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
    }

    /// <summary>
    /// 检测碰撞
    /// </summary>
    /// <param name="target">目标</param>
    /// <returns>是否碰撞</returns>
    public override bool IsCollision(Rectangle target)
    {
        if (target == null || X + Width < target.X || target.X + target.Width < X || Y + Height < target.Y || target.Y + target.Height < Y)
        {
            return false;
        }
        return true;
    }
}

/// <summary>
/// 图形接口
/// 提供图形反馈
/// </summary>
/// <typeparam name="T">图形类型</typeparam>
public interface IGraphical<T> where T : GraphicalItem<T>
{
    T GetGraphical();
}


/// <summary>
/// 图形抽象类
/// </summary>
/// <typeparam name="T">目标类型也是图形抽象类</typeparam>
public abstract class GraphicalItem<T> where T : GraphicalItem<T>
{

    /// <summary>
    /// 位置X
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// 位置Y
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// 检测碰撞
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <returns>是否碰撞</returns>
    public abstract bool IsCollision(T target);
}