﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DG.Tweening;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// 集群管理
/// 集群行为都在这里集中实现
/// </summary>
public class SchoolManager : MonoBehaviour
{
    // -------------------------公有属性-------------------------------
    public Vector3 MovementPlanePosition;


    public float MovementWidth;


    public float MovementHeight;
    
    /// <summary>
    /// 判定前方角度
    /// 在单位前方ForwardAngle角度内为该单位forward
    /// </summary>
    public float ForwardAngle = 90;

    /// <summary>
    /// 碰撞拥挤权重
    /// </summary>
    public float CollisionWeight = 1f;

    ///// <summary>
    ///// 碰撞挤开系数
    ///// </summary>
    public float CollisionThrough = 5f;

    /// <summary>
    /// 摩擦力系数
    /// </summary>
    public float Friction = 5;

    /// <summary>
    /// 组列表(全局)
    /// </summary>
    public static List<SchoolGroup> GroupList = new List<SchoolGroup>();


    // -------------------------私有属性-------------------------------

    /// <summary>
    /// 极限速度
    /// </summary>
    private float upTopSpeed = 100f;

    /// <summary>
    /// 目标列表
    /// </summary>
    private TargetList<PositionObject> targetList; 


    /// <summary>
    /// 已对比碰撞对象ID的列表
    /// </summary>
    private Dictionary<long, bool> areadyCollisionList = new Dictionary<long, bool>();

    /// <summary>
    /// 单位格子宽度
    /// </summary>
    private int unitWidth = 1;


    // -----------------------------公有方法------------------------------

    public void Start()
    {

    }

    public void Update()
    {
        // 刷新四叉树
        targetList.RebuildQuadTree();
        // 刷新地图对应位置
        targetList.RebulidMapInfo();
        // 单位移动
        AllMemberMove(targetList.List);
        // 绘制四叉树
        DrawQuadTreeLine(targetList.QuadTree);
    }



    /// <summary>
    /// 加入单位
    /// </summary>
    /// <param name="member">单位</param>
    public void Add(PositionObject member)
    {
        targetList.Add(member);
    }

    public void Init(float x, float y, int w, int h, int unitw, int[][] map)
    {
        targetList = new TargetList<PositionObject>(x, y, w, h, unitw);
        targetList.MapInfo = new MapInfo<PositionObject>();
        targetList.MapInfo.AddMap(unitw, w, h, map);
        MovementHeight = h;
        MovementWidth = w;
        unitWidth = unitw;
    }

    /// <summary>
    /// 清理现有对象
    /// </summary>
    public static void ClearAllGroup()
    {
        foreach (var group in GroupList)
        {
            group.CleanGroup();
        }
        GroupList.Clear();
    }


    // ------------------------私有方法--------------------------


    /// <summary>
    /// 所有成员判断组队行进与碰撞
    /// </summary>
    /// <param name="memberList">成员列表</param>
    private void AllMemberMove(IList<PositionObject> memberList)
    {
        // 验证数据有效性
        if (memberList == null || memberList.Count == 0)
        { return; }

        // 前方角度/2
        var cosForwardAngle = (float)Math.Cos(ForwardAngle / 2f);
        // 遍历所有成员
        for (var i = 0; i < memberList.Count; i++)
        {
            // 当前成员
            var member = memberList[i];
            if (member is SchoolBehaviour)
            {
                OneMemberMove(member as SchoolBehaviour, cosForwardAngle);
            }
            else if (member is FixtureBehaviour)
            {
                // 不移动
                // TODO 是否对周围产生斥力?
            }
        }

        // 清空对比列表
        areadyCollisionList.Clear();
    }

