﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 碰撞检测器
/// </summary>
public class CollisionDetection : IFormulaItem
{
    // 使用目标选择器选择范围内适合的单位

    /// <summary>
    /// 行为类型
    /// 0: 不等待其执行结束继续
    /// 1: 等待期执行结束调用callback
    /// </summary>
    public int FormulaType { get; private set; }

    /// <summary>
    /// 选取目标数量上限
    /// </summary>
    public int TargetCount { get; private set; }

    /// <summary>
    /// 目标阵营
    /// </summary>
    public TargetCampsType TargetCamps { get; private set; }

    /// <summary>
    /// 检测范围形状
    /// </summary>
    public GraphicType ScopeType { get; private set; }

    /// <summary>
    /// 范围描述参数
    /// </summary>
    public float[] ScopeParams { get; private set; }

    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillNum { get; private set; }

    /// <summary>
    /// 初始化碰撞检测
    /// </summary>
    /// <param name="formulaType">行为单元类型(0: 不等待, 1: 等待)</param>
    /// <param name="targetCount">目标数量</param>
    /// <param name="targetCamps">目标阵营</param>
    /// <param name="scopeType">范围类型</param>
    /// <param name="scopeParams">范围参数</param>
    /// <param name="skillNum">释放技能ID</param>
    public CollisionDetection(int formulaType, int targetCount, TargetCampsType targetCamps, GraphicType scopeType, float[] scopeParams, int skillNum)
    {
        this.FormulaType = formulaType;
        this.TargetCount = targetCount;
        this.TargetCamps = targetCamps;
        this.ScopeType = scopeType;
        this.ScopeParams = scopeParams;
        this.SkillNum = skillNum;
    }

    /// <summary>
    /// 生成行为单元
    /// </summary>
    /// <returns>行为单元对象</returns>
    public IFormula GetFormula(FormulaParamsPacker paramsPacker)
    {
        IFormula result = null;


        // TODO 技能数据应该在paramsPacker中

        // 检测范围
        // 获取图形对象

        // 搜索位置,形状(包含大小),目标数量,释放技能ID
        // 检查范围内对象
        // 获取目标单位个数个单位
        // 对他们释放技能(技能编号)
        // 


        return result;
    }


    /// <summary>
    /// 判断是否碰撞
    /// </summary>
    /// <param name="item">被搜索单位</param>
    /// <param name="scanerX">搜索单位位置X</param>
    /// <param name="scanerY">搜索单位位置Y</param>
    /// <param name="radius">搜索半径</param>
    /// <param name="openAngle">扇形开合角度</param>
    /// <param name="rotate">扇形转动角度</param>
    /// <returns>是否碰撞</returns>
    public static bool IsCollisionItem(IBaseMamber item, float scanerX, float scanerY, float radius, float openAngle = 361f, float rotate = 0f)
    {
        if (item == null)
        {
            return false;
        }
        // 求距离
        var xOff = item.X - scanerX;
        var yOff = item.Y - scanerY;
        var distanceSquare = xOff*xOff + yOff*yOff;

        var radiusSum = item.Diameter*0.5f + radius;
        
        // 距离超过半径和不会相交
        if (distanceSquare > radiusSum * radiusSum)
        {
            return false;
        }

        if (openAngle >= 360f)
        {
            // 判断圆形
            return true;
        }
        else
        {
            // 判断扇形
            var halfOpenAngle = openAngle*0.5f;
            // 两个点相对圆心方向
            var pointForCorner1 = new Vector2((float)Math.Sin(rotate + halfOpenAngle),
                (float)Math.Cos(rotate + halfOpenAngle));
            var pointForCorner2 = new Vector2((float)Math.Sin(rotate - halfOpenAngle),
                (float)Math.Cos(rotate - halfOpenAngle));
            var circlePosition = new Vector2(item.X, item.Y);
            var sectorPosition = new Vector2(scanerX, scanerY);

            // 判断圆心到扇形两条边的距离
            var distance1 = CollisionGraphics.EvaluatePointToLine(circlePosition, pointForCorner1, sectorPosition);
            var distance2 = CollisionGraphics.EvaluatePointToLine(circlePosition, sectorPosition, pointForCorner2);
            if (distance1 >= 0 && distance2 >= 0)
            {
                // 圆心在扇形开口角度内
                return true;
            }
            // 如果与两线相交则相交
            if (CollisionGraphics.CheckCircleAndLine(circle, sectorGraphics.Postion, pointForCorner1) ||
                CollisionGraphics.CheckCircleAndLine(circle, sectorGraphics.Postion, pointForCorner2))
            {
                return true;
            }
        }


        return false;
    }




}

/// <summary>
/// 目标阵营类型
/// </summary>
public enum TargetCampsType
{
    All = -1,
    Same = 0,
    Different = 1
}
