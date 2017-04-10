using System;
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
                //item1 = item2;
                return -1;
            }
            if (item2 == null)
            {
                //item2 = item1;
                return 1;
            }
            if (item1.F < item2.F)
            {
                return -1;
            }
            return item1.F > item2.F ? 1 : 0;
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

        //IDictionary<long, Node> openDic= new Dictionary<long, Node>();
        BinaryHeapList<Node> openBHList = new BinaryHeapList<Node>(compareTo);
        IDictionary<long, Node> closeDic = new Dictionary<long, Node>();
        // 初始化开始节点
        openBHList.Push(new Node(startX, startY));
        // openDic.Add(GetKey(startX, startY), new Node(startX, startY));
        // openList.Enqueue(new Node(startX, startY));
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
            currentPoint = openBHList.Pop();
            // 找到路径
            if (currentPoint.X == endX && currentPoint.Y == endY)
            {
                endNode = currentPoint;
                break;
            }
            //currentPoint = Utils.GetNodeFromListWithCompare(openDic.Values, false, compareTo);
            // 删除该节点
            var keyForCurrentPoing = Utils.GetKey(currentPoint.X, currentPoint.Y);
            //openDic.Remove(keyForCurrentPoing);
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
                    var node = ExistInList(surroundPoint.X, surroundPoint.Y, openBHList.GetItemDic);
                    if (node == null)
                    {
                        // 计算H值, 通过水平和垂直距离确定
                        surroundPoint.H = (int)(Math.Sqrt(Math.Pow(endX - surroundPoint.X, 2) + Math.Pow(endY - surroundPoint.Y, 2))) * 10;//Math.Abs(endX - surroundPoint.X)*10 + Math.Abs(endY - surroundPoint.Y)*10;//
                        surroundPoint.G = g;
                        surroundPoint.F = surroundPoint.H + surroundPoint.G;
                        surroundPoint.Parent = currentPoint;
                        openBHList.Push(surroundPoint);
                    }
                    else // 存在于开启列表, 比较当前的G值与之前的G值大小
                    {
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
            if (openBHList.Count == 0)
            {
                break;
            }

            // 如果搜索次数大于(w+h) * 4 则停止搜索
            if (counter > maxSearchCount)
            {
                //openList = null;
                break;
            }
        } while (true);
        
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

    ///// <summary>
    ///// 
    ///// </summary>
    ///// <param name="node"></param>
    ///// <param name="nodeList"></param>
    ///// <returns></returns>
    //private static Node ExistInList<T>(Node node, IList<T> nodeList) where T : Node
    //{
    //    if (node == null || nodeList == null)
    //    {
    //        return null;
    //    }
    //    for (var i = 0; i < nodeList.Count; i++)
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
            long key = Utils.GetKey(x, y);
            if (nodeDic.ContainsKey(key))
            {
                result = nodeDic[key];
            }
        }
        return result;
    }



}

/// <summary>
/// 二叉堆列表
/// </summary>
public class BinaryHeapList<T> where T : Node
{

    /// <summary>
    /// 当前节点值
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// 当前子树的值数量
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// 单位字典, key为(x 左移 32) + y
    /// </summary>
    public IDictionary<long, T> GetItemDic
    {
        get
        {
            if (isOriginal)
            {
                return itemDic;
            }
            return null;
        }
    } 


    ///// <summary>
    ///// 单位列表
    ///// </summary>
    //private List<T> itemList = new List<T>();

    /// <summary>
    /// 对比方法
    /// </summary>
    private Func<T, T, int> compTo = null;

    /// <summary>
    /// 左子树
    /// </summary>
    private BinaryHeapList<T> leftSubTree;

    /// <summary>
    /// 右子树
    /// </summary>
    private BinaryHeapList<T> rightSubTree;

    /// <summary>
    /// 当前节点是否有值
    /// </summary>
    private bool hasValue = false;

    /// <summary>
    /// 单位字典, key为(x 左移 32) + y
    /// </summary>
    private IDictionary<long, T> itemDic = null;

    /// <summary>
    /// 单位数组, 用于二叉堆存储
    /// </summary>
    private T[] itemArray = null;

    /// <summary>
    /// 是否为源树
    /// 源树存储所有单位的列表
    /// </summary>
    private bool isOriginal = false;

    /// <summary>
    /// 数组位置
    /// </summary>
    private int arrayPos = 0;


    /// <summary>
    /// 初始化二叉堆列表
    /// </summary>
    /// <param name="compTo">对比大小方法, arg1>arg2返回-1, 反之返回1, 相等返回0</param>
    /// <param name="isOrg">是否为源</param>
    public BinaryHeapList(Func<T, T, int> compTo, bool isOriginal = true)
    {
        this.compTo = compTo;
        itemArray = new T[1024];
        if (isOriginal)
        {
            itemDic = new Dictionary<long, T>();
            this.isOriginal = true;
        }
    }

