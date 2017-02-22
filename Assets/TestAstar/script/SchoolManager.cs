﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// 集群管理
/// 集群行为都在这里集中实现
/// </summary>
public class SchoolManager : MonoBehaviour
{

    public Vector3 MovementPlanePosition;


    public float MovementWidth;


    public float MovementHeight;
    
    /// <summary>
    /// 判定前方角度
    /// 在单位前方ForwardAngle角度内为该单位forward
    /// </summary>
    public float ForwardAngle = 90;
    /// <summary>
    /// 组列表(全局)
    /// </summary>
    public static List<SchoolGroup> GroupList = new List<SchoolGroup>();


    /// <summary>
    /// 四叉树对象
    /// </summary>
    private QuadTree<SchoolBehaviour> quadTree; 

    public void Start()
    {
        // 初始化四叉树
        quadTree = new QuadTree<SchoolBehaviour>(1, new Rectangle(MovementPlanePosition.x, MovementPlanePosition.z, MovementWidth, MovementHeight));
    }

    public void Update()
    {
        // TODO 将所有单位放入四叉树
        for (var i = 0; i < GroupList.Count; i++)
        { quadTree.Insert(GroupList[i].MemberList); }
        // 挨个更新队伍位置
        for (var i = 0; i < GroupList.Count; i++)
        {
            GroupMove(GroupList[i]);
        }
        // TODO 清空四叉树
        quadTree.Clear();
    }

