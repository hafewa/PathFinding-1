﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using UnityEngine;

/// <summary>
/// 集群行为
/// </summary>
public class SchoolBehaviour : MonoBehaviour, IGraphical<Rectangle>
{

    // ---------------------------设置属性-----------------------------
    /// <summary>
    /// 物理信息
    /// </summary>
    public PhysicsInfo PhysicsInfo{
        get { return physicsInfo ?? (physicsInfo = new PhysicsInfo()); }
        set { physicsInfo = value; }
    }

    /// <summary>
    /// 与其他单位间距
    /// </summary>
    public float Distance {
        get { return distance; }
        set { distance = value < 0 ? 0 : value; }
    }

    /// <summary>
    /// 与其他单位组队最大距离
    /// 超过该距离则不与该单位组队
    /// </summary>
    public float MaxDistance {
        get { return maxDistance;}
        set { maxDistance = value < 0 ? 10 : value; }
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    public float Speed {
        get { return PhysicsInfo.MaxSpeed; }
        set { PhysicsInfo.MaxSpeed = value < 0 ? 1 : value; }
    }

    /// <summary>
    /// 旋转速度
    /// </summary>
    public float RotateSpeed {
        get { return rotateSpeed; }
        set { rotateSpeed = value < 0 ? 1 : value; }
    }

    /// <summary>
    /// 转向权重
    /// 值越大, 转向越快
    /// </summary>
    public float RotateWeight {
        get { return rotateWeight; }
        set { rotateWeight = value < 0 ? 1 : value; }
    }

    /// <summary>
    /// 单元直径
    /// </summary>
    public int Diameter{
        get { return diameter;}
        set { diameter = value < 0 ? 1 : value; }
    }



    public bool CouldObstruct{
        get { return couldObstruct; }
        set { couldObstruct = value; }
    }


    public Vector3 TargetPos{
        get { return targetPos; }
        set{
            // TODO 切换当前状态
            targetPos = value;
        }
    }


    //public float ShowSpeed;

    //public float ShowAngle;

    public float Momentum;

    /// <summary>
    /// 组队ID, 只读
    /// </summary>
    public int GroupId{
        get { return groupId; }
        set{
            // 判断是否已有group, 如果有并且ID不同, 则删除原有group中的member
            if (value != groupId){
                if (group != null){
                    group.MemberList.Remove(this);
                }

                var newGroup = SchoolManager.GetGroupById(value);
                if (newGroup == null){
                    newGroup = new SchoolGroup(value);
                    SchoolManager.GroupList.Add(newGroup);
                    group = newGroup;
                }
                else if (group == null || group.GroupId != newGroup.GroupId){
                    group = newGroup;
                }
                
                // group列表中是否有改ID的group, 如果有则插入其中, 如果没有则创建
                groupId = value;
                if (!group.MemberList.Contains(this)){
                    group.MemberList.Add(this);
                }
            }
        }
    }

    /// <summary>
    /// 单位当前状态
    /// </summary>
    public SchoolItemState State = SchoolItemState.Unstart;

    //-----------------------------只读属性----------------------------------

    /// <summary>
    /// 组队编号
    /// </summary>
    public SchoolGroup Group {
        get { return group; }
    }

    /// <summary>
    /// 当前位置引用
    /// 读取与设置的为GameObject的localPosition
    /// </summary>
    public Vector3 Position{
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
    }

    /// <summary>
    /// 设置旋转值
    /// </summary>
    public Vector3 Rotate{
        set { this.transform.Rotate(value); }
    }

    /// <summary>
    /// 返回当前单位的正前向量
    /// </summary>
    public Vector3 Direction{
        get { return this.transform.forward; }
    }

    public Vector3 DirectionRight{
        get { return this.transform.right; }
    }

    /// <summary>
    /// 当前对象的gameobject的引用
    /// </summary>
    public GameObject ItemObj{
        get { return this.gameObject; }
    }

    /// <summary>
    /// 开始移动时调用
    /// </summary>
    public Action<GameObject> Moveing { get; set; }

    /// <summary>
    /// 拥挤等待时调用
    /// </summary>
    public Action<GameObject> Wait { get; set; }

    /// <summary>
    /// 移动到终点后调用
    /// 不是所有对象都会掉用该回调, 只有到达终点的会调用
    /// </summary>
    public Action<GameObject> Complete { get; set; }


    /// <summary>
    /// 物理信息
    /// </summary>
    private PhysicsInfo physicsInfo = new PhysicsInfo();

    /// <summary>
    /// 当前单位的目标点
    /// </summary>
    private Vector3 targetPos;

    /// <summary>
    /// 组编号
    /// </summary>
    private int groupId;

    /// <summary>
    /// 组对象
    /// </summary>
    private SchoolGroup group;

    /// <summary>
    /// 与其他单位间距
    /// </summary>
    private float distance = 1f;

    /// <summary>
    /// 与其他单位组队最大距离
    /// 超过该距离则不与该单位组队
    /// </summary>
    private float maxDistance = 10;

    /// <summary>
    /// 移动速度
    /// </summary>
    //private float speed = 10;

    /// <summary>
    /// 旋转速度
    /// </summary>
    private float rotateSpeed = 1;

    /// <summary>
    /// 转向权重
    /// 值越大, 转向越快
    /// </summary>
    private float rotateWeight = 1;

    /// <summary>
    /// 单元直径
    /// </summary>
    private int diameter = 1;

    /// <summary>
    /// 是否可被阻挡
    /// 如果为true则遇到其他单位则将其挤开
    /// 如果为false则减速等待
    /// </summary>
    private bool couldObstruct = true;


    private float hisX;


    private float hisY;


    private int hisDiameter;


    private Rectangle hisRectangle;


    /// <summary>
    /// 返回位置图形
    /// </summary>
    /// <returns>方形图形</returns>
    public Rectangle GetGraphical()
    {
        //var halfDiameter = Diameter * 0.5f;
        //var x = transform.localPosition.x - halfDiameter;
        //var y = transform.localPosition.z - halfDiameter;
        //var offsetX = hisX - x;
        //var offsetY = hisY - y;
        //// 值有变更时重新创建Rect
        //if (hisDiameter != diameter || offsetX > Utils.ApproachZero || offsetX < Utils.ApproachKZero ||
        //    offsetY > Utils.ApproachZero || offsetY < Utils.ApproachKZero || hisRectangle == null)
        //{
        //    hisX = x;
        //    hisY = y;
        //    hisDiameter = diameter;
        //    hisRectangle = new Rectangle(hisX, hisY, diameter, diameter);
        //}
        //return hisRectangle;

        //值有变更时重新创建Rect
        var halfDiameter = Diameter * 0.5f;
        var x = transform.localPosition.x - halfDiameter;
        var y = transform.localPosition.z - halfDiameter;
        if (hisDiameter != diameter || hisX - x > Utils.ApproachZero || x - hisX > Utils.ApproachZero ||
            hisY - y > Utils.ApproachZero || y - hisY > Utils.ApproachZero)
        {
            hisX = x;
            hisY = y;
            hisDiameter = diameter;
            if (hisRectangle == null)
            {
                //Debug.Log("1");
                hisRectangle = new Rectangle(x, y, diameter, diameter);
            }
            else
            {
                hisRectangle.X = x;
                hisRectangle.Y = y;
                hisRectangle.Width = diameter;
                hisRectangle.Height = diameter;
            }
        }
        return hisRectangle;
    }

    public SchoolBehaviour(int groupId)
    {
        this.GroupId = groupId;
    }

    public void Start()
    {
        // 将自己存入队员列表
        //Group.MemberList.Add(this);
    }

    public void Destory()
    {
        // 销毁时从列表中消除当前队员
        Group.MemberList.Remove(this);
    }
}




/// <summary>
/// 集群编队
/// </summary>
public class SchoolGroup
{
    /// <summary>
    /// 路径宽度
    /// </summary>
    public float PathWeight {
        get { return pathWeight; }
        set { pathWeight = value; } }

