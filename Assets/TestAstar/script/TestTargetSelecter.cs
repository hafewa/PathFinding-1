﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// 测试目标选择器
/// </summary>
public class TestTargetSelecter : MonoBehaviour {

    /// <summary>
    /// 成员列表
    /// </summary>
    public TargetList<Member> MemberList = null;

    /// <summary>
    /// 地图底板
    /// </summary>
    public GameObject MapPlane;

    /// <summary>
    /// 地图宽度
    /// </summary>
    public int MapWidth;

    /// <summary>
    /// 地图高度
    /// </summary>
    public int MapHeight;

    /// <summary>
    /// 单元数量
    /// </summary>
    public int ItemCount = 100;

    /// <summary>
    /// 搜索对象
    /// </summary>
    private Member _leader;


    private Dictionary<int, Color> typeColor = new Dictionary<int, Color>(); 



    void Start ()
    {
        // 创建地图目标列表
        MemberList = new TargetList<Member>(MapPlane.transform.localPosition.x, MapPlane.transform.localPosition.z, MapWidth, MapHeight, 1);

        CreateAllMember(ItemCount);

        // 初始化类型颜色
        typeColor.Add(1, Color.white);
        typeColor.Add(2, Color.black);
        typeColor.Add(3, Color.yellow);
        typeColor.Add(4, Color.blue);
        typeColor.Add(5, Color.cyan);

    }



	void Update () {

        // 绘制四叉树内单元
	    DrawQuadTreeLine(MemberList.QuadTree);

        // 单元移动
	    MemberMove();

        // 搜寻目标
	    ScanTarget();

	}


    /// <summary>
    /// 单元移动
    /// </summary>
    private void MemberMove()
    {
        bool isTouchTopBorder = false;
        bool isTouchBottomBorder = false;
        bool isTouchRightBorder = false;
        bool isTouchLeftBorder = false;
        foreach (var member in MemberList.List)
        {
            // 判断是否碰到边框
            isTouchTopBorder = member.X + member.Diameter >= MapPlane.transform.localPosition.x + MapWidth;
            isTouchBottomBorder = member.X <= MapPlane.transform.localPosition.x;
            isTouchRightBorder = member.Y + member.Diameter >= MapPlane.transform.localPosition.z + MapHeight;
            isTouchLeftBorder = member.Y <= MapPlane.transform.localPosition.z;

            if (isTouchTopBorder || isTouchBottomBorder)
            {
                // X折射
                member.Direction = new Vector3((isTouchTopBorder ? -1 : 1) * Math.Abs(member.Direction.x), 0, member.Direction.z);
            }
            if (isTouchRightBorder || isTouchLeftBorder)
            {
                // Y折射
                member.Direction = new Vector3(member.Direction.x, 0, (isTouchRightBorder ? -1 : 1) * Math.Abs(member.Direction.z));
            }
            // 按照方向移动
            var step = member.Direction.normalized*member.Speed*Time.deltaTime;
            member.X += step.x;
            member.Y += step.z;
        }
        MemberList.RebuildQuadTree();
    }
    
    /// <summary>
    /// 绘制单元位置与四叉树分区情况
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="quadTree"></param>
    private void DrawQuadTreeLine<T>(QuadTree<T> quadTree) where T : BaseMamber, IGraphical<Rectangle>
    {
        // 绘制四叉树边框
        DrawRect(quadTree.GetRectangle(), Color.white);
        // 遍历四叉树内容
        foreach (var item in quadTree.GetItemList())
        {
            // 绘制当前对象
            DrawRect(item.GetGraphical(), typeColor[item.MemberType]);
            // 绘制前进方向
            var position = new Vector3(item.X, 0, item.Y);
            Debug.DrawLine(position, position + item.Direction.normalized * 2, Color.red);
        }

        if (quadTree.GetSubNodes()[0] != null)
        {
            foreach (var node in quadTree.GetSubNodes())
            {
                DrawQuadTreeLine(node);
            }
        }
    }



    /// <summary>
    /// 单元搜寻目标
    /// </summary>
    private void ScanTarget()
    {
        // 遍历对象,
        //foreach (var item in MemberList.List)
        //{

        var item = _leader;
        // 根据对象的搜寻外径获取对向列表
        var itemRect = new Rectangle(item.X - item.ScanDiameter/2f, item.Y - item.ScanDiameter/2f, item.ScanDiameter,
                item.ScanDiameter);
            DrawRect(itemRect, Color.red);

            // 根据策略筛选目标
            var targetList = TargetFilter(item, MemberList.QuadTree);

            // 连线
            foreach (var targetItem in targetList)
            {
                Debug.DrawLine(new Vector3(item.X, 0, item.Y), new Vector3(targetItem.X, 0, targetItem.Y));
            }
        //}
    }