    /// <summary>
    /// 组队移动
    /// </summary>
    /// <param name="group">组队对象</param>
    private void GroupMove(SchoolGroup group)
    {
        var forwardAngle = Math.Cos(ForwardAngle / 2);
        // 当前已到达状态成员个数
        var completeCount = 0;
        // 队伍到达状态
        var groupComlete = false;
        // 遍历Counter
        var j = 0;
        var k = 0;
        // 编队成员
        SchoolBehaviour member = null;

        // 目标方向
        Vector3 dir;
        // 最终角度
        Vector3 finalDir;
        // 转向角度
        var rotateDir = 0f;
        // 移动速度
        float speed;
        // 单位与其他单位位置的差
        Vector3 offset;
        // 最终方向(朝向终点)与队友方向(朝向队友)的cos夹角值
        float sign;
        // 当前单位的方向与最终方向的cos夹角值
        float angleForTarget;
        // 集群中心点
        Vector3 averagePos = Vector3.zero;

        // 起点到终点的向量
        var pathVec = group.Target - group.StartPos;

        // 引力状态
        var isBackPath = false;

        // 求出当前位置与目标位置所构成向量与X轴正方向的角度
        var angleForVector = Math.Atan2(group.StartPos.x - group.Target.x, group.StartPos.z - group.Target.z) * 180 / Math.PI;
        // 路径点四角偏移量
        var offsetVector = Quaternion.Euler(new Vector3(0, (float)angleForVector, 0)) * new Vector3(1, 0, 0) * group.PathWeight;

        // 求出路径对角位置
        Vector3 pathCorner5 = group.Target.magnitude > group.StartPos.magnitude ? group.Target - offsetVector : group.StartPos - offsetVector;
        Vector3 pathCorner6 = group.Target.magnitude > group.StartPos.magnitude ? group.StartPos + offsetVector : group.Target + offsetVector;

        // 绘制路径宽度
        Vector3 pathCorner1 = group.Target + offsetVector;
        Vector3 pathCorner2 = group.Target - offsetVector;
        Vector3 pathCorner3 = group.StartPos + offsetVector;
        Vector3 pathCorner4 = group.StartPos - offsetVector;
        Debug.DrawLine(pathCorner1, pathCorner2);
        Debug.DrawLine(pathCorner2, pathCorner4);
        Debug.DrawLine(pathCorner4, pathCorner3);
        Debug.DrawLine(pathCorner3, pathCorner1);
        // 对角线
        Debug.DrawLine(pathCorner5, pathCorner6);
        //Debug.Log(string.Format("{0},{1},{2},{3},{4}", 
        //    pathCorner1, 
        //    pathCorner2, 
        //    group.GroupPos.x - group.Target.x, 
        //    group.GroupPos.z - group.Target.z, 
        //    Math.Atan2(group.GroupPos.x - group.Target.x, group.GroupPos.y - group.Target.y)));
        
        // 计算方向与速度
        for (j = 0; j < group.MemberList.Count; j++)
        {
            // 重置引力状态
            isBackPath = false;
            member = group.MemberList[j];
            speed = member.Speed;

            // 求平均位置
            averagePos += member.Position;
            // 如果该单位完成移动则跳过
            //if (member.State == SchoolItemState.Complete)
            //{
            //    continue;
            //}

            // 判断状态 调用开始移动 变更状态
            if (member.State == SchoolItemState.Unstart)
            {
                member.State = SchoolItemState.Moving;
                if (member.Moveing != null)
                {
                    member.Moveing(member.ItemObj);
                }
            }

            // 计算与目标的相对向量
            dir = group.Target - member.Position;
            // 经过计算后的最终方向
            finalDir = dir.normalized;

            // 遍历同队队友
            for (k = 0; k < group.MemberList.Count; k++)
            {
                var otherMember = group.MemberList[k];
                // 排除自己的情况
                if (otherMember.Equals(member))
                {
                    continue;
                }

                // 计算单位与队友的位置差向量
                offset = otherMember.Position - member.Position;
                // 计算最终方向与队友方向夹角
                sign = Vector3.Dot(dir.normalized, offset.normalized);
                // 判断与队友的相对位置是否在该单位前方
                if (sign > forwardAngle)
                {
                    if (offset.magnitude > member.Distance && offset.magnitude < member.MaxDistance)
                    {
                        // 在跟随区间
                        // 前方90度内有人, 并且距离小于改单位设置的最小间距
                        // 向前方队友位置偏移
                        finalDir += offset.normalized * member.RotateWeight * (1 - offset.magnitude / member.Distance);
                    }
                    else if (offset.magnitude <= member.Distance)
                    {
                        // 小于追随距离, 该方向减速
                        speed *= offset.magnitude / member.Distance;
                    }
                }

                // 判断侧向队友, 如果侧向队友靠太近则减少该方向移动速度, 如果太远则向该队友偏移
                if (sign < forwardAngle && sign > -forwardAngle)
                {
                    if (offset.magnitude > member.Distance && offset.magnitude < member.MaxDistance)
                    {
                        finalDir += offset.normalized * member.RotateWeight * (1 - offset.magnitude / member.Distance);
                    }
                    else if (offset.magnitude <= member.Distance)
                    {
                        // 小于追随距离, 该方向减速
                        //speed *= offset.magnitude / member.Distance;
                        finalDir -= offset.normalized * member.RotateWeight;
                    }
                }
                
            }

            // 判断当前位置与路径边界 超过边界给予向路径区域内的引力
            if (!Utils.IsCoverage(pathCorner5.x, pathCorner5.z, pathCorner6.x, pathCorner6.z, member.Position.x,
                member.Position.z))
            {
                // 增加引力
                // 求位置点位置到路径向量的垂直向量
                var itemVec = member.Position - group.StartPos;
                // 获得起点到该单元位置与起点到终点向量的夹角
                var cosTheta = Vector3.Dot(itemVec.normalized, pathVec.normalized);
                // 用起点到单元的向量长度乘以cos值获得向量长度
                var vecLenForTmp = itemVec.magnitude*cosTheta;
                // 中间向量长度乘以起点到终点向量的标准向量
                var midVec = pathVec.normalized*vecLenForTmp;
                // 获得引力向量
                var backPowerVec = midVec - itemVec;
                // 最终向量+该垂直向量获得引力
                //Debug.Log(string.Format("{0},{1},{2},{3},{4},{5}", itemVec,
                //    cosTheta,
                //    vecLenForTmp,
                //    midVec,
                //    backPowerVec,
                //    finalDir));
                // Debug.Log(backPowerVec);
                finalDir += backPowerVec;
                isBackPath = true;
            }

            // 如果目标没有在前方角度以内, 则停止前进, 进行转向
            angleForTarget = Vector3.Dot(dir.normalized, member.Direction);


            // TODO 增加与引力无关的强制无法前进方向停止前进
            // TODO 该计算为所有单位, 包括其他队伍单位
            // TODO 这个实现破坏结构需要重构, 结构设计问题
            // 减去该方向的力
            // 前方有人完全无法走动情况减速
            var closeMemberList = quadTree.Retrieve(member);

            foreach (var closeMember in closeMemberList)
            {
            //    // 判断是否有碰撞
                if (member.GetGraphical().IsCollision(closeMember.GetGraphical()))
                {
            //        // 有碰撞则获取两单位的连接向量, 并从原方向中去掉该方向
            //        var vec = closeMember.Position - member.Position;
            //        // 获得向量减去, 向量的量是多少?
            //        var per = Vector3.Dot(vec.normalized, finalDir.normalized);
            //        finalDir -= vec.normalized * per;
            //        // 求两向量夹角, 并sin该角度获得比例, 乘以原向量长度为最终长度

            //        // 用求出来的比例乘以速度, 降低速度
            //        speed = speed*(1 - per);
                }
            }

            // 直线运动防止抖动
            if (angleForTarget < 0.999f)
            {
                rotateDir = Vector3.Dot(finalDir, member.DirectionRight);
                rotateDir = rotateDir * 180;
                // 规范化值 超过180或-180则将值回归180与-180以内
                if (rotateDir > 180 || rotateDir < -180)
                {
                    rotateDir += ((int) rotateDir/180)*180*(Mathf.Sign(rotateDir));
                }
                // Debug.Log(rotateDir);
            }

            // 如果在路径内行进
            if (!isBackPath)
            {
                // 目标没有在前方角度以内, 停止移动只旋转
                if (angleForTarget < forwardAngle)
                {
                    speed = 0;
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
                    speed = speed * angleForTarget;
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
            }


            // 求出方向, 想这个方向旋转, 然后向该单位的正前方移动
            // TODO 应使用对象相对up 旋转
            member.Rotate = Vector3.up * rotateDir * member.RotateSpeed * Time.deltaTime;
            // 向前方移动, 转向单独处理
            member.Position += member.Direction * speed * Time.deltaTime;

            // 判断单位是否到达.
            if ((member.Position - group.Target).magnitude < member.Distance)
            {
                completeCount++;
                if (member.State != SchoolItemState.Complete)
                {
                    // 单位状态修改为complete
                    member.State = SchoolItemState.Complete;
                    // 调用到达
                    if (member.Complete != null) { member.Complete(member.ItemObj); }
                }
            }

            // 判断组队是否到达
            if (!groupComlete && completeCount * 100 / group.MemberList.Count > group.ProportionOfComplete)
            {
                if (group.Complete != null)
                {
                    // TODO 会调用多次
                    group.Complete(group);
                }
                groupComlete = true;
            }
        }
        // 组队平均位置
        averagePos = new Vector3(averagePos.x / group.MemberList.Count, averagePos.y / group.MemberList.Count, averagePos.z / group.MemberList.Count);
        group.GroupPos = averagePos;
    }

    /// <summary>
    /// 清除所有组
    /// </summary>
    public static void ClearAllGroup()
    {
        // 清除已有所有单元
        foreach (var group in GroupList)
        {
            group.CleanGroup();
        }

        GroupList = new List<SchoolGroup>();
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