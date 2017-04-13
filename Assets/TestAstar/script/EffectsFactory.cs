using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// ----------------------------------实体对象-----------------------------------------
/// <summary>
/// 特效工厂
/// </summary>
public class EffectsFactory
{
    /// <summary>
    /// 单例
    /// </summary>
    public static EffectsFactory Single
    {
        get
        {
            if (single == null)
            {
                single = new EffectsFactory();
            }
            return single;
        }
    }

    /// <summary>
    /// 单例对象
    /// </summary>
    private static EffectsFactory single = null;


    /// <summary>
    /// 创建点特效
    /// 特效只在一个位置上
    /// </summary>
    /// <returns>特效对象</returns>
    public EffectBehaviorAbstract CreatePointEffect()
    {
        EffectBehaviorAbstract result = null;



        return result;
    }

    /// <summary>
    /// 创建点对点特效
    /// 特效会从start按照速度与轨迹飞到end点
    /// </summary>
    /// <returns>特效对象</returns>
    public EffectBehaviorAbstract CreatePointToPointEffect()
    {
        EffectBehaviorAbstract result = null;



        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public EffectBehaviorAbstract CreateScopeEffect()
    {
        EffectBehaviorAbstract result = null;



        return result;
    }
}


/// <summary>
/// 点特效
/// </summary>
public class PointEffect : EffectBehaviorAbstract
{
    /// <summary>
    /// 特效出现位置
    /// </summary>
    private Vector3 position;

    /// <summary>
    /// 特效对象缩放
    /// </summary>
    private Vector3 scale;

    /// <summary>
    /// 特效扩散速度
    /// </summary>
    private float speed;

    /// <summary>
    /// 创建点特效
    /// </summary>
    /// <param name="pos">特效出现位置</param>
    /// <param name="scale">特效缩放</param>
    /// <param name="speed">特效播放速度</param>
    public PointEffect(Vector3 pos, Vector3 scale, float speed)
    {
        this.position = pos;
        this.scale = scale;
        this.speed = speed;
    }
    /// <summary>
    /// 开始移动
    /// </summary>
    public override void Begin()
    {
        // TODO 加载特效预设

        // 设置数据
    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public override void Stop()
    {
        
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Destroy()
    {
        
    }
}

/// <summary>
/// 点对点特效
/// </summary>
public class PointToPointEffect : EffectBehaviorAbstract
{

    /// <summary>
    /// 开始移动
    /// </summary>
    public override void Begin()
    {

    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public override void Stop()
    {

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Destroy()
    {

    }
}


/// <summary>
/// 范围特效
/// </summary>
public class ScopeEffect : EffectBehaviorAbstract
{

    /// <summary>
    /// 开始移动
    /// </summary>
    public override void Begin()
    {

    }

    /// <summary>
    /// 停止移动
    /// </summary>
    public override void Stop()
    {

    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Destroy()
    {

    }
}


/// <summary>
/// 特效运动Looper
/// </summary>
public class EffectLooper : ILoopItem
{

    /// <summary>
    /// 单次循环
    /// </summary>
    public void Do()
    {

    }

    /// <summary>
    /// 是否执行完毕
    /// </summary>
    /// <returns></returns>
    public bool IsEnd()
    {
        return false;
    }

    /// <summary>
    /// 被销毁时执行
    /// </summary>
    public void OnDestroy()
    {

    }
}


// ----------------------------------抽象对象-----------------------------------------

/// <summary>
/// 特效对象
/// </summary>
public abstract class EffectBehaviorAbstract : MonoBehaviour, IEffectsBehavior
{
    /// <summary>
    /// 开始移动
    /// </summary>
    public abstract void Begin();

    /// <summary>
    /// 停止移动
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// 销毁
    /// </summary>
    public abstract void Destroy();
    
}

/// <summary>
/// 特效行为接口
/// </summary>
public interface IEffectsBehavior
{
    /// <summary>
    /// 开始
    /// </summary>
    void Begin();
}