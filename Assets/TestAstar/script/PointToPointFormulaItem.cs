﻿using System;


/// <summary>
/// 点对点飞行特效行为构建器
/// </summary>
public class PointToPointFormulaItem : IFormulaItem
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
    /// 飞行速度
    /// </summary>
    public float Speed { get; private set; }

    /// <summary>
    /// 飞行轨迹
    /// </summary>
    public TrajectoryAlgorithmType FlyType { get; private set; }

    /// <summary>
    /// 释放特效位置
    /// 0: 释放特效单位(默认)
    /// 1: 接受单位位置
    /// </summary>
    private int releasePos = 0;

    /// <summary>
    /// 接受特效位置
    /// 0: 释放特效单位
    /// 1: 接受单位位置(默认)
    /// </summary>
    private int receivePos = 1;


    public PointToPointFormulaItem(int formulaType, string effectKey, float speed, int releasePos, int receivePos, TrajectoryAlgorithmType flyType)
    {
        FormulaType = formulaType;
        EffectKey = effectKey;
        Speed = speed;
        this.releasePos = releasePos;
        this.receivePos = receivePos;
        FlyType = flyType;
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
        else if (Speed <= 0)
        {
            errorMsg = "物体飞行速度不合法, <=0";
        }

        if (!string.IsNullOrEmpty(errorMsg))
        {
            throw new Exception(errorMsg);
        }

        IFormula result = new Formula((callback) =>
        {
            // 判断发射与接收位置
            var releasePosition = releasePos == 0 ? paramsPacker.StartPos : paramsPacker.TargetPos;
            var receivePosition = receivePos == 0 ? paramsPacker.StartPos : paramsPacker.TargetPos;
            EffectsFactory.Single.CreatePointToPointEffect(EffectKey, paramsPacker.ItemParent, releasePosition,
                                receivePosition, paramsPacker.Scale, Speed, FlyType, callback).Begin();
        }, FormulaType);

        return result;
    }
}
