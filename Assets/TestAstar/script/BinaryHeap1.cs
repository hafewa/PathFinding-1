using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 二叉堆
/// </summary>
/// <typeparam name="T">节点类型</typeparam>
public class BinaryHeap<T> where T : IComparable<T>
{
    /// <summary>
    /// 从当前节点开始的树所包含节点的数量
    /// </summary>
    public int Count { get; private set; }
    /// <summary>
    /// 当前节点
    /// </summary>
    public T CurrentNode { get; private set; }

    /// <summary>
    /// 是否为大顶二叉堆
    /// false则为小顶二叉堆
    /// </summary>
    private bool isMaxTop = true;

    /// <summary>
    /// 父节点
    /// </summary>
    private BinaryHeap<T> parentNode; 

    public BinaryHeap(T t, BinaryHeap<T> parentNode = null)
    {
        CurrentNode = t;
        this.parentNode = parentNode;
    }

    /// <summary>
    /// 左子节点
    /// </summary>
    public BinaryHeap<T> LeftNode { get; set; }

    /// <summary>
    /// 右子节点
    /// </summary>
    public BinaryHeap<T> RightNode { get; set; }

    /// <summary>
    /// 插入节点
    /// 插入后整理二叉堆
    /// </summary>
    /// <param name="t">节点内容</param>
    public void Push(T t)
    {
        // TODO 判断当前节点是否为空
        // 对比
        var result = CurrentNode.CompareTo(t);
        // 如果对比大于当前节点并且是大顶二叉堆则替换当前位置
        // 反之亦然
        // 子节点大小排序, 根节点之下第一排开始, 从左到右从小到大, 第二排从右到左从小到大......
        if (isMaxTop)
        {
            // 参数大于当前节点
            if (result < 0)
            {
                // 替换当前节点位置
                var orginNode = CurrentNode;
                // 当前节点赋值
                CurrentNode = t;
                // 对比两子节点将小的那个挤到下一层

                // 右侧节点为空
                // 左侧为空右侧一定为空
                if (RightNode != null)
                {
                    // 将右侧节点值取出放入左侧节点,
                    if (LeftNode != null)
                    {
                        LeftNode.Push(RightNode.CurrentNode);
                    }
                    else
                    {
                        LeftNode = new BinaryHeap<T>(RightNode.CurrentNode, this);
                    }
                    RightNode.Push(orginNode);
                    // 直接将当前右侧节点放到左侧节点, 左侧节点如果存在则挤到下面
                }
                else
                {
                    LeftNode = new BinaryHeap<T>(orginNode, this);
                }
            }
            else
            {
                // 小于当前节点, 则将其插入该节点的子节点
                // 对比同级子节点
                //var SameLevelNodeList = GetNodeListByLevel()
                // 使用
            }
        }
        Count++;
    }

    /// <summary>
    /// 返回最顶端的值
    /// </summary>
    /// <returns></returns>
    public T Pop()
    {
        // 弹出最顶端值
        var result = CurrentNode;
        // 删除顶端节点的值

        // 将子树重新填充排序


        Count--;
        return result;
    }

    /// <summary>
    /// 根据层级获取该一级的所有节点
    /// </summary>
    /// <param name="rootNode">开始搜索的节点</param>
    /// <param name="level">搜索从开始节点的子节点开始第几级</param>
    /// <returns></returns>
    public List<BinaryHeap<T>> GetNodeListByLevel(BinaryHeap<T> rootNode, int level)
    {
        if (rootNode == null || level < 0)
        {
            return null;
        }

        List<BinaryHeap<T>> result = null;

        // 遍历列表缓存
        List<BinaryHeap<T>> tmp = new List<BinaryHeap<T>>();
        List<BinaryHeap<T>> tmp2 = new List<BinaryHeap<T>>();
        var count = 0;
        tmp.Add(rootNode);
        while (count <= level)
        {

            // 搜索该层所有节点
            foreach (var node in tmp)
            {
                tmp2.Add(node.LeftNode);
                tmp2.Add(node.RightNode);
            }

            tmp.Clear();
            tmp.AddRange(tmp2);


            tmp2.Clear();
            count++;
        }
        result = tmp;
        return result;
    }




    public void Delete(T t)
    {

    }
    
}