﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*寻路
/// </summary>
public class AStarPathFinding
{


    /// <summary>
    /// 节点对比方法
    /// </summary>
    private static Func<Node, Node, int> compareTo = (item1, item2) =>
        {
            if (item1 == null)
            {
                item1 = item2;
                return 1;
            }
            if (item2 == null)
            {
                item2 = item1;
                return -1;
            }
            if (item1.F > item2.F)
            {
                return -1;
            }
            return item1.F < item2.F ? 1 : 0;
        };
    // TODO 二叉堆加入
    /// <summary>
    /// 寻找路径
    /// </summary>
    /// <param name="map">地图数组</param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    /// <param name="diameterX">物体X轴宽度</param>
    /// <param name="diameterY">物体Y轴宽度</param>
    /// <param name="isJumpPoint">是否为跳跃式点, 如果为true 则路径只会给出拐点处的关键点</param>
    /// <param name="completeCallback">结束回调函数</param>
    /// <returns>返回路径点列表, 如果列表长度为0则没有路径</returns>
    public static IList<Node> SearchRoad(int[][] map, int startX, int startY, int endX, int endY, int diameterX, int diameterY,
        bool isJumpPoint = false, Action completeCallback = null)
    {
        // 结束节点
        Node endNode = null;
        var now = Time.realtimeSinceStartup;

        //List<Node> openList = new List<Node>();
        //BinaryHeap.BinaryHeap<Node> openList = new BinaryHeap.BinaryHeap<Node>(Math.Abs(endX - startX) * Math.Abs(endY - startY), Order.DESC);
        //Queue<Node> openList = new Queue<Node>();
        //IList<Node> closeList = new List<Node>();
        IDictionary<long, Node> openDic= new Dictionary<long, Node>();
        IDictionary<long, Node> closeDic = new Dictionary<long, Node>();
        // 初始化开始节点
        openDic.Add(GetKey(startX, startY), new Node(startX, startY));
        //openList.Enqueue(new Node(startX, startY));
        // 如果搜索次数大于(w+h) * 4 则停止搜索
        var maxSearchCount = (map.Length + map[0].Length) * 40;

        // 计算结束偏移
        endX = endX - diameterX;
        endY = endY - diameterY;

        var counter = 0;

        // 寻路G值
        int g;
        Node[] surroundPointArray;
        Node currentPoint;
        
        do
        {
            counter++;
            // 获取最小节点
            currentPoint = Utils.GetNodeFromListWithCompare(openDic.Values, false, compareTo);
            // 删除该节点
            var keyForCurrentPoing = GetKey(currentPoint.X, currentPoint.Y);
            openDic.Remove(keyForCurrentPoing);
            // 将当前节点放入关闭列表
            if (!closeDic.ContainsKey(keyForCurrentPoing))
            {
                closeDic.Add(keyForCurrentPoing, currentPoint);
            }
            // 获取当前节点周围的节点
            surroundPointArray = SurroundPoint(currentPoint);

            foreach (Node surroundPoint in surroundPointArray)
            {
                // 斜向是否可移动
                // 判断周围节点合理性
                if (ExistInList(surroundPoint.X, surroundPoint.Y, closeDic) == null && IsPassable(map, surroundPoint, currentPoint, diameterX, diameterY))
                {
                    // 计算G值 上下左右为10, 四角为14
                    g = currentPoint.G + (((currentPoint.X - surroundPoint.X) * (currentPoint.Y - surroundPoint.Y)) == 0 ? 10 : 14);

                    // 该点是否在开启列表中
                    if (ExistInList(surroundPoint.X, surroundPoint.Y, openDic) == null)
                    {
                        // 计算H值, 通过水平和垂直距离确定
                        surroundPoint.H = (int)(Math.Sqrt(Math.Pow(endX - surroundPoint.X, 2) + Math.Pow(endY - surroundPoint.Y, 2))) * 10;//Math.Abs(endX - surroundPoint.X)*10 + Math.Abs(endY - surroundPoint.Y)*10;//
                        surroundPoint.G = g;
                        surroundPoint.F = surroundPoint.H + surroundPoint.G;
                        surroundPoint.Parent = currentPoint;
                        openDic.Add(GetKey(surroundPoint.X, surroundPoint.Y), surroundPoint);
                    }
                    else // 存在于开启列表, 比较当前的G值与之前的G值大小
                    {
                        var node = ExistInList(surroundPoint.X, surroundPoint.Y, openDic);
                        if (g < node.G)
                        {
                            node.Parent = currentPoint;
                            node.G = g;
                            node.F = g + node.H;
                        }
                    }
                }
            }


            // 如果开放列表为空, 则没有通路
            if (openDic.Count == 0)
            {
                break;
            }

            // 如果搜索次数大于(w+h) * 4 则停止搜索
            if (counter > maxSearchCount)
            {
                //openList = null;
                break;
            }

            // TODO 可以不创建新对象
        } while ((endNode = ExistInList(endX, endY, openDic)) == null);
        
        IList<Node> path = new List<Node>();
        // 如果有可行路径
        if (endNode != null)
        {
            // 将路径回退并放入列表
            var currentNode = endNode;
            do
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            } while (currentNode.X != startX || currentNode.Y != startY);
        }

