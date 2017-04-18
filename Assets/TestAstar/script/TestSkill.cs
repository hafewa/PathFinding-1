﻿using System;
using UnityEngine;
using System.Collections;
using Util;

/// <summary>
/// 测试技能
/// </summary>
public class TestSkill : MonoBehaviour {
    
    /// <summary>
    /// 发射位置
    /// </summary>
    public Vector3 StartPos = new Vector3(0, 0, 0);

    /// <summary>
    /// 目标位置
    /// </summary>
    public Vector3 TargetPos = new Vector3(0, 0, 100);

    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillNum = 0;

	void Start () {
	
	}
	
	void Update ()
	{
        // 控制方法
	    Control();

	}


    /// <summary>
    /// 控制
    /// </summary>
    private void Control()
    {

        // ------------------------主动技能--------------------------
        // 点击鼠标左键发射技能


        if (Input.GetKeyUp(KeyCode.Q))
        {
            // 震爆弹
            SkillNum = 1;
            Debug.Log("切换 震爆弹");
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            SkillNum = 2;
            Debug.Log("切换 制导弹头");
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            SkillNum = 3;
            Debug.Log("切换 EMP手雷");
        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            SkillNum = 4;
            Debug.Log("切换 致幻手雷");
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            SkillNum = 5;
            Debug.Log("切换 士气");
        }
        else if (Input.GetKeyUp(KeyCode.Y))
        {
            SkillNum = 6;
            Debug.Log("切换 牺牲(嘲讽)");
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {

        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {

        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {

        }
        else if (Input.GetKey(KeyCode.Alpha9))
        {

        }



        if (Input.GetMouseButtonUp(0))
        {


            switch (SkillNum)
            {
                case 0:
                {
                    // TODO 加入状态判断
                    // 创建技能
                    var formula = new Formula((callback) =>
                    {
                        // 效果1
                        EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                            TargetPos, new Vector3(1, 1, 1), 100, callback).Begin();
                        Debug.Log("特效2");

                    }, Formula.FormulaWaitType).After(new Formula((callback) =>
                    {
                        // 等待1秒
                        new Timer(1).OnCompleteCallback(callback).Start();
                        Debug.Log("等待1");
                    })).After(new Formula((callback) =>
                    {
                        // 效果2
                        EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                            TargetPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        Debug.Log("特效1");
                    }));

                    // 执行技能效果
                    SkillManager.Single.DoFormula(formula);
                }
                    break;
                case 1:
                {
                    // 步骤1
                    Action<Action> step1 = (callback) =>
                    {
                        // 攻击物向目标飞行
                        EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                            TargetPos, new Vector3(1, 1, 1), 100, callback).Begin();
                    };
                    // 步骤2
                    Action<Action> step2 = (callback) =>
                    {
                        // 创建攻击体
                        Debug.Log("创建攻击检测对象, 检测范围内对象.");
                        Debug.Log("创建范围攻击特效");
                        EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            TargetPos, new Vector3(10, 10, 10), 3, 1, callback).Begin();

                    };
                    
                    // 技能 震爆弹
                    var formula = new Formula(step1, Formula.FormulaWaitType)
                        .After(new Formula(step2));

                    // 执行技能效果
                    SkillManager.Single.DoFormula(formula);
                }
                    break;
                case 2:
                {

                    // 技能 制导弹头
                    GameObject targetObj = null;
                    // 步骤1
                    Action<Action> step1 = (callback) =>
                    {
                        // 创建目标
                        targetObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        targetObj.transform.position = TargetPos;
                        callback();
                    };
                    // 步骤2
                    Action<Action> step2 = (callback) =>
                    {
                        // 发射跟踪
                        EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                            TargetPos, new Vector3(1, 1, 1), 100, callback).Begin();
                    };
                    // 步骤3
                    Action<Action> step3 = (callback) =>
                    {
                        // 结算与特效
                        Debug.Log("伤害检测与特效");
                        EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                            TargetPos, new Vector3(1, 1, 1), 10, 1, callback).Begin();
                    };
                    // 步骤4
                    Action<Action> step4 = (callback) =>
                    {
                        Destroy(targetObj);
                    };

                    var formula = new Formula(step1, Formula.FormulaWaitType)
                        .After(new Formula(step2, Formula.FormulaWaitType))
                        .After(new Formula(step3, Formula.FormulaWaitType))
                        .After(new Formula(step4));

                    // 执行技能效果
                    SkillManager.Single.DoFormula(formula);
                }
                    break;
                case 3:
                {
                        Debug.Log("EMP手雷");
                        // EMP手雷效果
                        GameObject targetObj = null;
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 创建目标
                            targetObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            targetObj.transform.position = TargetPos;
                            callback();
                        };
                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                                TargetPos, new Vector3(1, 1, 1), 100, callback).Begin();
                        };
                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            TargetPos, new Vector3(10, 1, 10), 3, 1, callback).Begin();
                        };
                        // 步骤4
                        Action<Action> step4 = (callback) =>
                        {
                            Destroy(targetObj);
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType)
                            .After(new Formula(step2, Formula.FormulaWaitType))
                            .After(new Formula(step3, Formula.FormulaWaitType))
                            .After(new Formula(step4));

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 4:
                {
                        // 技能 致幻手雷
                        Debug.Log("致幻手雷");
                        GameObject targetObj = null;
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 创建目标
                            targetObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            targetObj.transform.position = TargetPos;
                            callback();
                        };
                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                                TargetPos, new Vector3(1, 1, 1), 100, callback).Begin();
                        };
                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            TargetPos, new Vector3(3, 3, 3), 1, 1, callback).Begin();
                        };
                        // 步骤4
                        Action<Action> step4 = (callback) =>
                        {
                            Destroy(targetObj);
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType)
                            .After(new Formula(step2, Formula.FormulaWaitType))
                            .After(new Formula(step3, Formula.FormulaWaitType))
                            .After(new Formula(step4));

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 5:
                    { 
                        Debug.Log("士气");
                        // 步骤2
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测周围友军单位, 增加buff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            TargetPos, new Vector3(20, 0.2f, 20), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 6:
                {
                        Debug.Log("牺牲(嘲讽)");
                        // 步骤2
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测周围敌军, 增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            TargetPos, new Vector3(40, 0.1f, 40), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 7:
                {
                }
                    break;
                case 8:
                {
                }
                    break;
                case 9:
                {
                }
                    break;
                case 10:
                {
                }
                    break;
            }
        }


        // ------------------------主动技能--------------------------
    }
}