    /// <summary>
    /// 筛选对象
    /// TODO 优化
    /// </summary>
    /// <typeparam name="T">对象类型. 必须继承</typeparam>
    /// <param name="searchObj">搜索对象</param>
    /// <param name="quadTree">四叉树</param>
    /// <returns></returns>
    private IList<T> TargetFilter<T>(T searchObj, QuadTree<T> quadTree) where T : ISelectWeightData, BaseMamber, IGraphical<Rectangle>
    {
        IList<T> result = null;
        if (searchObj != null && quadTree != null)
        {
            var inScope =
                quadTree.GetScope(new Rectangle(searchObj.X - searchObj.ScanDiameter / 2f, searchObj.Y - searchObj.ScanDiameter / 2f,
                    searchObj.ScanDiameter,
                    searchObj.ScanDiameter));

            var targetCount = searchObj.TargetCount;
            // 目标列表Array
            var targetArray = new T[targetCount];
            // 目标权重值
            var weightKeyArray = new float[targetCount];
            // 根据各项权重获取合适的目标
            // 生命值权重
            var healthWeight = searchObj.HealthWeight;
            // 角度权重
            var angleWeight = searchObj.AngleWeight;
            // 距离权重
            var distanceWeight = searchObj.DistanceWeight;
            // 等级权重
            //var levelWeight = searchObj.LevelWeight;

            for (var i = 0; i < inScope.Count; i++)
            {
                var item = inScope[i];
                if (item.Equals(searchObj))
                {
                    continue;
                }
                // 各项权重具体实现
                // 从列表中找到几项权重值最高的目标个数个单位
                // 将各项值标准化, 然后乘以权重求和, 得到最高值

                // 生命值标准化: 100 - 当前生命值/最大生命值*100
                float healthStand = 100 - item.Health * 100f / item.MaxHealth;
                // 距离标准化: 100 - 当前距离/最大距离*100
                float distanceStand = 100 -
                                    new Vector2(searchObj.X - item.X, searchObj.Y - item.Y).magnitude * 100f /
                                    searchObj.ScanDiameter;
                // 角度标准化: 100 - 当前角度 / 180 * 100
                var angle = Math.Acos(Vector3.Dot(searchObj.Direction.normalized,
                    new Vector3(item.X - searchObj.X, 0, item.Y - searchObj.Y).normalized));
                float angleStand = 100 -
                                 (float)angle * 100f / 180;

                // TODO 各项为插入式结构
                // 求权重和
                var sumWeight = healthStand*healthWeight + distanceStand*distanceWeight + angleStand*angleWeight;
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
        }
        return result;
    }

    /// <summary>
    /// 绘制矩形
    /// </summary>
    /// <param name="rectangle"></param>
    private void DrawRect(Rectangle rectangle, Color color)
    {
        Debug.DrawLine(new Vector3(rectangle.X, 0, rectangle.Y), new Vector3(rectangle.X, 0, rectangle.Y + rectangle.Height), color);
        Debug.DrawLine(new Vector3(rectangle.X, 0, rectangle.Y), new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y), color);
        Debug.DrawLine(new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y + rectangle.Height), new Vector3(rectangle.X, 0, rectangle.Y + rectangle.Height), color);
        Debug.DrawLine(new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y + rectangle.Height), new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y), color);
    }

    /// <summary>
    /// 创建测试单元
    /// </summary>
    /// <param name="count">创建单元个数</param>
    private void CreateAllMember(int count)
    {
        var random = new System.Random();
        // 测试 四叉树遇上超大目标
        for (var i = 0; i < count; i++)
        {
            var member = new Member();
            var x = random.Next(0, MapWidth);
            var y = random.Next(0, MapHeight);

            member.Name = "" + i;
            member.X = x;
            member.Y = y;
            member.MemberType = i%5 + 1;
            member.Health = i % 5 + 1;
            // 随机给方向
            member.Direction = new Vector3(random.Next(1, 100), 0, random.Next(1, 100));
            MemberList.Add(member);
            if (i == 0)
            {
                _leader = member;
            }
        }
    }

    /// <summary>
    /// 创建单个成员载体
    /// </summary>
    /// <returns></returns>
    //private GameObject CreateOneMember()
    //{
    //    return GameObject.CreatePrimitive(PrimitiveType.Cube);
    //}
}



/// <summary>
/// 单位数据
/// </summary>
public class Member : PositionObject, ISelectWeightData, BaseMamber, IGraphical<Rectangle>
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
        get { return targetCount;}
        set { targetCount = value; }
    }

    /// <summary>
    /// 目标点
    /// </summary>
    public Vector3 Direction
    {
        get { return direction;}
        set { direction = value; }
    }


    /// <summary>
    /// 生命权重
    /// </summary>
    public float HealthWeight {
        get { return healthWeight;}
        set { healthWeight = value; } }

    /// <summary>
    /// 位置权重
    /// </summary>
    public float DistanceWeight {
        get {return distanceWeight;}
        set { distanceWeight = value; } }

    /// <summary>
    /// 角度权重
    /// </summary>
    public float AngleWeight {
        get { return angleWeight; }
        set { angleWeight = value; } }

    /// <summary>
    /// 类型权重
    /// </summary>
    public float TypeWeight {
        get { return typeWeight; }
        set { typeWeight = value; } }

    /// <summary>
    /// 等级权重
    /// </summary>
    public float LevelWeight {
        get { return levelWeight; }
        set { levelWeight = value; } }


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