    /// <summary>
    /// 可移动单位移动
    /// </summary>
    /// <param name="member">单个单位</param>
    /// <param name="cosForwardAngle">前方角度</param>
    private void OneMemberMove(SchoolBehaviour member, float cosForwardAngle)
    {
        if (member == null || !member.IsMoving)
        {
            return;
        }

        ChangeMemberState(member);
        // 当前单位到目标的方向
        Vector3 targetDir = member.TargetPos - member.Position;
        // 转向角度
        float rotate = 0f;
        // 标准化目标方向
        Vector3 normalizedTargetDir = targetDir.normalized;
        // 计算后最终方向
        var finalDir = GetGroupGtivity(member, cosForwardAngle);
        // 当前方向与目标方向夹角
        var angleForTarget = Vector3.Dot(normalizedTargetDir, member.Direction);

        // 当前单位位置减去周围单位的位置的和, 与最终方向相加, 这个向量做处理, 只能指向目标方向的左右90°之内, 防止调头
        // 获取周围成员(不论敌友, 包括障碍物)的斥力引力
        // 直线移动防止抖动
        if (angleForTarget < 0.999f)
        {
            // 计算转向
            rotate = Vector3.Dot(finalDir.normalized, member.DirectionRight) * 180;
            if (rotate > 180 || rotate < -180)
            {
                rotate += ((int)rotate / 180) * 180 * (Mathf.Sign(rotate));
            }
        }

        // 目标没有在前方 则停止并转向目标到前方角度内
        //if (angleForTarget < cosForwardAngle)
        //{
        //    member.PhysicsInfo.SpeedDirection *= 0f;
        //    // 切换等待状态
        //}
        //else
        //{
        //    // 角度越小速度约接近原始速度
        //    member.PhysicsInfo.SpeedDirection *= angleForTarget;
        //    // 切换行进状态
        //}


        // 转向
        member.Rotate = Vector3.up * rotate * member.RotateSpeed * Time.deltaTime;
        // 前进
        // TODO speed 用作引力产生系数用
        member.Position += member.PhysicsInfo.SpeedDirection * Time.deltaTime;
        GetCloseMemberGrivity(member);
        // 速度由于摩擦力的原因衰减
        member.PhysicsInfo.SpeedDirection -= member.PhysicsInfo.SpeedDirection.normalized * Friction * Time.deltaTime;
        // TODO 最大速度限制, 方式有待确认
        var speed = member.PhysicsInfo.SpeedDirection.magnitude;
        if (speed > member.PhysicsInfo.MaxSpeed)
        {
            member.PhysicsInfo.SpeedDirection *= member.PhysicsInfo.MaxSpeed/speed;
        }
    }

    /// <summary>
    /// 计算队伍引力
    /// </summary>
    /// <param name="member">队员对象</param>
    /// <param name="cosForwardAngle">前方角度</param>
    /// <param name="speed"></param>
    /// <returns></returns>
    private Vector3 GetGroupGtivity(SchoolBehaviour member, float cosForwardAngle)
    {
        var result = Vector3.zero;
        
        // 同队伍聚合
        if (member != null && member.Group != null)
        {
            var grivity = member.TargetPos - member.Position;
            // 当前单位到目标的方向
            //Vector3 targetDir = member.TargetPos - member.Position;
            
            // 遍历同队成员计算方向与速度
            //for (var j = 0; j < member.Group.MemberList.Count; j++)
            //{
            //    var teammate = member.Group.MemberList[j];
            //    // 排除自己
            //    if (member.Equals(teammate)) { continue; }
            //    // 计算与队友位置差
            //    var teammateOffset = teammate.Position - member.Position;
            //    // 该向量与目标方向的夹角
            //    var teammateAngleOffset = Vector3.Dot(teammateOffset.normalized, targetDir.normalized);
            //    // 判断队友是否在当前单位前方一定角度内
            //    if (teammateAngleOffset > cosForwardAngle)
            //    {
            //        // 在跟随区间
            //        var minDistance = member.Diameter + member.Diameter;
            //        var maxDistance = member.MaxDistance + member.Diameter;
            //        if (teammateOffset.magnitude > minDistance && teammateOffset.magnitude < maxDistance)
            //        {
            //            // 向前方队友位置偏移
            //            grivity += teammateOffset.normalized * member.RotateWeight * (1 - teammateOffset.magnitude / minDistance);
            //        }
            //        else if (teammateOffset.magnitude <= minDistance)
            //        {
            //            // 前方90度内有人, 并且距离小于改单位设置的最小间距, 对该方向产生斥力
            //            grivity -= teammateOffset.normalized * member.RotateWeight * (teammateOffset.magnitude / minDistance);
            //        }
            //    }
            //}

            //grivity = grivity.normalized + targetDir.normalized;

            //member.Momentum = member.PhysicsInfo.Momentum;
            //if (member.PhysicsInfo.Momentum < 0)
            //{
            //    member.PhysicsInfo.Momentum = 0;
            //    member.PhysicsInfo.Speed = 0;
            //}
            //else
            //{
            //    member.PhysicsInfo.Speed = member.PhysicsInfo.MaxSpeed;
            //}
            // TODO 走向队友附近的槽, 并且前方有障碍则绕开, 绕开不太好做啊.
            // 绘制目标
            Debug.DrawLine(member.Position, grivity + member.Position);
            // 操作动量, 产生前进动量, 这个动量不会超过引力方向最大速度
            // TODO 速度不稳定问题
            member.PhysicsInfo.SpeedDirection += grivity.normalized * member.PhysicsInfo.MaxSpeed * CollisionWeight * Time.deltaTime;

            // 加入最大速度限制, 防止溢出
            member.PhysicsInfo.SpeedDirection *= GetUpTopSpeed(member.PhysicsInfo.SpeedDirection.magnitude);
            result = grivity;
        }

        return result;
    }

