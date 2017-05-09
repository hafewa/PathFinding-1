﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// 测试碰撞功能
/// </summary>
public class TestCollision : MonoBehaviour
{
    /// <summary>
    /// 创建随机形状单位个数
    /// </summary>
    public int RandomItemCount = 10;

    /// <summary>
    /// 宽度
    /// </summary>
    public int Width = 500;

    /// <summary>
    /// 高度
    /// </summary>
    public int Height = 500;


    /// <summary>
    /// 平滑角度
    /// </summary>
    public float UnitTheta = 0.1f;

    /// <summary>
    /// 随机单位
    /// </summary>
    private System.Random random = new System.Random((int)DateTime.Now.Ticks);

    /// <summary>
    /// 移动单位列表
    /// </summary>
    public IList<ICollisionGraphics> CollisionList = new List<ICollisionGraphics>();


    public IList<bool> IsCollisionList = new List<bool>();


    // public float Angle = 10;


	void Start ()
	{
        // 初始化
	    Init();

	}
	

	void Update ()
	{
        //Utils.DrawRect(new Vector3(10,0,10), 10, 20, Angle, Color.white);
        // 单位移动
        ItemMove();

        // 绘制单位
        DrawItem();

        // 检测碰撞
        CheckCollision();

        // 控制
        Control();

    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        CollisionList.Clear();
        // 创建单位
        for (var i = 0; i < RandomItemCount; i++)
        {
            // 随机单位
            var itemTypeNum = random.Next(1, 4);

            switch (itemTypeNum)
            {
                case 1:
                {
                    // 随机位置
                    var position = new Vector2(random.Next(0, Width), random.Next(0, Height));
                    // 矩形
                    CollisionList.Add(new RectGraphics(new Vector2(position.x, position.y), random.Next(1, 20), random.Next(2, 30),
                        random.Next(1, 360)));
                        IsCollisionList.Add(false);
                }
                    break;
                case 2:
                {
                    // 圆形
                    // 随机位置
                    var position = new Vector2(random.Next(0, Width), random.Next(0, Height));

                    CollisionList.Add(new CircleGraphics(new Vector2(position.x, position.y), random.Next(1, 20)));
                        IsCollisionList.Add(false);
                    }
                    break;
                case 3:
                {
                    // 扇形
                    // 随机位置
                    var position = new Vector2(random.Next(0, Width), random.Next(0, Height));

                    CollisionList.Add(new SectorGraphics(new Vector2(position.x, position.y), random.Next(1, 360), random.Next(1, 20), random.Next(1, 180)));
                        IsCollisionList.Add(false);
                    }
                    break;
            }


            // 随机大小

            // 随机转向

        }
    }

    /// <summary>
    /// 单位移动
    /// </summary>
    private void ItemMove()
    {
        foreach (var item in CollisionList)
        {
            // 旋转
            switch (item.GraphicType)
            {
                case GraphicType.Rect:
                    var rectItem = item as RectGraphics;
                    if (rectItem != null)
                    {
                        rectItem.Rotation += 30*Time.deltaTime;
                    }

                    break;
                case GraphicType.Sector:
                    var sectorItem = item as SectorGraphics;
                    if (sectorItem != null)
                    {
                        sectorItem.Rotation += 1 * Time.deltaTime;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 控制
    /// </summary>
    private void Control()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Init();
        }
    }

    /// <summary>
    /// 检查单位之间的碰撞
    /// </summary>
    private void CheckCollision()
    {
        for (var i = 0; i < CollisionList.Count; i++)
        {
            var item = CollisionList[i];
            for (var j = i + 1; j < CollisionList.Count; j++)
            {
                if (item.CheckCollision(CollisionList[j]))
                {
                    IsCollisionList[i] = true;
                    IsCollisionList[j] = true;
                }
                else
                {
                    IsCollisionList[j] = false;
                }
            }
        }
    }

    /// <summary>
    /// 绘制图形
    /// </summary>
    private void DrawItem()
    {
        // 绘制边框
        Utils.DrawRect(new Vector3(0, 0, 0), Width, Height, 0, Color.black);
        for (var i = 0; i < CollisionList.Count; i++)
        {
            var item = CollisionList[i];
            var isCollision = IsCollisionList[i];
            switch (item.GraphicType)
            {
                case GraphicType.Circle:
                    var circleItem = item as CircleGraphics;
                    if (circleItem != null)
                    {
                        Utils.DrawCircle(new Vector3(circleItem.Postion.x, 0, circleItem.Postion.y), circleItem.Radius, isCollision ? Color.red : Color.white);
                    }
                    break;
                case GraphicType.Rect:
                    var rectItem = item as RectGraphics;
                    if (rectItem != null)
                    {
                        Utils.DrawRect(new Vector3(rectItem.Postion.x, 0, rectItem.Postion.y), rectItem.Width, rectItem.Height, rectItem.Rotation, isCollision ? Color.red : Color.white);
                    }
                    break;
                case GraphicType.Sector:
                    var sectorItem = item as SectorGraphics;
                    if (sectorItem != null)
                    {
                        var halfAngle = sectorItem.OpenAngle*0.5f;
                        var point1 = sectorItem.Postion;
                        var point2 = new Vector2((float)Math.Sin(sectorItem.Rotation + halfAngle),
                (float)Math.Cos(sectorItem.Rotation + halfAngle));
                        var point3 = new Vector2((float)Math.Sin(sectorItem.Rotation - halfAngle),
                            (float)Math.Cos(sectorItem.Rotation - halfAngle));
                        Utils.DrawTriangle(point1, point2, point3, isCollision ? Color.red : Color.white);
                    }
                    break;
            }
        }
    }


    

}
