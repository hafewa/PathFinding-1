using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 集群管理
/// 集群行为都在这里集中实现
/// </summary>
public class SchoolManager : MonoBehaviour
{

    /// <summary>
    /// 组列表(全局)
    /// </summary>
    public static List<SchoolGroup> GroupList = new List<SchoolGroup>();

    public void Start()
    {
        // 初始化
    }
    public void Update()
    {
        // 挨个更新对象位置
        for (var i = 0; i < GroupList.Count; i++)
        {
            var group = GroupList[i];
            Move(group);
        }
    }

    /// <summary>
    /// 组队移动
    /// </summary>
    /// <param name="group">组队对象</param>
    private void Move(SchoolGroup group)
    {
        // 计算方向与速度
        for (var j = 0; j < group.MemberList.Count; j++)
        {
            var member = group.MemberList[j];
            var dir = group.Target - member.Position;
            // 遍历同队队友
            for (var k = 0; k < group.MemberList.Count; k++)
            {
                var otherMember = group.MemberList[k];
                if (otherMember.Equals(member))
                {
                    continue;
                }
                // 如果有队友在该队员前方则进行跟随并减速

                // 寻找前方队友
                var offset = otherMember.Position - member.Position;
                var sign = Vector3.Dot(dir.normalized, offset.normalized);
                if (sign > 0)
                {
                    // 前方有人
                }
                else
                {
                    // 否则全速前进不等队友
                }

            }
            // 朝目标组队移动

            // 计算与目标的相对向量

            // 判断与队友的相对位置

        }
    }

    /// <summary>
    /// 清除所有组
    /// </summary>
    public static void ClearAllGroup()
    {
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