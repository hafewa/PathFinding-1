using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
        MemberList = new TargetList<Member>(MapPlane.transform.localPosition.x, MapPlane.transform.localPosition.z, MapWidth, MapHeight);

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
        var item = _leader;
        // 根据对象的搜寻外径获取对向列表
        var itemRect = new Rectangle(item.X - item.ScanDiameter / 2f, item.Y - item.ScanDiameter / 2f, item.ScanDiameter,
            item.ScanDiameter);
        var inScopeList = MemberList.QuadTree.GetScope(itemRect);
        DrawRect(itemRect, Color.gray);
        // 连线
        foreach (var targetItem in inScopeList)
        {
            Debug.DrawLine(new Vector3(item.X, 0, item.Y), new Vector3(targetItem.X, 0, targetItem.Y));
        }
       
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
            // 随机给方向
            member.Direction = new Vector3(random.Next(1, 10), 0, random.Next(1, 10));
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
    private GameObject CreateOneMember()
    {
        return GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
}



/// <summary>
/// 单位数据
/// </summary>
public class Member :  SelectWeightData, BaseMamber, IGraphical<Rectangle>
{
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
    /// 目标点
    /// </summary>
    public Vector3 Direction
    {
        get { return direction;}
        set { direction = value; }
    }

    private float speed = 4f;

    private int maxHealth = 100;

    private int health = 100;

    private int atack = 10;

    private int define = 10;

    private int memberType = 1;

    private int diameter = 1;

    private int scanDiameter = 40;

    private float x = 0;

    private float y = 0;

    public string Name = "";

    /// <summary>
    /// 目标点
    /// </summary>
    public Vector3 direction;

    


    /// <summary>
    /// 单位矩形占位
    /// </summary>
    private Rectangle _rect = null;

    private float _hisX = 0;

    private float _hisY = 0;

    private int _hisDimeter = 0;


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
    float Speed { get; set; }
    int MaxHealth { get; set; }
    int Health { get; set; }
    int Atack { get; set; }
    int Define { get; set; }
    int MemberType { get; set; }
    int Diameter { get; set; }
    int ScanDiameter { get; set; }
    float X { get; set; }
    float Y { get; set; }


    /// <summary>
    /// 目标点
    /// </summary>
    Vector3 Direction { get; set; }
}