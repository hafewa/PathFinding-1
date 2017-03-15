﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using MonoEX;

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
    /// 节点缓存
    /// </summary>
    private Queue<QuadTree<T>> nodeCache = null;

    /// <summary>
    /// 矩形缓存
    /// </summary>
    private Queue<Rectangle> rectCache = null; 

    /// <summary>
    /// 初始化四叉树
    /// </summary>
    /// <param name="level">当前四叉树所在位置</param>
    /// <param name="rect">当前四叉树的位置与宽度大小</param>
    /// <param name="parentQueue">父级引用cache</param>
    public QuadTree(int level, Rectangle rect, Queue<QuadTree<T>> parentNodeCache = null, Queue<Rectangle> parentRectCache = null)
    {
        this.level = level;
        itemsList = new List<T>();
        this.rect = rect;
        nodes = new QuadTree<T>[4];
        nodeCache = parentNodeCache ?? new Queue<QuadTree<T>>();
        rectCache = parentRectCache ?? new Queue<Rectangle>();
    }


    /// <summary>
    /// 根据rect获取该rect所在的节点
    /// </summary>
    /// <param name="item">对象</param>
    /// <returns>节点编号</returns>
    public int GetIndex(Rectangle item)
    {
        var result = -1;
        // 获得当前节点rect的中心点
        var midPointX = this.rect.X + this.rect.Width/2;
        var midPointY = this.rect.Y + this.rect.Height/2;
        
        // 0点在左下角
        var topContians = (item.Y > midPointY); 
        var bottomContians = (item.Y < midPointY - item.Height);

        if (item.X < midPointX - item.Width)
        {
            if (topContians)
            {
                // 左上角
                result = 3;
            }
            else if (bottomContians)
            {
                // 左下角
                result = 2;
            }
        }
        else if (item.X > midPointX)
        {
            if (topContians)
            {
                // 右上角
                result = 0;
            }
            else if (bottomContians)
            {
                // 右下角
                result = 1;
            }
        }
        else
        {
            // TODO
            int i = 0;
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
            if (InsertToSubNode(item, nodes))
            {
                return;
            }
        }
        
        itemsList.Add(item);
        // 判断是否item数量大于maxCount, 并且level小于maxLevel
        if (itemsList.Count > maxItemCount && level < maxLevel)
        {
            // 大于则创建子节点
            if (nodes[0] == null)
            {
                Split();
            }
            // 将节点挨个加入子节点, 不能放入子节点的继续保留
            for (var i = 0; i < itemsList.Count; i++)
            {
                var tmpItem = itemsList[i];
                if (InsertToSubNode(tmpItem, nodes))
                {
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
    /// <param name="rect">碰撞对象</param>
    /// <returns>可能碰撞的列表, 对量性质: 在传入rect所在的最底层自己点的对量+其上各个父级的边缘节点</returns>
    public IList<T> Retrieve(Rectangle rectangle)
    {
        var result = new List<T>();

        var index = GetIndex(rectangle);
        // 如果未在子节点则从当前节点返回所有对象
        if (index != -1 && nodes[0] != null)
        {
            result.AddRange(nodes[index].Retrieve(rectangle));
        }

        result.AddRange(itemsList);

        return result;
    }


    /// <summary>
    /// 按照矩形返回获取范围内对向列表
    /// TODO 优化
    /// </summary>
    /// <param name="scopeRect">范围rect</param>
    /// <returns></returns>
    public IList<T> GetScope(Rectangle scopeRect)
    {
        List<T> result = null;
        // 判断与当前四叉树的相交
        if (scopeRect != null && rect.IsCollision(scopeRect))
        {
            result = new List<T>();
            T tmpItem;
            // 遍历当前节点中的对象列表, 是否有相交
            for (var i = 0; i < itemsList.Count; i++)
            {
                tmpItem = itemsList[i];
                if (tmpItem.GetGraphical().IsCollision(scopeRect))
                {
                    result.Add(tmpItem);
                }
            }
            if (nodes[0] != null)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].rect.IsCollision(scopeRect))
                    {
                        result.AddRange(nodes[i].GetScope(scopeRect));
                    }
                }
            }
            // 划定范围, 获取范围内子对象中符合范围的对象
            // 判断是否与该区域相交 相交则取该区域内对象判断, 并获取其子节点判断是否相交
            // 获取子列表中的对象
        }

        return result;
    }

    /// <summary>
    /// 获取当前四叉树的矩形区域
    /// </summary>
    /// <returns></returns>
    public Rectangle GetRectangle()
    {
        return rect;
    }


    /// <summary>
    /// 获得子树列表
    /// </summary>
    /// <returns></returns>
    public QuadTree<T>[] GetSubNodes()
    {
        return nodes;
    }

    /// <summary>
    /// 获取当前树中的单元列表
    /// </summary>
    /// <returns></returns>
    public IList<T> GetItemList()
    {
        return itemsList;
    }

    
    /// <summary>
    /// 清除四叉树
    /// 已创建对象不会消除(减少GC消耗), clear后会放入对象池中当在读创建时取出重用
    /// </summary>
    public void Clear()
    {
        // 清空列表
        itemsList.Clear();
        // 清空范围
        // rect = null;

        for (var i = 0; i < nodes.Length; i++)
        {
            var quadTree = nodes[i];
            if (quadTree == null) { continue; }
            quadTree.Clear();
            nodes[i] = null;
            // 将列表放入缓存
            nodeCache.Enqueue(quadTree);
            rectCache.Enqueue(quadTree.rect);
        }
    }

    // --------------------------------私有方法---------------------------------


    /// <summary>
    /// 拆分当前四叉树节点, 增加四个字节点
    /// 并将当前节点内的的对象进行分类转移至子节点
    /// </summary>
    private void Split()
    {
        QuadTree<T> node = null;
        Rectangle subRect = null;
        int subLevel = level + 1;
        for (var i = 0; i < 4; i++)
        {
            subRect = GetSplitRectangle(rect, i);
            if (nodeCache.Count != 0)
            {
                node = nodeCache.Dequeue();
                node.level = subLevel;
                node.rect = subRect;
            }
            else
            {
                node = new QuadTree<T>(subLevel, subRect, nodeCache);
            }

            nodes[i] = node;
        }
    }


    /// <summary>
    /// 将对象插入子节点
    /// </summary>
    /// <param name="item"></param>
    /// <param name="subNodes"></param>
    private bool InsertToSubNode(T item, QuadTree<T>[] subNodes)
    {
        var result = false;
        var index = GetIndex(item.GetGraphical());
        if (index != -1)
        {
            subNodes[index].Insert(item);
            result = true;
        }
        return result;
    }

    /// <summary>
    /// 获得子节点
    /// </summary>
    /// <param name="parentRect">父节点矩形范围</param>
    /// <param name="subRectNum">子节点ID</param>
    /// <param name="rectCacheQueue">矩形对象缓存队列</param>
    /// <returns>子节点矩形范围</returns>
    private static Rectangle GetSplitRectangle(Rectangle parentRect, int subRectNum,
        Queue<Rectangle> rectCacheQueue = null)
    {
        // 获得当前四叉树的一半宽度高度
        var subQuadTreeWidth = parentRect.Width/2;
        var subQuadTreeHeight = parentRect.Height/2;
        var subX = 0f;
        var subY = 0f;
        Rectangle result = null;
        if (rectCacheQueue != null && rectCacheQueue.Count > 0)
        {
            result = rectCacheQueue.Dequeue();
        }
        else
        {
            result = new Rectangle();
        }
        switch (subRectNum)
        {
            case 0:
                subX = parentRect.X + subQuadTreeWidth;
                subY = parentRect.Y + subQuadTreeHeight;
                break;
            case 1:
                subX = parentRect.X + subQuadTreeWidth;
                subY = parentRect.Y;
                break;
            case 2:
                subX = parentRect.X;
                subY = parentRect.Y;
                break;
            case 3:
                subX = parentRect.X;
                subY = parentRect.Y + subQuadTreeHeight;
                break;
        }

        result.X = subX;
        result.Y = subY;
        result.Width = subQuadTreeWidth;
        result.Height = subQuadTreeHeight;

        return result;
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

    public Rectangle()
    {
        
    }

    public Rectangle(float x, float y, float w, float h)
    {
        X = x;
        Y = y;
        Width = w;
        Height = h;
    }

    /// <summary>
    /// 检测碰撞
    /// TODO 优化
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


    public override string ToString()
    {
        return string.Format("x:{0}, y:{1}, W:{2}, H:{3}", X, Y, Width, Height);
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