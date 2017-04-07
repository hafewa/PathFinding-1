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
    public IList<T> TargetFilter<T>(T searchObj, QuadTree<T> quadTree) where T : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
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
                // 钻地隐形单位
                if (item.IsHideZD)
                {
                    if (searchObj.HideZDWeight < 0)
                    {
                        continue;
                    }
                    sumWeight += searchObj.HideZDWeight;
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
    public static List<T> Scottering<T>(T targetObj, T searchObj, QuadTree<T> quadTree) where T : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
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

    /// <summary>
    /// 获取单位权重值
    /// </summary>
    /// <typeparam name="T">单位</typeparam>
    /// <param name="target">被选择单位</param>
    /// <param name="selecter">选择单位</param>
    /// <returns>权重值</returns>
    private float GetWight<T>(T target, T selecter) where T : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
    {
        var result = 0f;
        if (target != null && selecter != null)
        {
            
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
    /// <param name="unitWidht"></param>
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
    public int TargetCount { get; set; }

    /// <summary>
    /// 方向
    /// </summary>
    public Vector3 Direction { get; set; }

    // ------------------------------公有属性--------------------------------


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
    /// 是否钻地
    /// </summary>
    public bool IsHideZD { get; set; }

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
    /// 选择钻地隐形单位权重
    /// </summary>
    public float HideZDWeight { get; set; }

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
    /// 是否钻地
    /// </summary>
    bool IsHideZD { get; set; }

    /// <summary>
    /// 是否嘲讽
    /// </summary>
    bool IsTaunt { get; set; }
    float Speed { get; set; }
    int MaxHealth { get; set; }
    int Health { get; set; }
    int Atack { get; set; }
    int Define { get; set; }
    int Diameter { get; set; }
    int ScanDiameter { get; set; }
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



/// <summary>
/// 选择目标权重抽象类
/// TODO 改成接口, 不适用抽象类
/// </summary>
public interface ISelectWeightData
{
    // Level 1, 2, 3所有值都是从-1 - 正无穷, -1为完全不理会, 0为不影响权重, 权重越大越重要
    // Level 4的值 0 - 正无穷 不会出现完全不理会的情况


    // ----------------------------权重选择 Level1-----------------------------
    /// <summary>
    /// 选择地面单位权重
    /// </summary>
    float SurfaceWeight { get; set; }

    /// <summary>
    /// 选择天空单位权重
    /// </summary>
    float AirWeight { get; set; }

    /// <summary>
    /// 选择建筑权重
    /// </summary>
    float BuildWeight { get; set; }

    
    // ----------------------------权重选择 Level1-----------------------------

    /// <summary>
    /// 选择坦克权重
    /// </summary>
    float TankWeight { get; set; }

    /// <summary>
    /// 选择轻型载具权重
    /// </summary>
    float LVWeight { get; set; }

    /// <summary>
    /// 选择火炮权重
    /// </summary>
    float CannonWeight { get; set; }

    /// <summary>
    /// 选择飞行器权重
    /// </summary>
    float AirCraftWeight { get; set; }

    /// <summary>
    /// 选择步兵权重
    /// </summary>
    float SoldierWeight { get; set; }


    // ----------------------------权重选择 Level3-----------------------------
    /// <summary>
    /// 选择隐形单位权重
    /// </summary>
    float HideWeight { get; set; }

    /// <summary>
    /// 选择钻地隐形单位权重
    /// </summary>
    float HideZDWeight { get; set; }

    /// <summary>
    /// 选择嘲讽权重(这个值应该很大, 除非有反嘲讽效果的单位)
    /// </summary>
    float TauntWeight { get; set; }


    // ----------------------------权重选择 Level4-----------------------------

    
    /// <summary>
    /// 低生命权重
    /// </summary>
    float HealthMinWeight { get; set; }

    /// <summary>
    /// 高生命权重
    /// </summary>
    float HealthMaxWeight { get; set; }


    /// <summary>
    /// 近位置权重
    /// </summary>
    float DistanceMinWeight { get; set; }

    /// <summary>
    /// 远位置权重
    /// </summary>
    float DistanceMaxWeight { get; set; }

    /// <summary>
    /// 角度权重
    /// </summary>
    float AngleWeight { get; set; }


}

/// <summary>
/// 单位类型
/// </summary>
public enum MemberItemType
{
    Tank = 0,
    LV,
    Cannon,
    Aircraft,
    Soldier
}