    /// <summary>
    /// 将单位加入列表, 并根据对比方法将其放到合适位置
    /// </summary>
    /// <param name="item">放入单位</param>
    public void Push(T item)
    {
        var localItem = item;

        if (localItem == null)
        {
            return;
        }

        // 将值插入列表最后位置
        itemArray[arrayPos] = item;
        // 然后向上调整数组
        FilterUp(arrayPos);
        // TODO 判断是否空间足够, 不足则重新分配空间



        // 数据本地化, 避免数据修改后向外扩散
        //var localItem = item;
        //// 值不能为null
        //if (localItem == null)
        //{
        //    return;
        //}
        //// 判断当前节点值
        //// 无值则放入当前节点
        //if (!hasValue)
        //{
        //    Value = localItem;
        //    hasValue = true;
        //    return;
        //}
        //else
        //{
        //    // 与当前节点对比
        //    // 将小的放入子节点
        //    if (compTo(Value, localItem) > 0)
        //    {
        //        var tmpItem = Value;
        //        Value = localItem;
        //        localItem = tmpItem;
        //    }
        //}

        //// 有值则放入判断放入子树
        //// 判断左右子树是否为空, 为空则创建子树并将值放入
        //if (leftSubTree == null)
        //{
        //    leftSubTree = new BinaryHeapList<T>(compTo, false);
        //    leftSubTree.Push(localItem);
        //    return;
        //}
        //if (rightSubTree == null)
        //{
        //    rightSubTree = new BinaryHeapList<T>(compTo, false);
        //    rightSubTree.Push(localItem);
        //    return;
        //}

        //// 不为空则判断与左右子树对比大小, 如果大于左右子树, 将小的放入其子树
        ////var compareLeft = compTo(leftSubTree.Value, localItem);
        ////var compareRight = compTo(rightSubTree.Value, localItem);
        //var compareTwoNode = compTo(leftSubTree.Value, rightSubTree.Value);

        //if (compareTwoNode > 0)
        //{
        //    // 放入左子树
        //    leftSubTree.Push(localItem);
        //}
        //else
        //{
        //    // 放入右子树
        //    rightSubTree.Push(localItem);
        //}
        arrayPos++;
        Count++;
        // 加入单元列表
        if (isOriginal)
        {
            itemDic.Add(Utils.GetKey(item.X, item.Y), item);
        }
    }

    /// <summary>
    /// 获取按照对比方法最大值并从列表中删除
    /// </summary>
    /// <returns>最大值</returns>
    public T Pop()
    {

        // 将数组最上位置弹出, 并将数组最后一位放到第一位
        var lastPos = arrayPos - 1;
        T result = itemArray[0];
        itemArray[0] = itemArray[lastPos];
        // 然后调整数组
        FileterDown(0, lastPos);
        // 将最上的节点value返回, 并将子树的内容向上退一格
        //if (hasValue)
        //{
        //bool popLeft = false;
        //bool popRight = false;
        //// result = 当前节点值
        //result = Value;
        //// 将子节点值向上
        //// 判断两子树是否都不为空
        //if (leftSubTree != null && rightSubTree != null)
        //{
        //    // 对比左右子树
        //    var compareTwoNode = compTo(leftSubTree.Value, rightSubTree.Value);
        //    if (compareTwoNode > 0)
        //    {
        //        // 将右节点上移
        //        popRight = true;
        //    }
        //    else
        //    {
        //        // 将左节点上移
        //        popLeft = true;
        //    }
        //}
        //else
        //{
        //    if (leftSubTree != null)
        //    {
        //        popLeft = true;
        //    }
        //    else if (rightSubTree != null)
        //    {
        //        popRight = true;
        //    }
        //}

        //if (popRight)
        //{
        //    // 将右节点上移
        //    Value = rightSubTree.Pop();
        //    // TODO 子节点无值无子节点将子节点放入缓存池
        //    // 将子节点置为空
        //    if (!rightSubTree.hasValue)
        //    {
        //        rightSubTree = null;
        //    }
        //}
        //else if (popLeft)
        //{
        //    // 将左节点上移
        //    Value = leftSubTree.Pop();
        //    // TODO 子节点无值无子节点将子节点放入缓存池
        //    // 将子节点置为空
        //    if (!leftSubTree.hasValue)
        //    {
        //        leftSubTree = null;
        //    }
        //}
        //else
        //{
        //    // 如果两个节点都为空
        //    hasValue = false;
        //    Value = default(T);
        //}

        // 从单位列表中删除该单位
        if (isOriginal)
        {
            itemDic.Remove(Utils.GetKey(result.X, result.Y));
        }
        //}
        Count--;
        arrayPos--;
        return result;
    }


    /// <summary>
    /// 向上调整数组
    /// </summary>
    /// <param name="start">开始调整位置</param>
    private void FilterUp(int start)
    {
        var current = start;
        // 获取父节点位置
        var parent = (current - 1)/2;
        // 当前节点值
        var item = itemArray[current];
        while (current > 0)
        {
            // 对比当前节点与父节点大小
            if (compTo(itemArray[parent], item) < 0)
            {
                // 如果当前节点值(判断)大于父节点则退出循环
                break;
            }
            else
            {
                // 节点上移
                itemArray[current] = itemArray[parent];
                current = parent;
                parent = (parent - 1)/2;
            }
        }

        itemArray[current] = item;
    }

    /// <summary>
    /// 向下调整数组
    /// </summary>
    /// <param name="start">调整开始位置</param>
    /// <param name="end">调整结束位置</param>
    private void FileterDown(int start, int end)
    {
        // 当前节点
        var current = start;
        // 左子节点
        var left = current*2 + 1;
        // 当前节点值
        var item = itemArray[current];

        while (left <= end)
        {
            if (left < end && compTo(itemArray[left], itemArray[left + 1]) > 0)
            {
                // 取比较大的子节点
                left++;
            }
            if (compTo(itemArray[left], item) > 0)
            {
                // 如果当前节点值(判断)大于子节点则退出
                break;
            }
            else
            {
                // 节点下移
                itemArray[current] = itemArray[left];
                current = left;
                left = left*2 + 1;
            }
        }
        itemArray[current] = item;
    }

}