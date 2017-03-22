﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 位置对象
/// </summary>
public abstract class PositionObject : MonoBehaviour, IGraphical<Rectangle>
{
    /// <summary>
    /// 单位ID
    /// </summary>
    public long Id {
        get
        {
            if (id == -1)
            {
                id = ++staticId;
            }
            return id;
        }
    }


    /// <summary>
    /// 用于标记对象
    /// </summary>
    private static long staticId;

    /// <summary>
    /// 当前单位ID
    /// </summary>
    private long id = -1;

    /// <summary>
    /// 物理信息
    /// </summary>
    public virtual PhysicsInfo PhysicsInfo
    {
        get
        {
            if (physicsInfo == null)
            {
                physicsInfo = new PhysicsInfo();
            }
            if (physicsInfo.Quality < Utils.ApproachZero)
            {
                physicsInfo.Quality = Diameter * Diameter;
            }
            return physicsInfo;
        }
    }


    /// <summary>
    /// 单元直径
    /// </summary>
    public float Diameter
    {
        get { return diameter; }
        set
        {
            diameter = value < 0 ? 1 : value;
            // 质量 = 直径平方
            physicsInfo.Quality = diameter * diameter;
        }
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
    /// 设置旋转值
    /// </summary>
    public Vector3 Rotate
    {
        set { this.transform.Rotate(value); }
    }

    /// <summary>
    /// 返回当前单位的正前向量
    /// </summary>
    public Vector3 Direction
    {
        get { return this.transform.forward; }
    }

    public Vector3 DirectionRight
    {
        get { return this.transform.right; }
    }

    /// <summary>
    /// 当前对象的gameobject的引用
    /// </summary>
    public GameObject ItemObj
    {
        get { return this.gameObject; }
    }
    

    /// <summary>
    /// 物理信息
    /// </summary>
    protected PhysicsInfo physicsInfo = new PhysicsInfo();

    /// <summary>
    /// 单元直径
    /// </summary>
    private float diameter = 1;

    /// <summary>
    /// 历史x
    /// </summary>
    private float hisX;

    /// <summary>
    /// 历史y
    /// </summary>
    private float hisY;

    /// <summary>
    /// 历史直径
    /// </summary>
    private float hisDiameter;

    /// <summary>
    /// 历史rect
    /// </summary>
    private Rectangle hisRectangle;

    /// <summary>
    /// 返回位置图形
    /// </summary>
    /// <returns>方形图形</returns>
    public Rectangle GetGraphical()
    {
        //值有变更时重新创建Rect
        var halfDiameter = Diameter * 0.5f;
        var x = transform.localPosition.x - halfDiameter;
        var y = transform.localPosition.z - halfDiameter;
        if (hisDiameter - diameter > Utils.ApproachZero ||
            diameter - hisDiameter > Utils.ApproachZero ||
            hisX - x > Utils.ApproachZero ||
            x - hisX > Utils.ApproachZero ||
            hisY - y > Utils.ApproachZero ||
            y - hisY > Utils.ApproachZero)
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
    public float Momentum
    {
        get { return SpeedDirection.magnitude * Quality; }
        //set
        //{
        //    momentum = value;
        //}
    }

    /// <summary>
    /// 速度方向
    /// </summary>
    public Vector3 SpeedDirection
    {
        get { return speedDirection; }
        set { speedDirection = value; }
    }

    ///// <summary>
    ///// 动向方向
    ///// </summary>
    //public Vector3 Direction
    //{
    //    get { return direction; }
    //    set { direction = value.normalized; }
    //}

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
    //public float Speed
    //{
    //    get { return Momentum.magnitude / Quality; }
    //    //set {
    //    //    //speed = value > maxSpeed ? maxSpeed : value;
    //    //    speed = value;
    //    //}
    //}

    /// <summary>
    /// 最大速度
    /// </summary>
    public float MaxSpeed
    {
        get { return maxSpeed; }
        set { maxSpeed = value; }
    }

    ///// <summary>
    ///// 动量
    ///// </summary>
    //private Vector3 momentum;

    /// <summary>
    /// 速度方向
    /// </summary>
    private Vector3 speedDirection;

    ///// <summary>
    ///// 动量方向
    ///// </summary>
    //private Vector3 direction;

    /// <summary>
    /// 质量
    /// </summary>
    private float quality = 1;

    /// <summary>
    /// 速度
    /// </summary>
    //private float speed = 0;

    /// <summary>
    /// 最大速度
    /// </summary>
    private float maxSpeed = 10;
}