    /// <summary>
    /// 获取同区域内成员引力斥力
    /// </summary>
    /// <param name="member"></param>
    /// <returns></returns>
    private void GetCloseMemberGrivity(SchoolBehaviour member)
    {
        if (member == null)
        {
            return;
        }
        // 遍历附近单位(不论敌友), 检测碰撞并排除碰撞, (挤开效果), 列表中包含障碍物
        var closeMemberList = targetList.QuadTree.Retrieve(member.GetGraphical());
        var rect = member.GetGraphical();
        // 目标方向
        var targetDir = member.TargetPos - member.Position;
        // 释放压力方向
        var pressureReleaseDir = Vector3.zero;
        // 是否需要躲避
        var collisionCount = 0;
        for (var k = 0; k < closeMemberList.Count; k++)
        {
            var closeMember = closeMemberList[k];
            if (closeMember.Equals(member))
            {
                continue;
            }

            // 计算周围人员的位置, 相对位置的倒数相加, 并且不往来时方向移动
            var diffPosition = member.Position - closeMember.Position;
            pressureReleaseDir -= diffPosition;
            // 判断两对象是否以计算过, 如果计算过不再计算
            var compereId1 = member.Id + closeMember.Id << 32;
            var compereId2 = closeMember.Id + member.Id << 32;
            if (!areadyCollisionList.ContainsKey(compereId1) &&
                !areadyCollisionList.ContainsKey(compereId2))
            {
                var closeRect = closeMember.GetGraphical();
                if (rect.IsCollision(closeRect))
                {
                    // 如果碰撞来自前方, 则增加
                    if (Vector3.Angle(targetDir, -diffPosition) < 90)
                    {
                        collisionCount++;
                    }

                    var minDistance = member.Diameter + closeMember.Diameter;

                    var departSpeed = closeMember.PhysicsInfo.SpeedDirection - member.PhysicsInfo.SpeedDirection;
                    
                    // 基础排斥力
                    if (diffPosition.magnitude < minDistance)
                    {
                        // TODO 不放在这里 直接控制位置
                        member.Position += diffPosition.normalized * (minDistance - diffPosition.magnitude) * CollisionThrough * Time.deltaTime;
                    }

                    // 求出射角度, 出射角度*出射量
                    // 使用向量法线计算求出出射标准向量
                    var outDir =
                        ((member.PhysicsInfo.SpeedDirection +
                          Vector3.Dot(member.PhysicsInfo.SpeedDirection, diffPosition) *diffPosition)*2 -
                         member.PhysicsInfo.SpeedDirection).normalized;
                    

                    // 质量比例
                    var qualityRate = member.PhysicsInfo.Quality * member.PhysicsInfo.Quality / (closeMember.PhysicsInfo.Quality * closeMember.PhysicsInfo.Quality);
                    departSpeed *= 0.5f;

                    // 当前对象的弹出角度为镜面弹射角度
                    var partForMember = -outDir * departSpeed.magnitude/qualityRate;
                    var partForCloseMember = departSpeed*qualityRate;
                    if (partForMember.magnitude > departSpeed.magnitude)
                    {
                        partForMember *= departSpeed.magnitude/partForMember.magnitude;
                    }
                    if (partForCloseMember.magnitude > departSpeed.magnitude)
                    {
                        partForCloseMember *= departSpeed.magnitude / partForCloseMember.magnitude;
                    }
                    member.PhysicsInfo.SpeedDirection += partForMember;
                    closeMember.PhysicsInfo.SpeedDirection -= partForCloseMember;
                    // 加入最大速度限制, 防止溢出
                    member.PhysicsInfo.SpeedDirection *= GetUpTopSpeed(member.PhysicsInfo.SpeedDirection.magnitude);
                    closeMember.PhysicsInfo.SpeedDirection *= GetUpTopSpeed(closeMember.PhysicsInfo.SpeedDirection.magnitude);
                    // 加入已对比列表
                    areadyCollisionList.Add(compereId1, true);
                    Debug.DrawLine(member.Position, partForMember + member.Position, Color.green);
                    //DrawRect(rect, Color.black);
                    //DrawRect(closeRect, Color.black);
                }
            }
        }

        // 判断是否需要躲避
        if (collisionCount > 1)
        {
            // TODO 引力方向是附近的空格子
            // 获取周围的格子
            //var aroundNodes = targetList.MapInfo.GetAroundPos(member, 2);
            // 给予横向拉扯力
            // 求聚合位置向量的垂直向量
            var transverseDir = Vector3.Cross(pressureReleaseDir, Vector3.up);
            // 随机左右
            member.PhysicsInfo.SpeedDirection += transverseDir * member.PhysicsInfo.MaxSpeed * (new Random(DateTime.Now.Second).Next(10) > 5 ? -1 : 1);
        }
    }


