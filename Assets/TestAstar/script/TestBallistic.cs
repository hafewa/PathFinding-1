﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 弹道测试
/// </summary>
public class TestBallistic : MonoBehaviour {

    /// <summary>
    /// 开火发射点
    /// </summary>
    public Vector3 FirePoint = Vector3.zero;

    /// <summary>
    /// 开火方向
    /// </summary>
    public Vector3 FireDirection = Vector3.forward;
    
    /// <summary>
    /// 目标点
    /// </summary>
    // public Vector3 TargetPos = Vector3.zero;

    /// <summary>
    /// 炮弹开火力度, 默认10
    /// </summary>
    public float FirePower = 10;

    /// <summary>
    /// 是否使用重力
    /// </summary>
    public bool HasGravity = true;

    /// <summary>
    /// 重力系数 默认9.8
    /// </summary>
    public float Gravity = 9.8f;
    
    /// <summary>
    /// 炮弹
    /// </summary>
    public GameObject Ball;
    
    /// <summary>
    /// 主相机
    /// </summary>
    public Camera MainCamera;

    /// <summary>
    /// 地板对象
    /// </summary>
    public GameObject Plane;

    void Start()
    {
        if (MainCamera == null)
        {
            MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            Plane = GameObject.Find("Plane");
        }
    }


	void Update () {
        // 控制相机
        MoveCamera();

        // 控制开火
	    ControlFire();

        // 绘制发射方向
        Debug.DrawRay(FirePoint, FireDirection * 10);
	}

    public void ControlFire()
    {
        if (Input.GetMouseButton(0))
        {
            // 发射射线
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // 判断击中
            if (Physics.Raycast(ray, out hit))
            {
                // 如果击中了地板则获取该位置设为发射目标点
                if (hit.collider.name.Equals(Plane.name))
                {
                    // 设置目标点
                    //Debug.Log(hit.point);
                    // 创建炮弹
                    var ballisticItem = Ball != null
                        ? Instantiate(Ball)
                        : GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // 初始化位置
                    ballisticItem.transform.localPosition = FirePoint;
                    // 发射, 挂载弹道脚本
                    var ballistic = BallisticFactory.Single.CreateBallistic(ballisticItem, FirePoint, FireDirection,
                        hit.point,
                        FirePower, 3, HasGravity, Gravity, trajectoryType: TrajectoryAlgorithmType.Sine); //ballisticItem.AddComponent<Ballistic>();
                    //ballistic.Direction = FireDirection * FirePower;
                    //ballistic.BallisticArriveTarget = new BallisticArriveTargetForPosition(new Vector3(20, 0, 20));
                    ballistic.Complete = (a, b) =>
                    {
                        Destroy(a.gameObject);
                        //Debug.Log("end");
                    };
                    //ballistic.Speed = FirePower;

                }
            }
        }
    }



    /// <summary>
    /// 移动相机
    /// 上下左右控制相机x,z轴移动
    /// pageup pagedown 控制相机y轴移动
    /// 鼠标控制相机方向
    /// </summary>
    private void MoveCamera()
    {
        if (MainCamera == null)
        {
            Debug.Log("主相机为空.");
            return;
        }
        // 位置移动
        // 移动x,z轴
        if (Input.GetKey(KeyCode.UpArrow))
        {
            MainCamera.transform.localPosition += MainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            MainCamera.transform.localPosition -= MainCamera.transform.forward;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MainCamera.transform.localPosition += Quaternion.Euler(0, -90, 0) * new Vector3(MainCamera.transform.forward.x, 0, MainCamera.transform.forward.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MainCamera.transform.localPosition += Quaternion.Euler(0, 90, 0) * new Vector3(MainCamera.transform.forward.x, 0, MainCamera.transform.forward.z);
        }
        // 移动y轴
        if (Input.GetKey(KeyCode.PageUp))
        {
            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, MainCamera.transform.localPosition.y + 1, MainCamera.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            MainCamera.transform.localPosition = new Vector3(MainCamera.transform.localPosition.x, MainCamera.transform.localPosition.y - 1, MainCamera.transform.localPosition.z);
        }

        // 方向移动
        if (Input.GetMouseButton(1))
        {
            float rotateX = MainCamera.transform.localEulerAngles.x - Input.GetAxis("Mouse Y");
            float rotateY = MainCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X");
            MainCamera.transform.localEulerAngles = new Vector3(rotateX, rotateY, 0);
        }
    }
}
