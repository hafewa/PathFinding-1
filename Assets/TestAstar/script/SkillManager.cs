﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 技能管理器
/// </summary>
public class SkillManager : MonoBehaviour
{
    /// <summary>
    /// 单例
    /// </summary>
    public static SkillManager Single
    {
        get
        {
            if (single == null)
            {
                single = new SkillManager();
            }
            return single;
        }
    }

    /// <summary>
    /// 单例对象
    /// </summary>
    private static SkillManager single = null;
    // 创建效果使用类似lambda的方式连续公式的方式

    void Start()
    {
        single = this;
    }

    /// <summary>
    /// 执行方程式
    /// </summary>
    /// <param name="formula">方程式对象</param>
    public void DoFormula(IFormula formula)
    {
        if (formula == null)
        {
            throw new Exception("方程式对象为空.");
        }
        StartCoroutine(LoopDoFormula(formula));
    }

    /// <summary>
    /// 携程循环
    /// </summary>
    private IEnumerator LoopDoFormula(IFormula formula)
    {
        if (formula != null)
        {
            // 获取链表中的第一个
            var topNode = formula.GetFirst();

            // 顺序执行每一个操作
            do
            {
                var isWaiting = true;
                // 创建回调
                Action callback = () =>
                {
                    isWaiting = false;
                };
                topNode.Do(callback);
                if (topNode.FormulaType == Formula.FormulaWaitType)
                {
                    while (isWaiting)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                topNode = topNode.NextFormula;
            } while (topNode != null);
        }
        yield break;
    }
}


/// <summary>
/// 行为链单位
/// </summary>
public class Formula : IFormula
{

    /// <summary>
    /// 下一个节点
    /// </summary>
    public IFormula NextFormula { get; set; }

    /// <summary>
    /// 上一个节点
    /// </summary>
    public IFormula PreviewFormula { get; set; }

    /// <summary>
    /// 不等待直接下一节点执行
    /// </summary>
    public const int FormulaNotWaitType = 0;

    /// <summary>
    /// 等待完成后继续执行
    /// </summary>
    public const int FormulaWaitType = 1;

    // -------------------------属性---------------------------


    /// <summary>
    /// 方程式类型
    /// 0: 无需等待直接继续下一节点
    /// 1: 等待当前节点执行完成再执行下一节点
    /// </summary>
    public int FormulaType {
        get { return formulaType; }
        set { formulaType = value; } }

    /// <summary>
    /// 执行操作 
    /// 外部只读
    /// </summary>
    public Action<Action> Do { get; private set; }



    ///// <summary>
    ///// 前一个行为
    ///// </summary>
    //private IFormula beforeBehavior = null;

    ///// <summary>
    ///// 后一个行为
    ///// </summary>
    //private IFormula afterBehavior = null;

    /// <summary>
    /// 方程式执行方式
    /// </summary>
    private int formulaType = FormulaNotWaitType;


    // -----------------------公用方法-----------------------
    

    /// <summary>
    /// 构建方法
    /// 传入执行操作
    /// </summary>
    /// <param name="doForWaitAction">执行操作(等待完成)</param>
    /// <param name="type">执行类型</param>
    public Formula(Action<Action> doForWaitAction, int type = FormulaNotWaitType)
    {
        Do = doForWaitAction;
        formulaType = type;
    }

    /// <summary>
    /// 获取链表头
    /// </summary>
    /// <returns>链表表头单位</returns>
    public IFormula GetFirst()
    {
        IFormula tmpItem = PreviewFormula;
        IFormula first = this;
        while (tmpItem != null)
        {
            first = tmpItem;
            tmpItem = tmpItem.PreviewFormula;
        }
        return first;
    }

    /// <summary>
    /// 行为链中是否有下一单位
    /// </summary>
    /// <returns></returns>
    public bool HasNext()
    {
        if (NextFormula != null)
        {
            return true;
        }
        return false;
    }
    

    /// <summary>
    /// 添加下一个执行
    /// </summary>
    /// <param name="nextBehavior">下一个执行对象</param>
    /// <returns>当前对象</returns>
    public IFormula After(IFormula nextBehavior)
    {
        if (nextBehavior != null)
        {
            // 如果后一个单位不为空则向后移
            if (NextFormula != null)
            {
                NextFormula.After(NextFormula);
            }
            NextFormula = nextBehavior;
            nextBehavior.PreviewFormula = this;

            return nextBehavior;
        }
        return this;
    }

    /// <summary>
    /// 添加前一个执行
    /// </summary>
    /// <param name="preBehavior">前一个执行对象</param>
    /// <returns>当前对象</returns>
    public IFormula Before(IFormula preBehavior)
    {
        if (preBehavior != null)
        {
            // 如果前一个单位不为空则向前移
            if (PreviewFormula != null)
            {
                PreviewFormula.Before(PreviewFormula);
            }
            PreviewFormula = preBehavior;
            preBehavior.NextFormula = this;

            return preBehavior;
        }
        return this;
    }

    ///// <summary>
    ///// 执行该公式链(从头开始执行)
    ///// 顺序执行无
    ///// </summary>
    //public void DoFormula()
    //{
    //    // 获取链表中的第一个
    //    var topNode = GetFirst();

    //    // 顺序执行每一个操作
    //    while (topNode.HasNext())
    //    {
    //        topNode.Do();
    //        topNode = topNode.GetNext();
    //    }
    //}
}




// ----------------------------抽象类-------------------------------

/// <summary>
/// 行为链单位接口
/// </summary>
public interface IFormula
{
    /// <summary>
    /// 下一个节点
    /// </summary>
    IFormula NextFormula { get; set; }

    /// <summary>
    /// 上一个节点
    /// </summary>
    IFormula PreviewFormula { get; set; }
    /// <summary>
    /// 方程式类型
    /// 0: 无需等待直接继续下一节点
    /// 1: 等待当前节点执行完成再执行下一节点
    /// </summary>
    int FormulaType { get; set; }

    /// <summary>
    /// 具体执行lambda表达式
    /// </summary>
    Action<Action> Do { get; }

    /// <summary>
    /// 添加下一个执行
    /// </summary>
    /// <param name="nextBehavior">下一个执行对象</param>
    /// <returns>当前对象</returns>
    IFormula After(IFormula nextBehavior);

    /// <summary>
    /// 添加前一个执行
    /// </summary>
    /// <param name="preBehavior">前一个执行对象</param>
    /// <returns>当前对象</returns>
    IFormula Before(IFormula preBehavior);

    /// <summary>
    /// 行为链中是否有下一单位
    /// </summary>
    /// <returns></returns>
    bool HasNext();

    ///// <summary>
    ///// 获取下一个单位
    ///// </summary>
    ///// <returns>下一个单位</returns>
    //IFormula GetNext();

    ///// <summary>
    ///// 获取上一个单位
    ///// </summary>
    ///// <returns></returns>
    //IFormula GetPreview();

    /// <summary>
    /// 获取链表头
    /// </summary>
    /// <returns>链表头单位</returns>
    IFormula GetFirst();

    ///// <summary>
    ///// 执行方程式
    ///// </summary>
    ///// <param name="skillBehavior"></param>
    //void DoFormula();
}