        // 处理跳点路径
        if (isJumpPoint && path.Count > 0)
        {
            // 如果路径方向发生变化则变化的奇点为拐点放入列表
            var jumpPointPath = new List<Node>();
            var startPoint = path[0];
            jumpPointPath.Add(startPoint);
            var xOff = startPoint.X;
            var yOff = startPoint.Y;
            foreach (var pathPoint in path)
            {
                // 不是第一个元素
                if (pathPoint.Parent == null)
                {
                    continue;
                }

                // x,y差值是否与上一次相同, 不相同则保存拐点
                var nowXOff = pathPoint.X - pathPoint.Parent.X;
                var nowYOff = pathPoint.Y - pathPoint.Parent.Y;
                if (nowXOff != xOff || nowYOff != yOff)
                {
                    xOff = nowXOff;
                    yOff = nowYOff;
                    jumpPointPath.Add(pathPoint);
                }
            }
            path.Clear();
            path = jumpPointPath;

        }

        if (completeCallback != null)
        {
            completeCallback();
        }

        Debug.Log(string.Format("{0:#.##########}", Time.realtimeSinceStartup - now));
        // 返回路径, 如果路径数量为0 则没有可行路径
        return path;
    }

    /// <summary>
    /// 当前位置是否可以通过
    /// </summary>
    /// <param name="computeMap">地图</param>
    /// <param name="nowNode">当前位置</param>
    /// <param name="prvNode">父节点</param>
    /// <param name="diameterX">移动物体X轴高度</param>
    /// <param name="diameterY">移动物体Y轴高度</param>
    /// <returns></returns>
    private static bool IsPassable(int[][] computeMap, Node nowNode, Node prvNode, int diameterX, int diameterY)
    {
        //var now = Time.realtimeSinceStartup;
        // 定义 物体位置为左上角(主要指直径大于1的)
        // 验证参数是否合法
        if (diameterX <= 0 || diameterY < 0 || computeMap == null || nowNode == null || prvNode == null)
        {
            return false;
        }
        // TODO 优化方案 差值判断不同区域, 重复区域忽略
        // 遍历直径内的点
        // 优化, 中间忽略, 只判断外圈
        for (var i = 0; i < diameterX; i++)
        {
            for (var j = 0; j < diameterY; j++)
            {
                if (i > 0 && i < diameterX - 1 && j > 0 && j < diameterY - 1)
                {
                    continue;
                }
                var computeX = nowNode.X + i;
                var computeY = nowNode.Y + j;
                // 判断点的位置是否合法
                if (computeX < 0 ||
                    computeY < 0 ||
                    computeX >= computeMap[0].Length ||
                    computeY >= computeMap.Length ||
                    computeMap[computeY][computeX] == Utils.Obstacle)
                {
                    return false;
                }
            }
        }
        //Debug.Log(string.Format("{0:#.#######}",Time.realtimeSinceStartup - now));

        return true;
    }

    /// <summary>
    /// 获取当前点周围的点
    /// </summary>
    /// <param name="curPoint">当前点</param>
    /// <returns>周围节点的数组</returns>
    private static Node[] SurroundPoint(Node curPoint)
    {
        var x = curPoint.X;
        var y = curPoint.Y;
        return new[]
        {
            new Node(x - 1, y - 1),
            new Node(x, y - 1),
            new Node(x + 1, y - 1),
            new Node(x + 1, y),
            new Node(x + 1, y + 1),
            new Node(x, y + 1),
            new Node(x - 1, y + 1),
            new Node(x - 1, y)
        };
    }


    /// <summary>
    /// 在列表中查找节点
    /// 如果列表中存在则返回该节点
    /// </summary>
    /// <param name="node">被查找节点</param>
    /// <param name="nodeList">查找列表</param>
    /// <returns></returns>
    //private static Node ExistInList(Node node, IEnumerator<Node> nodeList)
    //{
    //    if (node == null || nodeList == null)
    //    {
    //        return null;
    //    }
    //    //var now = Time.realtimeSinceStartup;
    //    // ToArray能提高运行速度
    //    while (nodeList.MoveNext())
    //    {
    //        var tmpNode = nodeList.Current;
    //        if (node.X == tmpNode.X && node.Y == tmpNode.Y)
    //        {
    //            return tmpNode;
    //        }

    //    }
    //    //Debug.Log(string.Format("{0:#.##########}", Time.realtimeSinceStartup - now));
    //    return null;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeList"></param>
    /// <returns></returns>
    //private static Node ExistInList(Node node, Node[] nodeList)
    //{
    //    if (node == null || nodeList == null)
    //    {
    //        return null;
    //    }
    //    for (var i = 0; i < nodeList.Length; i++)
    //    {
    //        var tmpNode = nodeList[i];
    //        try
    //        {
    //            if (node.X == tmpNode.X && node.Y == tmpNode.Y)
    //            {
    //                return tmpNode;
    //            }
    //        }
    //        catch
    //        {
    //            int j = 0;
    //        }
    //    }
    //    return null;
    //}

    /// <summary>
    /// 在字典中获取节点
    /// </summary>
    /// <param name="x">位置x</param>
    /// <param name="y">位置y</param>
    /// <param name="nodeDic">目标字典</param>
    /// <returns>如果存在则返回该节点, 否则返回null</returns>
    private static Node ExistInList(long x, long y, IDictionary<long, Node> nodeDic)
    {
        Node result = null;
        if (x >= 0 && y >= 0 && nodeDic != null)
        {
            long key = GetKey(x, y);
            if (nodeDic.ContainsKey(key))
            {
                result = nodeDic[key];
            }
        }
        return result;
    }

    /// <summary>
    /// 获取node的key值
    /// </summary>
    /// <param name="x">位置x</param>
    /// <param name="y">位置y</param>
    /// <returns>key值</returns>
    private static long GetKey(long x, long y)
    {
        var result = (x << 32) + y;
        return result;
    }

}

/// <summary>
/// 二叉堆列表
/// </summary>
public class BinaryHeapList<T>
{
    /// <summary>
    /// 单位列表
    /// </summary>
    private List<T> itemList = new List<T>();

    /// <summary>
    /// 对比方法
    /// </summary>
    private Func<T, T> compTo = null;


    /// <summary>
    /// 初始化二叉堆列表
    /// </summary>
    /// <param name="compTo">对比大小方法, arg1>arg2返回-1, 反之返回1, 相等返回0</param>
    public BinaryHeapList(Func<T, T> compTo)
    {
        this.compTo = compTo;
    }  

    public void Push(T item)
    {
        
    }


    public T Pop()
    {
        T result = default (T);

        return result;
    }
}