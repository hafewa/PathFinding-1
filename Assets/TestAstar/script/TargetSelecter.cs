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
    /// <summary>
    /// 单例对象
    /// </summary>
    public static TargetSelecter Single
    {
        get
        {
            if (single == null)
            {
                single = new TargetSelecter();
            }
            return single;
        }
    }

    /// <summary>
    /// 单例对象
    /// </summary>
    private static TargetSelecter single = null;


    /// <summary>
    /// 筛选对象
    /// TODO 优化
    /// </summary>
    /// <typeparam name="T">对象类型. 必须继承</typeparam>
    /// <param name="searchObj">搜索对象</param>
    /// <param name="quadTree">四叉树</param>
    /// <returns></returns>
    public IList<T> TargetFilter<T>(T searchObj, QuadTree<T> quadTree) where T : ISelectWeightData, IBaseMamber, IGraphical<Rectangle>
    {
        IList<T> result = null;
        if (searchObj != null && quadTree != null)
        {
            // 取出范围内的单位
            var inScope =
                quadTree.GetScope(new Rectangle(searchObj.X - searchObj.ScanDiameter / 2f, searchObj.Y - searchObj.ScanDiameter / 2f,
                    searchObj.ScanDiameter,
                    searchObj.ScanDiameter));

            // 单位数量
            var targetCount = searchObj.TargetCount;
            // 目标列表Array
            var targetArray = new T[targetCount];
            // 目标权重值
            var weightKeyArray = new float[targetCount];

            for (var i = 0; i < inScope.Count; i++)
            {
                var item = inScope[i];
                if (item.Equals(searchObj))
                {
                    continue;
                }
                var sumWeight = 0f;


                // 从列表中找到几项权重值最高的目标个数个单位
                // 将各项值标准化, 然后乘以权重求和, 得到最高值

                // -------------------------Level1-----------------------------
                // 是否可攻击空中单位
                if (item.IsAir)
                {
                    if (searchObj.AirCraftWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.AirCraftWeight;
                }

                // 是否可攻击建筑
                if (item.IsBuild)
                {
                    if (searchObj.BuildWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.BuildWeight;
                }

                // 是否可攻击地面单位
                if (item.IsSurface)
                {
                    if (searchObj.SurfaceWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.SurfaceWeight;
                }

                // -------------------------Level2-----------------------------
                switch (item.ItemType)
                {
                    case MemberItemType.Tank:
                        sumWeight += searchObj.TankWeight;
                        break;
                    case MemberItemType.LV:
                        sumWeight += searchObj.LVWeight;
                        break;
                    case MemberItemType.Cannon:
                        sumWeight += searchObj.CannonWeight;
                        break;
                    case MemberItemType.Aircraft:
                        sumWeight += searchObj.AirCraftWeight;
                        break;
                    case MemberItemType.Soldier:
                        sumWeight += searchObj.SoldierWeight;
                        break;
                }

                // -------------------------Level3-----------------------------
                // 隐形单位
                if (item.IsHide)
                {
                    if (searchObj.HideWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.HideWeight;
                }
                // 嘲讽单位
                if (item.IsTaunt)
                {
                    if (searchObj.TauntWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.TauntWeight;
                }

                // -------------------------Level4-----------------------------
                // 小生命权重, 血越少权重越高
                if (searchObj.HealthMaxWeight > 0)
                {
                    // 血量 (最大血量 - 当前血量)/最大血量 * 生命权重
                    sumWeight += searchObj.HealthMaxWeight * (item.MaxHealth - item.Health) / item.MaxHealth;
                }

                // 大生命权重, 生命值越多权重越高
                if (searchObj.HealthMinWeight > 0)
                {
                    // 血量 当前血量/最大血量 * 生命权重
                    sumWeight += searchObj.HealthMinWeight * item.Health / item.MaxHealth;
                }

                // 角度权重, 角度越大权重越小
                if (searchObj.AngleWeight > 0)
                {
                    sumWeight += searchObj.AngleWeight * (180 - Vector3.Angle(searchObj.Direction, new Vector3(item.X - searchObj.X, 0, item.Y - searchObj.Y))) / 180;
                }

                var distance = Utils.GetTwoPointDistance2D(searchObj.X, searchObj.Y, item.X, item.Y);
                // 长距离权重, 距离越远权重越大
                if (searchObj.DistanceMinWeight > 0)
                {
                    sumWeight += searchObj.DistanceMinWeight *
                                 (searchObj.ScanDiameter -  distance) /
                                 searchObj.ScanDiameter;
                }

                // 短距离权重, 距离越远权重越小
                if (searchObj.DistanceMaxWeight > 0)
                {
                    sumWeight += searchObj.DistanceMaxWeight * distance / searchObj.ScanDiameter;
                }

                // TODO 各项为插入式结构
                // 比对列表中的值, 大于其中某项值则将其替换位置并讲其后元素向后推一位.
                for (var j = 0; j < weightKeyArray.Length; j++)
                {
                    if (sumWeight > weightKeyArray[j])
                    {
                        for (var k = weightKeyArray.Length - 1; k > j; k--)
                        {
                            weightKeyArray[k] = weightKeyArray[k - 1];
                            targetArray[k] = targetArray[k - 1];
                        }
                        weightKeyArray[j] = sumWeight;
                        targetArray[j] = item;
                        break;
                    }
                }
            }

            result = targetArray.Where(targetItem => targetItem != null).ToList();

            // TODO 不放在这里 散射效果
            if (searchObj.ScatteringRadius > Utils.ApproachZero && result.Count > 0)
            {
                result = Scottering(result[0], searchObj, quadTree);
            }
        }

        return result;
    }

    /// <summary>
    /// 散射效果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="targetObj">目标单位</param>
    /// <param name="searchObj">搜索单位</param>
    /// <param name="quadTree">对象二叉树</param>
    /// <returns>被散射目标</returns>
    public static List<T> Scottering<T>(T targetObj, T searchObj, QuadTree<T> quadTree) where T : ISelectWeightData, IBaseMamber, IGraphical<Rectangle>
    {
        List<T> result = null;
        // 散射, 按照距离搜索周围的单位, 并将其中一个作为本次的目标
        // 如果散射参数有值, 并且筛选列表中存在目标, 则取第一个单位作为筛选目标散射周围单位
        if ((searchObj.ScatteringRadius > Utils.ApproachZero && targetObj != null))
        {
            result = new List<T>();
            var target = targetObj;
            // TODO 圆形 取单位周围单位
            var rect = new Rectangle(target.X - searchObj.ScatteringRadius, target.Y - searchObj.ScatteringRadius,
                searchObj.ScatteringRadius * 2, searchObj.ScatteringRadius * 2);
           
            // 距离太近向后推
            var searchPos = new Vector3(searchObj.X, 0, searchObj.Y);
            var targetPos = new Vector3(target.X, 0, target.Y);
            var diffPos = targetPos - searchPos;
            var distance = diffPos.magnitude;
            // 距离小于极限距离
            if (distance < searchObj.ScatteringRadius * 1.2f)
            {
                // 将目标位置向后推
                diffPos = diffPos.normalized * searchObj.ScatteringRadius;
                targetPos = searchPos + diffPos;
                rect.X = targetPos.x - searchObj.ScatteringRadius;
                rect.Y = targetPos.z - searchObj.ScatteringRadius;
            }
            Utils.DrawRect(rect, Color.red);
            Debug.DrawLine(searchPos, targetPos, Color.red);
            // 散射范围内的单位
            var scatteringList = quadTree.GetScope(rect);
            // 计算命中
            var hitRate = 0f;
            var sumVolume = 0f;
            foreach (var scatteringItem in scatteringList)
            {
                sumVolume += scatteringItem.Diameter * scatteringItem.Diameter;
            }
            hitRate = (1 - (float)Math.Pow(1 - searchObj.Accuracy, sumVolume)) * 100;
            // 先随机命中, 如果命中则返回一个对象
            // 如果没命中则返回空对象
            // 随机一个值从对象中
            var random = new System.Random(DateTime.Now.Millisecond);
            var randomNum = random.Next(100);
            
            if (randomNum <= hitRate)
            {
                // 命中
                result.Add(scatteringList[random.Next(scatteringList.Count)]);
            }
        }
        return result;
    }
    
    ///// <summary>
    ///// 查找符合条件的对向列表
    ///// </summary>
    ///// <typeparam name="T">对象类型</typeparam>
    ///// <param name="mine">当前对象</param>
    ///// <param name="list">目标对象列表</param>
    ///// <param name="func">判断方法</param>
    ///// <returns>符合条件的对象列表</returns>
    //// 数据符合合并类型, 选择具体功能外抛
    //private static IList<T> Search<T>(T mine, IList<T> list, Func<T, T, bool> func) where T : ISelectWeightData
    //{
    //    List<T> result = null;
    //    if (list != null && func != null && mine != null)
    //    {
    //        result = new List<T>();
    //        T item;
    //        for (var i = 0; i < list.Count; i++)
    //        {
    //            item = list[i];
    //            if (func(mine, item))
    //            {
    //                result.Add(item);
    //            }
    //        }
    //    }
    //    return result;
    //}

    ///// <summary>
    ///// 获取单位权重值
    ///// </summary>
    ///// <typeparam name="T">单位</typeparam>
    ///// <param name="target">被选择单位</param>
    ///// <param name="selecter">选择单位</param>
    ///// <returns>权重值</returns>
    //private float GetWight<T>(T target, T selecter) where T : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
    //{
    //    var result = 0f;
    //    if (target != null && selecter != null)
    //    {
            
    //    }
    //    return result;
    //}
}