    /// <summary>
    /// 目标位置
    /// 设置目标的同时 所有成员状态会重置为未开始
    /// </summary>
    public Vector3 Target
    {
        get {
            if (MemberList != null || MemberList.Count > 0)
            {
                return MemberList[0].TargetPos;
            }
            return Vector3.zero;
        }
        set
        {
            // 一旦变更, 虽有所属成员状态全部变成Unstart
            foreach (var member in MemberList)
            {
                member.State = SchoolItemState.Unstart;
                member.TargetPos = value;
            }
            //targetPos = value;
            startPos = GroupPos;
        }
    }

    /// <summary>
    /// 队伍位置
    /// 这个位置代表队伍中心位置
    /// </summary>
    public Vector3 GroupPos { get; set; }

    /// <summary>
    /// 队伍起始位置
    /// </summary>
    public Vector3 StartPos { get { return startPos; } }

    /// <summary>
    /// 成员列表
    /// </summary>
    public List<SchoolBehaviour> MemberList = new List<SchoolBehaviour>();

    /// <summary>
    /// 组编号
    /// </summary>
    public int GroupId;

    /// <summary>
    /// 创建group时给予ID
    /// </summary>
    /// <param name="groupId"></param>
    public SchoolGroup(int groupId)
    {
        GroupId = groupId;
    }

