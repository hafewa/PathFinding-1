﻿using System;
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
    /// <param name="effectKey"></param>
    /// <param name="parent"></param>
    /// <param name="position"></param>
    /// <param name="scale"></param>
    /// <param name="durTime"></param>
    /// <param name="speed"></param>
    /// <returns>特效对象</returns>
    public EffectBehaviorAbstract CreatePointEffect(string effectKey, Transform parent, Vector3 position, Vector3 scale, float durTime, float speed)
    {
        EffectBehaviorAbstract result = null;

        result = new PointEffect(effectKey, parent, position, scale, durTime, speed);

        return result;
    }

    /// <summary>
    /// 创建点对点特效
    /// 特效会从start按照速度与轨迹飞到end点
    /// </summary>
    /// <returns>特效对象</returns>
    public EffectBehaviorAbstract CreatePointToPointEffect(string effectKey, Transform parent, Vector3 position, Vector3 to, Vector3 scale, float speed)
    {
        EffectBehaviorAbstract result = null;

        result = new PointToPointEffect(effectKey, parent, position, to, scale, speed);

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
    /// 特效key
    /// </summary>
    private string effectKey;

    /// <summary>
    /// 父级
    /// </summary>
    private Transform parent;

    /// <summary>
    /// 特效出现位置
    /// </summary>
    private Vector3 position;

    /// <summary>
    /// 特效对象缩放
    /// </summary>
    private Vector3 scale;

    /// <summary>
    /// 持续时间
    /// </summary>
    private float durTime;

    /// <summary>
    /// 特效扩散速度
    /// </summary>
    private float speed;

    /// <summary>
    /// 特效对象
    /// </summary>
    private GameObject effectObject;

    /// <summary>
    /// 创建点特效
    /// </summary>
    /// <param name="effectKey">特效key, 可以使路径, 或者AB包中对应的key</param>
    /// <param name="parent">父级</param>
    /// <param name="position">特效出现位置</param>
    /// <param name="scale">特效缩放</param>
    /// <param name="durTime"></param>
    /// <param name="speed">TODO 特效播放速度</param>
    public PointEffect(string effectKey, Transform parent, Vector3 position, Vector3 scale, float durTime, float speed)
    {
        this.effectKey = effectKey;
        this.parent = parent;
        this.position = position;
        this.scale = scale;
        this.durTime = durTime;
        this.speed = speed;
    }

    /// <summary>
    /// 开始
    /// </summary>
    public override void Begin()
    {
        // 加载特效预设
        effectObject = EffectsLoader.Single.Load(effectKey);
        if (effectObject == null)
        {
            throw new Exception("特效为空, 加载失败.");
        }
        effectObject = Instantiate(effectObject);
        // 设置数据
        effectObject.transform.parent = parent;
        effectObject.transform.localScale = scale;
        effectObject.transform.position = position;
        // TODO 特效播放速度
        // 特效持续时间
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public override void Pause()
    {
        if (effectObject != null)
        {
            effectObject.SetActive(false);
        }
    }

    /// <summary>
    /// 继续
    /// </summary>
    public override void AntiPause()
    {
        if (effectObject != null)
        {
            effectObject.SetActive(true);
        }
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Destroy()
    {
        if (effectObject != null)
        {
            // 销毁对象
            Destroy(effectObject);
            effectObject = null;
        }
    }
}

/// <summary>
/// 点对点特效
/// </summary>
public class PointToPointEffect : EffectBehaviorAbstract
{
    /// <summary>
    /// 特效key
    /// </summary>
    private string effectKey;

    /// <summary>
    /// 父级
    /// </summary>
    private Transform parent;

    /// <summary>
    /// 特效出现位置
    /// </summary>
    private Vector3 position;

    /// <summary>
    /// 特效目标位置
    /// </summary>
    private Vector3 to;

    /// <summary>
    /// 特效对象缩放
    /// </summary>
    private Vector3 scale;

    /// <summary>
    /// 特效扩散速度
    /// </summary>
    private float speed;

    /// <summary>
    /// 特效对象
    /// </summary>
    private GameObject effectObject;

    /// <summary>
    /// 弹道
    /// </summary>
    private Ballistic ballistic;

    /// <summary>
    /// 点对点特效
    /// TODO 加入路径
    /// </summary>
    /// <param name="effectKey">特效key, 可以使路径, 或者AB包中对应的key</param>
    /// <param name="parent">父级</param>
    /// <param name="from">起点</param>
    /// <param name="to">目标点</param>
    /// <param name="scale">缩放</param>
    /// <param name="speed">速度</param>
    public PointToPointEffect(string effectKey, Transform parent, Vector3 from, Vector3 to, Vector3 scale, float speed)
    {
        this.effectKey = effectKey;
        this.parent = parent;
        this.position = from;
        this.to = to;
        this.scale = scale;
        this.speed = speed;
    }


    /// <summary>
    /// 开始移动
    /// </summary>
    public override void Begin()
    {
        // 加载特效预设
        effectObject = EffectsLoader.Single.Load(effectKey);
        if (effectObject == null)
        {
            throw new Exception("特效为空, 加载失败.");
        }
        effectObject = Instantiate(effectObject);
        // 设置数据
        effectObject.transform.parent = parent;
        effectObject.transform.localScale = scale;
        effectObject.transform.position = position;
        // 开始移动
        // 创建弹道
        ballistic = BallisticFactory.Single.CreateBallistic(effectObject, position, to - position,
                       to,
                       speed, 1,false, trajectoryType: TrajectoryAlgorithmType.Line);

        // 运行完成
        ballistic.Complete = (a, b) =>
        {
            Destroy();
        };
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public override void Pause()
    {
        if (ballistic != null)
        {
            ballistic.Pause();
        }
    }

    /// <summary>
    /// 继续
    /// </summary>
    public override void AntiPause()
    {
        if (ballistic != null)
        {
            ballistic.AntiPause();
        }
    }

    /// <summary>
    /// 销毁
    /// </summary>
    public override void Destroy()
    {
        if (ballistic != null)
        {
            Destroy(ballistic.gameObject);
        }
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
    /// 暂停
    /// </summary>
    public override void Pause()
    {

    }
    
    /// <summary>
    /// 继续
    /// </summary>
    public override void AntiPause()
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

/// <summary>
/// 特效加载器
/// </summary>
public class EffectsLoader : IResourcesLoader
{
    /// <summary>
    /// 单例
    /// </summary>
    public static EffectsLoader Single
    {
        get
        {
            if (single == null)
            {
                single = new EffectsLoader();
            }
            return single;
        }
    }

    /// <summary>
    /// 单例对象
    /// </summary>
    private static EffectsLoader single = null;

    /// <summary>
    /// 加载特效
    /// </summary>
    /// <param name="key">特效路径</param>
    /// <returns>特效对象</returns>
    public GameObject Load(string key)
    {
        GameObject result = null;

        result = (GameObject)Resources.Load(key);

        return result;
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
    /// 暂停
    /// </summary>
    public abstract void Pause();

    /// <summary>
    /// 继续
    /// </summary>
    public abstract void AntiPause();

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

    /// <summary>
    /// 暂停
    /// </summary>
    void Pause();

    /// <summary>
    /// 继续
    /// </summary>
    void AntiPause();

    /// <summary>
    /// 销毁
    /// </summary>
    void Destroy();
}



/// <summary>
/// 资源加载器
/// </summary>
public interface IResourcesLoader
{
    /// <summary>
    /// 加载
    /// </summary>
    /// <param name="key">文件对应的key, 可能是path或者对应AB包中的key</param>
    /// <returns>特效对象</returns>
    GameObject Load(string key);
}