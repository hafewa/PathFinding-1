﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 圆形图形
/// </summary>
public class CircleGraphics : CollisionGraphics
{

    /// <summary>
    /// 半径
    /// </summary>
    public float Radius { get; set; }



    /// <summary> 
    /// 初始化
    /// </summary>
    /// <param name="position">圆心位置</param>
    /// <param name="radius">圆半径</param>
    public CircleGraphics(Vector2 position, float radius)
    {
        Postion = position;
        Radius = radius;
        graphicType = GraphicType.Circle;
    }

}