    /// <summary>
    /// 达成Complete的百分比
    /// 比如集群10个单位, 此处填50, 则5个单位complete该group则complete
    /// </summary>
    public int ProportionOfComplete {
        get { return proportionOfComplete; }
        set
        {
            // 非法值
            if (value < 0 || value > 100)
            {
                proportionOfComplete = 20;
            }

            proportionOfComplete = value;
        }
    }

    /// <summary>
    /// 组队到达
    /// </summary>
    public Action<SchoolGroup> Complete;





    /// <summary>
    /// 达成Complete的百分比
    /// 默认20
    /// </summary>
    private int proportionOfComplete = 10;

    /// <summary>
    /// 目标位置
    /// </summary>
    //private Vector3 targetPos;

    /// <summary>
    /// 路径起始点
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// 路径宽度
    /// 默认宽度1
    /// </summary>
    public float pathWeight = 1.5f;

    /// <summary>
    /// 清除队伍信息
    /// </summary>
    public void CleanGroup()
    {
        // 清除所有从属单位

        foreach (var member in MemberList)
        {
            GameObject.Destroy(member.gameObject);
        }
        MemberList.Clear();
    }
}

/// <summary>
/// 集群单位状态
/// </summary>
public enum SchoolItemState
{
    // 未开始状态
    Unstart,
    // 移动中状态
    Moving,
    // 等待中状态
    Waiting,
    // 结束状态
    Complete,
}


/// <summary>
/// 集群物理信息
/// </summary>
public class PhysicsInfo
{

    /// <summary>
    /// 物理动量
    /// 动量 = 质量 * 速度
    /// 最大动量 = 质量 * 最大速度
    /// </summary>
    public float Momentum {
        get { return momentum; }
        set
        {
            //var maxMomentum = quality*speed;
            //momentum = value > maxMomentum ? maxMomentum : value;
            momentum = value;
        }
    }

    /// <summary>
    /// 物体质量
    /// </summary>
    public float Quality
    {
        get { return quality; }
        set { quality = value; }
    }

    /// <summary>
    /// 移动速度
    /// </summary>
    public float Speed
    {
        get { return speed; }
        set {
            //speed = value > maxSpeed ? maxSpeed : value;
            speed = value;
        }
    }

    /// <summary>
    /// 最大速度
    /// </summary>
    public float MaxSpeed
    {
        get { return maxSpeed;}
        set { maxSpeed = value; }
    }

    /// <summary>
    /// 动量
    /// </summary>
    private float momentum = 10;

    /// <summary>
    /// 质量
    /// </summary>
    private float quality = 1;

    /// <summary>
    /// 速度
    /// </summary>
    private float speed = 0;

    /// <summary>
    /// 最大速度
    /// </summary>
    private float maxSpeed = 10;
}