    private void ChangeMemberState(SchoolBehaviour member)
    {
        if (member == null)
        {
            return;
        }

        if (member.State == SchoolItemState.Unstart)
        {
            member.State = SchoolItemState.Moving;
            if (member.Moveing != null)
            {
                member.Moveing(member.ItemObj);
            }
        }

        if (member.PhysicsInfo.SpeedDirection.magnitude < 1)
        {
            // 开始等待
            if (member.State != SchoolItemState.Waiting && member.State != SchoolItemState.Complete)
            {
                member.State = SchoolItemState.Waiting;
                if (member.Wait != null)
                {
                    member.Wait(member.ItemObj);
                }
            }
        }
        else
        {
            // 根据角度获得差速, 直线移动最快
            if (member.State != SchoolItemState.Moving && member.State != SchoolItemState.Complete)
            {
                // 结束等待, 开始移动
                member.State = SchoolItemState.Moving;
                if (member.Moveing != null)
                {
                    member.Moveing(member.ItemObj);
                }
            }
        }

        if ((member.Position - member.Group.Target).magnitude < member.Diameter)
        {
            if (member.State != SchoolItemState.Complete)
            {
                // 单位状态修改为complete
                member.State = SchoolItemState.Complete;
                // 调用到达
                if (member.Complete != null) { member.Complete(member.ItemObj); }
                member.Group.CompleteMemberCount++;
            }
        }

        // 判断组队是否到达
        if (!member.Group.IsComplete && member.Group.CompleteMemberCount * 100 / member.Group.MemberList.Count > member.Group.ProportionOfComplete)
        {
            if (member.Group.Complete != null)
            {
                member.Group.IsComplete = true;
                member.Group.Complete(member.Group);
            }
        }
    }


    /// <summary>
    /// 控制极限速度
    /// </summary>
    /// <param name="speed">当前速度</param>
    /// <returns>如果speed超过极限速度则将其置为极限速度系数</returns>
    private float GetUpTopSpeed(float speed)
    {
        var result = 1f;
        if (speed > upTopSpeed)
        {
            result = upTopSpeed / speed;
        }
        return result;
    }


    /// <summary>
    /// 绘制单元位置与四叉树分区情况
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="argQuadTree"></param>
    private void DrawQuadTreeLine<T>(QuadTree<T> argQuadTree) where T : IGraphical<Rectangle>
    {
        // 绘制四叉树边框
        DrawRect(argQuadTree.GetRectangle(), Color.white);
        // 遍历四叉树内容
        foreach (var item in argQuadTree.GetItemList())
        {
            // 绘制当前对象
            DrawRect(item.GetGraphical(), Color.green);
        }

        if (argQuadTree.GetSubNodes()[0] != null)
        {
            foreach (var node in argQuadTree.GetSubNodes())
            {
                DrawQuadTreeLine(node);
            }
        }
    }


    /// <summary>
    /// 绘制矩形
    /// </summary>
    /// <param name="rectangle">被绘制矩形</param>
    /// <param name="color">绘制颜色</param>
    private void DrawRect(Rectangle rectangle, Color color)
    {
        Debug.DrawLine(new Vector3(rectangle.X, 0, rectangle.Y), new Vector3(rectangle.X, 0, rectangle.Y + rectangle.Height), color);
        Debug.DrawLine(new Vector3(rectangle.X, 0, rectangle.Y), new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y), color);
        Debug.DrawLine(new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y + rectangle.Height), new Vector3(rectangle.X, 0, rectangle.Y + rectangle.Height), color);
        Debug.DrawLine(new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y + rectangle.Height), new Vector3(rectangle.X + rectangle.Width, 0, rectangle.Y), color);
    }

    /// <summary>
    /// 清除所有组
    /// </summary>
    public void ClearAll(){
        // 清除已有所有单元
        foreach (var group in GroupList)
        {
            group.CleanGroup();
        }

        GroupList.Clear();
        targetList.List.Clear();
    }

    /// <summary>
    /// 根据ID查询group
    /// </summary>
    /// <param name="groupId">被查询groupId</param>
    /// <returns>返回查询到的groupId 如果不存在则返回null</returns>
    public static SchoolGroup GetGroupById(int groupId)
    {
        for (var i = 0; i < GroupList.Count; i++)
        {
            var tmpGroup = GroupList[i];
            if (tmpGroup.GroupId == groupId)
            {
                return tmpGroup;
            }
        }
        return null;
    }

}