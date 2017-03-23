using System;
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
    /// 组列表(全局)
    /// </summary>
    public static List<SchoolGroup> GroupList = new List<SchoolGroup>();


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
        targetList = new TargetList<PositionObject>(x, y, w, h);
        targetList.MapInfo = new MapInfo<PositionObject>();
        targetList.MapInfo.AddMap(unitw, w, h, map);
    }

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
        // TODO 左右绕开
        // 当前单位位置减去周围单位的位置的和, 与最终方向相加, 这个向量做处理, 只能指向目标方向的左右90°之内, 防止调头
        // 获取周围成员(不论敌友, 包括障碍物)的斥力引力
        //finalDir += aroundDir.normalized;
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
        // 速度衰减
        member.PhysicsInfo.SpeedDirection *= 0.995f;
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
            Vector3 targetDir = member.TargetPos - member.Position;
            
            // 遍历同队成员计算方向与速度
            for (var j = 0; j < member.Group.MemberList.Count; j++)
            {
                var teammate = member.Group.MemberList[j];
                // 排除自己
                if (member.Equals(teammate)) { continue; }
                // 计算与队友位置差
                var teammateOffset = teammate.Position - member.Position;
                // 该向量与目标方向的夹角
                var teammateAngleOffset = Vector3.Dot(teammateOffset.normalized, targetDir.normalized);
                // 判断队友是否在当前单位前方一定角度内
                if (teammateAngleOffset > cosForwardAngle)
                {
                    // 在跟随区间
                    var minDistance = member.Diameter + member.Diameter;
                    var maxDistance = member.MaxDistance + member.Diameter;
                    if (teammateOffset.magnitude > minDistance && teammateOffset.magnitude < maxDistance)
                    {
                        // 向前方队友位置偏移
                        grivity += teammateOffset.normalized * member.RotateWeight * (1 - teammateOffset.magnitude / minDistance);
                    }
                    else if (teammateOffset.magnitude <= minDistance)
                    {
                        // 前方90度内有人, 并且距离小于改单位设置的最小间距, 对该方向产生斥力
                        //grivity -= teammateOffset.normalized * member.RotateWeight * (teammateOffset.magnitude / minDistance);
                    }
                }
            }

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

                    // 质量比例
                    var qualityRate = member.PhysicsInfo.Quality * member.PhysicsInfo.Quality / (closeMember.PhysicsInfo.Quality * closeMember.PhysicsInfo.Quality);
                    member.PhysicsInfo.SpeedDirection += departSpeed / qualityRate;
                    closeMember.PhysicsInfo.SpeedDirection -= departSpeed * qualityRate;
                    // 加入最大速度限制, 防止溢出
                    member.PhysicsInfo.SpeedDirection *= GetUpTopSpeed(member.PhysicsInfo.SpeedDirection.magnitude);
                    closeMember.PhysicsInfo.SpeedDirection *= GetUpTopSpeed(closeMember.PhysicsInfo.SpeedDirection.magnitude);
                    // 加入已对比列表
                    areadyCollisionList.Add(compereId1, true);
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
            member.PhysicsInfo.SpeedDirection += transverseDir * (new Random(DateTime.Now.Second).Next(10) > 5 ? -1 : 1);
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
            DrawRect(item.GetGraphical(), Color.white);
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



    ///// <summary>
    ///// 组队移动
    ///// </summary>
    ///// <param name="group">组队对象</param>
    //private void GroupMove(SchoolGroup group)
    //{ 
    //    var forwardAngle = Math.Cos(ForwardAngle / 2);
    //    // 当前已到达状态成员个数
    //    var completeCount = 0;
    //    // 队伍到达状态
    //    var groupComlete = false;
    //    // 遍历Counter
    //    var j = 0;
    //    var k = 0;
    //    // 编队成员
    //    SchoolBehaviour member = null;

    //    // 目标方向
    //    Vector3 dir;
    //    // 最终角度
    //    Vector3 finalDir;
    //    // 转向角度
    //    var rotateDir = 0f;
    //    // 移动速度
    //    float speed;
    //    // 单位与其他单位位置的差
    //    Vector3 offset;
    //    // 最终方向(朝向终点)与队友方向(朝向队友)的cos夹角值
    //    float sign;
    //    // 当前单位的方向与最终方向的cos夹角值
    //    float angleForTarget;
    //    // 集群中心点
    //    Vector3 averagePos = Vector3.zero;

    //    // 起点到终点的向量
    //    var pathVec = group.Target - group.StartPos;

    //    // 引力状态
    //    var isBackPath = false;

    //    // 求出当前位置与目标位置所构成向量与X轴正方向的角度
    //    var angleForVector = Math.Atan2(group.StartPos.x - group.Target.x, group.StartPos.z - group.Target.z) * 180 / Math.PI;
    //    // 路径点四角偏移量
    //    var offsetVector = Quaternion.Euler(new Vector3(0, (float)angleForVector, 0)) * new Vector3(1, 0, 0) * group.PathWeight;

    //    // 求出路径对角位置
    //    Vector3 pathCorner5 = group.Target.magnitude > group.StartPos.magnitude ? group.Target - offsetVector : group.StartPos - offsetVector;
    //    Vector3 pathCorner6 = group.Target.magnitude > group.StartPos.magnitude ? group.StartPos + offsetVector : group.Target + offsetVector;

    //    // 绘制路径宽度
    //    Vector3 pathCorner1 = group.Target + offsetVector;
    //    Vector3 pathCorner2 = group.Target - offsetVector;
    //    Vector3 pathCorner3 = group.StartPos + offsetVector;
    //    Vector3 pathCorner4 = group.StartPos - offsetVector;
    //    Debug.DrawLine(pathCorner1, pathCorner2);
    //    Debug.DrawLine(pathCorner2, pathCorner4);
    //    Debug.DrawLine(pathCorner4, pathCorner3);
    //    Debug.DrawLine(pathCorner3, pathCorner1);
    //    // 对角线
    //    Debug.DrawLine(pathCorner5, pathCorner6);
    //    //Debug.Log(string.Format("{0},{1},{2},{3},{4}", 
    //    //    pathCorner1, 
    //    //    pathCorner2, 
    //    //    group.GroupPos.x - group.Target.x, 
    //    //    group.GroupPos.z - group.Target.z, 
    //    //    Math.Atan2(group.GroupPos.x - group.Target.x, group.GroupPos.y - group.Target.y)));

    //    // 计算方向与速度
    //    for (j = 0; j < group.MemberList.Count; j++)
    //    {
    //        // 重置引力状态
    //        isBackPath = false;
    //        member = group.MemberList[j];
    //        speed = member.Speed;

    //        // 求平均位置
    //        averagePos += member.Position;
    //        // 如果该单位完成移动则跳过
    //        //if (member.State == SchoolItemState.Complete)
    //        //{
    //        //    continue;
    //        //}

    //        // 判断状态 调用开始移动 变更状态
    //        if (member.State == SchoolItemState.Unstart)
    //        {
    //            member.State = SchoolItemState.Moving;
    //            if (member.Moveing != null)
    //            {
    //                member.Moveing(member.ItemObj);
    //            }
    //        }

    //        // 计算与目标的相对向量
    //        dir = group.Target - member.Position;
    //        // 经过计算后的最终方向
    //        finalDir = dir.normalized;

    //        // 遍历同队队友
    //        for (k = 0; k < group.MemberList.Count; k++)
    //        {
    //            var otherMember = group.MemberList[k];
    //            // 排除自己的情况
    //            if (otherMember.Equals(member))
    //            {
    //                continue;
    //            }

    //            // 计算单位与队友的位置差向量
    //            offset = otherMember.Position - member.Position;
    //            // 计算最终方向与队友方向夹角
    //            sign = Vector3.Dot(dir.normalized, offset.normalized);
    //            // 判断与队友的相对位置是否在该单位前方
    //            if (sign > forwardAngle)
    //            {
    //                if (offset.magnitude > member.Distance && offset.magnitude < member.MaxDistance)
    //                {
    //                    // 在跟随区间
    //                    // 前方90度内有人, 并且距离小于改单位设置的最小间距
    //                    // 向前方队友位置偏移
    //                    finalDir += offset.normalized * member.RotateWeight * (1 - offset.magnitude / member.Distance);
    //                }
    //                else if (offset.magnitude <= member.Distance)
    //                {
    //                    // 小于追随距离, 该方向减速
    //                    speed *= offset.magnitude / member.Distance;
    //                }
    //            }

    //            // 判断侧向队友, 如果侧向队友靠太近则减少该方向移动速度, 如果太远则向该队友偏移
    //            if (sign < forwardAngle && sign > -forwardAngle)
    //            {
    //                if (offset.magnitude > member.Distance && offset.magnitude < member.MaxDistance)
    //                {
    //                    finalDir += offset.normalized * member.RotateWeight * (1 - offset.magnitude / member.Distance);
    //                }
    //                else if (offset.magnitude <= member.Distance)
    //                {
    //                    // 小于追随距离, 该方向减速
    //                    //speed *= offset.magnitude / member.Distance;
    //                    finalDir -= offset.normalized * member.RotateWeight;
    //                }
    //            }

    //        }

    //        // 判断当前位置与路径边界 超过边界给予向路径区域内的引力
    //        if (!Utils.IsCoverage(pathCorner5.x, pathCorner5.z, pathCorner6.x, pathCorner6.z, member.Position.x,
    //            member.Position.z))
    //        {
    //            // 增加引力
    //            // 求位置点位置到路径向量的垂直向量
    //            var itemVec = member.Position - group.StartPos;
    //            // 获得起点到该单元位置与起点到终点向量的夹角
    //            var cosTheta = Vector3.Dot(itemVec.normalized, pathVec.normalized);
    //            // 用起点到单元的向量长度乘以cos值获得向量长度
    //            var vecLenForTmp = itemVec.magnitude*cosTheta;
    //            // 中间向量长度乘以起点到终点向量的标准向量
    //            var midVec = pathVec.normalized*vecLenForTmp;
    //            // 获得引力向量
    //            var backPowerVec = midVec - itemVec;
    //            // 最终向量+该垂直向量获得引力
    //            //Debug.Log(string.Format("{0},{1},{2},{3},{4},{5}", itemVec,
    //            //    cosTheta,
    //            //    vecLenForTmp,
    //            //    midVec,
    //            //    backPowerVec,
    //            //    finalDir));
    //            // Debug.Log(backPowerVec);
    //            finalDir += backPowerVec;
    //            isBackPath = true;
    //        }

    //        // 如果目标没有在前方角度以内, 则停止前进, 进行转向
    //        angleForTarget = Vector3.Dot(dir.normalized, member.Direction);


    //        // TODO 增加与引力无关的强制无法前进方向停止前进
    //        // TODO 该计算为所有单位, 包括其他队伍单位
    //        // TODO 这个实现破坏结构需要重构, 结构设计问题
    //        // 减去该方向的力
    //        // 前方有人完全无法走动情况减速
    //        var closeMemberList = argquadTree.Retrieve(member);

    //        foreach (var closeMember in closeMemberList)
    //        {
    //        //    // 判断是否有碰撞
    //            if (member.GetGraphical().IsCollision(closeMember.GetGraphical()))
    //            {
    //        //        // 有碰撞则获取两单位的连接向量, 并从原方向中去掉该方向
    //        //        var vec = closeMember.Position - member.Position;
    //        //        // 获得向量减去, 向量的量是多少?
    //        //        var per = Vector3.Dot(vec.normalized, finalDir.normalized);
    //        //        finalDir -= vec.normalized * per;
    //        //        // 求两向量夹角, 并sin该角度获得比例, 乘以原向量长度为最终长度

    //        //        // 用求出来的比例乘以速度, 降低速度
    //        //        speed = speed*(1 - per);
    //            }
    //        }

    //        // 直线运动防止抖动
    //        if (angleForTarget < 0.999f)
    //        {
    //            rotateDir = Vector3.Dot(finalDir, member.DirectionRight);
    //            rotateDir = rotateDir * 180;
    //            // 规范化值 超过180或-180则将值回归180与-180以内
    //            if (rotateDir > 180 || rotateDir < -180)
    //            {
    //                rotateDir += ((int) rotateDir/180)*180*(Mathf.Sign(rotateDir));
    //            }
    //            // Debug.Log(rotateDir);
    //        }

    //        // 如果在路径内行进
    //        if (!isBackPath)
    //        {
    //            // 目标没有在前方角度以内, 停止移动只旋转
    //            if (angleForTarget < forwardAngle)
    //            {
    //                speed = 0;
    //                // 开始等待
    //                if (member.State != SchoolItemState.Waiting && member.State != SchoolItemState.Complete)
    //                {
    //                    member.State = SchoolItemState.Waiting;
    //                    if (member.Wait != null)
    //                    {
    //                        member.Wait(member.ItemObj);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                // 根据角度获得差速, 直线移动最快
    //                speed = speed * angleForTarget;
    //                if (member.State != SchoolItemState.Moving && member.State != SchoolItemState.Complete)
    //                {
    //                    // 结束等待, 开始移动
    //                    member.State = SchoolItemState.Moving;
    //                    if (member.Moveing != null)
    //                    {
    //                        member.Moveing(member.ItemObj);
    //                    }
    //                }
    //            }
    //        }


    //        // 求出方向, 想这个方向旋转, 然后向该单位的正前方移动
    //        // TODO 应使用对象相对up 旋转
    //        member.Rotate = Vector3.up * rotateDir * member.RotateSpeed * Time.deltaTime;
    //        // 向前方移动, 转向单独处理
    //        member.Position += member.Direction * speed * Time.deltaTime;

    //        // 判断单位是否到达.
    //        if ((member.Position - group.Target).magnitude < member.Distance)
    //        {
    //            completeCount++;
    //            if (member.State != SchoolItemState.Complete)
    //            {
    //                // 单位状态修改为complete
    //                member.State = SchoolItemState.Complete;
    //                // 调用到达
    //                if (member.Complete != null) { member.Complete(member.ItemObj); }
    //            }
    //        }

    //        // 判断组队是否到达
    //        if (!groupComlete && completeCount * 100 / group.MemberList.Count > group.ProportionOfComplete)
    //        {
    //            if (group.Complete != null)
    //            {
    //                // TODO 会调用多次
    //                group.Complete(group);
    //            }
    //            groupComlete = true;
    //        }
    //    }
    //    // 组队平均位置
    //    averagePos = new Vector3(averagePos.x / group.MemberList.Count, averagePos.y / group.MemberList.Count, averagePos.z / group.MemberList.Count);
    //    group.GroupPos = averagePos;
    //}

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