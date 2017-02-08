using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 集群行为
/// </summary>
public class SchoolBehaviour : MonoBehaviour
{

    
    /// <summary>
    /// 组队ID, 只读
    /// </summary>
    public int GroupId
    {
        get { return groupId; }
        set
        {
            // 判断是否已有group, 如果有并且ID不同, 则删除原有group中的member
            if (value != groupId)
            {
                // group列表中是否有改ID的group, 如果有则插入其中, 如果没有则创建
                group.MemberList.Remove(this);
                var newGroup = SchoolManager.GetGroupById(groupId);
                if (newGroup == null)
                {
                    newGroup = new SchoolGroup(groupId);
                    SchoolManager.GroupList.Add(newGroup);
                    group = newGroup;
                }
                groupId = value;
            }
        }
    }

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
    public Vector3 Position
    {
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
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
    /// 移动结束后调用
    /// </summary>
    public Action<GameObject> Complete { get; set; }

    /// <summary>
    /// 组编号
    /// </summary>
    private int groupId;

    /// <summary>
    /// 组对象
    /// </summary>
    private SchoolGroup group;


    public SchoolBehaviour(int groupId)
    {
        this.GroupId = groupId;
    }

    public void Start()
    {
        // 将自己存入队员列表
        Group.MemberList.Add(this);
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
    /// group所属阵营
    /// </summary>
    public int Camp { get; set; }

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 Target { get; set; }

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

}