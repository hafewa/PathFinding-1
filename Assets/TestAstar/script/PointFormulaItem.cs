﻿using System;


/// <summary>
/// 点特效
/// </summary>
public class PointFormulaItem : IFormulaItem
{
    /// <summary>
    /// 行为类型
    /// 0: 不等待其执行结束继续
    /// 1: 等待期执行结束调用callback
    /// </summary>
    public int FormulaType { get; private set; }

    /// <summary>
    /// 特效key(或者路径)
    /// </summary>
    public string EffectKey { get; private set; }

    /// <summary>
    /// 特效出现位置
    /// 0: 释放者位置
    /// 1: 被释放者位置
    /// </summary>
    public int TargetPos { get; private set; }

    /// <summary>
    /// 飞行速度
    /// </summary>
    public float Speed { get; private set; }

    /// <summary>
    /// 持续时间
    /// </summary>
    public float DurTime { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="formulaType">是否等待执行完毕 0 否, 1 是</param>
    /// <param name="effectKey">特效key(或路径)</param>
    /// <param name="targetPos">出现位置</param>
    /// <param name="speed">播放速度</param>
    /// <param name="durTime">持续时间</param>
    public PointFormulaItem(int formulaType, string effectKey, int targetPos, float speed, float durTime)
    {
        FormulaType = formulaType;
        EffectKey = effectKey;
        TargetPos = targetPos;
        Speed = speed;
        DurTime = durTime;
    }


    /// <summary>
    /// 获取行为构建器
    /// </summary>
    /// <returns>构建完成的单个行为</returns>
    public IFormula GetFormula(FormulaParamsPacker paramsPacker)
    {
        // 验证数据正确, 如果有问题直接抛错误
        string errorMsg = null;
        if (paramsPacker == null)
        {
            errorMsg = "调用参数 paramsPacker 为空.";
        }
        else if (EffectKey == null)
        {
            errorMsg = "特效Key(或路径)为空.";
        }

        if (!string.IsNullOrEmpty(errorMsg))
        {
            throw new Exception(errorMsg);
        }

        var tmpTargetPos = TargetPos;

        IFormula result = new Formula((callback) =>
        {
            var pos = tmpTargetPos == 0 ? paramsPacker.StartPos : paramsPacker.TargetPos;
            // 判断发射与接收位置
            EffectsFactory.Single.CreatePointEffect(EffectKey, paramsPacker.ItemParent, pos, paramsPacker.Scale, DurTime, Speed, callback).Begin();
        }, FormulaType);

        return result;
    }
}