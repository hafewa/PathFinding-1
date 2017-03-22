﻿using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*寻路
/// </summary>
public class AStarPathFinding
{

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
    public static IList<Node> SearchRoad(int[][] map, int startX, int startY, int endX, int endY, int diameterX, int diameterY,
        bool isJumpPoint = false, Action completeCallback = null)
    {
        // 结束节点
        Node endNode = null;
        var now = Time.realtimeSinceStartup;

        List<Node> openList = new List<Node>();
        //BinaryHeap.BinaryHeap<Node> openList = new BinaryHeap.BinaryHeap<Node>(Math.Abs(endX - startX) * Math.Abs(endY - startY), Order.DESC);
        //Queue<Node> openList = new Queue<Node>();
        IList<Node> closeList = new List<Node>();
        // 初始化开始节点
        openList.Add(new Node(startX, startY));
        //openList.Enqueue(new Node(startX, startY));
        // 如果搜索次数大于(w+h) * 4 则停止搜索
        var maxSearchCount = (map.Length + map[0].Length) * 4;

        // 计算结束偏移
        endX = endX - diameterX;
        endY = endY - diameterY;
        var counter = 0;

        int g;
        Node[] surroundPointArray;
        Node currentPoint;

        // 节点对比方法
        Func<Node, Node, int> compareTo = (item1, item2) =>
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

        do
        {
            counter++;
            // 获取最小节点
            currentPoint = Utils.GetNodeFromListWithCompare(openList, false, compareTo);
            // 删除该节点
            openList.Remove(currentPoint);
            //currentPoint = openList.Dequeue();
            // 将当前节点放入关闭列表
            closeList.Add(currentPoint);
            // 获取当前节点周围的节点
            surroundPointArray = SurroundPoint(currentPoint);

            foreach (Node surroundPoint in surroundPointArray)
            {
                // 是否可以通过
                // 判断位置
                // 判断是否障碍
                // 斜向是否可移动
                // 判断周围节点合理性
                if (ExistInList(surroundPoint, closeList.GetEnumerator()) == null && IsPassable(map, surroundPoint, currentPoint, diameterX, diameterY))
                {
                    // 计算G值 上下左右为10, 四角为14
                    g = currentPoint.G + ((currentPoint.X - surroundPoint.X) * (currentPoint.Y - surroundPoint.Y)) == 0
                        ? 10
                        : 14;

                    // 该点是否在开启列表中
                    if (ExistInList(surroundPoint, openList.GetEnumerator()) == null)
                    {
                        // 计算H值, 通过水平和垂直距离确定
                        surroundPoint.H = Math.Abs(endX - surroundPoint.X)*10 + Math.Abs(endY - surroundPoint.Y)*10;//(int)(Math.Sqrt(Math.Pow(endX - surroundPoint.X, 2) + Math.Pow(endY - surroundPoint.Y, 2)));//
                        surroundPoint.G = g;
                        surroundPoint.F = surroundPoint.H + surroundPoint.G;
                        surroundPoint.Parent = currentPoint;
                        openList.Add(surroundPoint);
                    }
                    else // 存在于开启列表, 比较当前的G值与之前的G值大小
                    {
                        var node = ExistInList(surroundPoint, openList.GetEnumerator());
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
            if (openList.Count == 0)
            {
                break;
            }

            // 重新排列, 将F值最小的放在最先取出的位置
            //var tmpList = openList.ToArray();
            //Array.Sort(tmpList, (a, b) => a.F - b.F);
            //openList = new Queue<Node>(tmpList);
            // 如果搜索次数大于(w+h) * 4 则停止搜索
            if (counter > maxSearchCount)
            {
                //openList = null;
                break;
            }

        } while ((endNode = ExistInList(new Node(endX, endY), openList.GetEnumerator())) == null);
        
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
    private static Node ExistInList(Node node, IEnumerator<Node> nodeList)
    {
        if (node == null || nodeList == null)
        {
            return null;
        }
        //var now = Time.realtimeSinceStartup;
        // ToArray能提高运行速度
        while (nodeList.MoveNext())
        {
            var tmpNode = nodeList.Current;
            if (node.X == tmpNode.X && node.Y == tmpNode.Y)
            {
                return tmpNode;
            }

        }
        //Debug.Log(string.Format("{0:#.##########}", Time.realtimeSinceStartup - now));
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeList"></param>
    /// <returns></returns>
    private static Node ExistInList(Node node, Node[] nodeList)
    {
        if (node == null || nodeList == null)
        {
            return null;
        }
        for (var i = 0; i < nodeList.Length; i++)
        {
            var tmpNode = nodeList[i];
            try
            {
                if (node.X == tmpNode.X && node.Y == tmpNode.Y)
                {
                    return tmpNode;
                }
            }
            catch
            {
                int j = 0;
            }
        }
        return null;
    }

}
