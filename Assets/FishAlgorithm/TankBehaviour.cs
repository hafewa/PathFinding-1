//write by shadow
using UnityEngine;
using System.Collections;

public class TankBehaviour : MonoBehaviour
{
    private const float minMoveCheck = 0.2f;

    public int groupId = 0;//组 id
    public float moveSpeed = 5, rotateSpeed = 20;//移动旋转速度

    public Vector3 position
    {
        get { return transform.position; }
    }

    public Vector3 movement
    {
        get { return myMovement; }
    }

    private Vector3 myMovement = Vector3.zero;
    private TankGroup myGroup;
    private float tgtSpeed = 0, speed = 0, currentSpeed;

    public void SetGroup(int index)
    {
        myGroup = TankGroup.GetTankGroup(index);
    }

    // Use this for initialization
    void Start()
    {
        SetGroup(groupId);
    }

    // Update is called once per frame
    void Update()
    {
        //获取与目标的差值向量
        Vector3 displacement = myGroup.targetPosition - position;
        //方向*权重
        Vector3 direction = displacement.normalized * myGroup.targetWeight;

        //如果到达目标点 重新计算目的地距离权重
        if (displacement.magnitude < myGroup.targetCloseDistance)
            direction = Vector3.zero;

        //获取周围组的移动
        direction += GetGroupMovement();

        //计算移动速度
        if ((myGroup.targetPosition - position).magnitude < minMoveCheck)
            tgtSpeed = 0;
        else
            tgtSpeed = moveSpeed;

        speed = Mathf.Lerp(speed, tgtSpeed, 2 * Time.deltaTime);

        //移动
        Drive(direction, speed);
    }

    /// <summary>
    /// 组队移动
    /// </summary>
    /// <returns>当前移动方向 + 保持间距的斥力/引力</returns>
    private Vector3 GetGroupMovement()
    {
        // TODO 使用碰撞体获取周围的队友
        Collider[] c = Physics.OverlapSphere(position, myGroup.keepDistance, myGroup.mask);

        // 定义距离, 
        Vector3 dis, v1 = Vector3.zero, v2 = Vector3.zero;

        // 遍历周围队友
        for (int i = 0; i < c.Length; i++)
        {
            TankBehaviour otherTank = c[i].GetComponent<TankBehaviour>();
            // 与该队友距离
            dis = position - otherTank.position;

            // TODO 速度1 保持距离用
            v1 += dis.normalized * (1 - dis.magnitude / myGroup.keepDistance);//查看与周围单位的距离
            // TODO 速度2 加上当前移动方向
            v2 += otherTank.movement;//查看周围单位移动方向

            Debug.DrawLine(position, otherTank.position, Color.yellow);
        }

        // 返回 当前移动方向 + 保持间距的斥力/引力
        return v1.normalized * myGroup.keepWeight + v2.normalized * myGroup.moveWeight;//添加权重因素
    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="direction">移动方向</param>
    /// <param name="spd">移动速度</param>
    private void Drive(Vector3 direction, float spd)
    {
        // 移动方向
        Vector3 finialDirection = direction.normalized;
        // 移动速度与Y轴旋转
        float finialSpeed = spd, finialRotate = 0;

        // 旋转方向
        float rotateDir = Vector3.Dot(finialDirection, transform.right);
        // 前进方向
        float forwardDir = Vector3.Dot(finialDirection, transform.forward);

        // 如果移动方向没有在前方180度范围内, 判断左转还是右转(Cos a) 
        if (forwardDir < 0)
            rotateDir = Mathf.Sign(rotateDir);

        // 如果移动方向没有在前方200度左右范围内, 降低速度
        if (forwardDir < -0.2f)
            finialSpeed = Mathf.Lerp(currentSpeed, -spd * 8, 4 * Time.deltaTime);

        // 保持转向速度在一个区间内
        if (forwardDir < 0.98f)//防抖
            finialRotate = Mathf.Clamp(rotateDir * 180, -rotateSpeed, rotateSpeed);

        // 速度控制并限制速度
        finialSpeed *= Mathf.Clamp01(direction.magnitude);
        finialSpeed *= Mathf.Clamp01(1 - Mathf.Abs(rotateDir) * 0.8f);

        // 前进
        transform.Translate(Vector3.forward * finialSpeed * Time.deltaTime);
        // 旋转
        transform.Rotate(Vector3.up * finialRotate * Time.deltaTime);

        // 记录当前速度
        currentSpeed = finialSpeed;
        // 当前移动
        myMovement = direction * finialSpeed;
    }

}