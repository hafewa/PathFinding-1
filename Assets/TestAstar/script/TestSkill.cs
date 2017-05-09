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

    /// <summary>
    /// 主相机
    /// </summary>
    public Camera MainCamera;

    /// <summary>
    /// 平台
    /// </summary>
    public GameObject Plane;


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
        // 点击产生目标点
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name.Equals(Plane.name))
                {
                    TargetPos = hit.point;
                }
            }
        }

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
        else if (Input.GetKeyUp(KeyCode.U))
        {
            SkillNum = 7;
            Debug.Log("切换 复活");
        }
        else if (Input.GetKeyUp(KeyCode.I))
        {
            SkillNum = 8;
            Debug.Log("切换 战地急救");
        }
        else if (Input.GetKeyUp(KeyCode.O))
        {
            SkillNum = 9;
            Debug.Log("切换 魅惑");
        }
        else if (Input.GetKeyUp(KeyCode.P))
        {
            SkillNum = 10;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            SkillNum = 11;
            Debug.Log("切换 跟踪飞弹");
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            SkillNum = 12;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            SkillNum = 13;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            SkillNum = 14;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.G))
        {
            SkillNum = 15;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.H))
        {
            SkillNum = 16;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.J))
        {
            SkillNum = 17;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            SkillNum = 18;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.L))
        {
            SkillNum = 19;
            Debug.Log("切换 粘附燃料");
        }
        else if (Input.GetKeyUp(KeyCode.Z))
        {
            SkillNum = 20;
            Debug.Log("切换 粘附燃料");
        }


        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("暂停继续");
            if (SkillManager.isPause)
            {
                SkillManager.Single.Start();
            }
            else
            {
                SkillManager.Single.Pause();
            }
        }



        if (Input.GetMouseButtonUp(0))
        {

            var myPos = TargetPos;
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
                            myPos, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
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
                            myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
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
                            myPos, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                    };
                    // 步骤2
                    Action<Action> step2 = (callback) =>
                    {
                        // 创建攻击体
                        Debug.Log("创建攻击检测对象, 检测范围内对象.");
                        Debug.Log("创建范围攻击特效");
                        EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(10, 10, 10), 3, 1, callback).Begin();

                    };

                    // 测试暂停工能

                    var pauseFormula = new Formula((callback) =>
                    {
                        var timer = new Timer(0.1f);
                        Action completeCallback = () => { };

                        completeCallback = () =>
                        {
                            if (SkillManager.isPause)
                            {
                                // 继续暂停
                                timer = new Timer(0.1f);
                                timer.OnCompleteCallback(completeCallback);
                                timer.Start();
                            }
                            else
                            {
                                // 暂停结束
                                callback();
                            }
                        };

                        timer.OnCompleteCallback(completeCallback);
                        timer.Start();
                    }, 1);

                    // 技能 震爆弹
                    var formula = new Formula(step1, Formula.FormulaWaitType)
                        .After(pauseFormula)
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
                        targetObj.transform.position = myPos;
                        callback();
                    };
                    // 步骤2
                    Action<Action> step2 = (callback) =>
                    {
                        // 发射跟踪
                        EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                            myPos, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                    };
                    // 步骤3
                    Action<Action> step3 = (callback) =>
                    {
                        // 结算与特效
                        Debug.Log("伤害检测与特效");
                        EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                            myPos, new Vector3(1, 1, 1), 10, 1, callback).Begin();
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
                            targetObj.transform.position = myPos;
                            callback();
                        };
                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                                myPos, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                        };
                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(10, 1, 10), 3, 1, callback).Begin();
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
                            targetObj.transform.position = myPos;
                            callback();
                        };
                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToPointEffect("test/TrailPrj", null, StartPos,
                                myPos, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                        };
                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(3, 3, 3), 1, 1, callback).Begin();
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
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测周围友军单位, 增加buff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(20, 0.2f, 20), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 6:
                {
                        Debug.Log("牺牲(嘲讽)");
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测周围敌军, 增加Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(40, 0.1f, 40), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 7:
                {
                        Debug.Log("复活");
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测坟场中附近单位, 复活效果 与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(30, 0.1f, 30), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 8:
                {
                        Debug.Log("战地急救");
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            // 发射跟踪
                            Debug.Log("碰撞检测附近友军单位, 上加血Buff 与特效");
                            EffectsFactory.Single.CreatePointEffect("test/ExplordScope", null,
                            myPos, new Vector3(20, 0.1f, 30), 1, 1, callback).Begin();
                        };

                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 9:
                {
                        Debug.Log("魅惑");
                        GameObject targetObj = null;
                        // 步骤1
                        Action<Action> step1 = (callback) =>
                        {
                            Debug.Log("创建目标");
                            // 创建目标
                            targetObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            targetObj.transform.position = myPos;
                            callback();
                        };
                        // 步骤2
                        Action<Action> step2= (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("增加魅惑Debuff与特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                            myPos, new Vector3(3, 3, 3), 1, 1, callback).Begin();
                        };
                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            Debug.Log("销毁目标");
                            Destroy(targetObj);
                        };

                    var formula = new Formula(step1, Formula.FormulaWaitType)
                        .After(new Formula(step2, Formula.FormulaWaitType))
                        .After(new Formula(step3, Formula.FormulaWaitType));

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 10:
                {
                    Debug.Log("粘附燃料");
                    Action<Action> step1 = (callback) =>
                    {
                        // 结算与特效
                        Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                        EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                            myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                    };
                    var formula = new Formula(step1, Formula.FormulaWaitType);

                    // 执行技能效果
                    SkillManager.Single.DoFormula(formula);
                }
                    break;
                case 11:
                {
                    Debug.Log("跟踪飞弹");
                    var random = new System.Random(DateTime.Now.Millisecond);
                    {
                        GameObject targetObj1 = null;
                        Action<Action> step1 = (callback) =>
                        {
                            // 范围内创建两个随机位置对象
                            targetObj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            targetObj1.transform.position =
                                new Vector3(random.Next((int) (myPos.x - 10), (int) (myPos.x + 10)), 0,
                                    random.Next((int) (myPos.z - 10), (int) (myPos.z + 10)));

                            callback();
                        };


                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToObjEffect("test/TrailPrj", null, StartPos,
                                targetObj1, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                        };

                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            Debug.Log("销毁目标");
                            Destroy(targetObj1);
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType)
                            .After(new Formula(step2, Formula.FormulaWaitType))
                            .After(new Formula(step3));

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);

                    }
                    {
                        GameObject targetObj1 = null;
                        Action<Action> step1 = (callback) =>
                        {
                            // 范围内创建两个随机位置对象
                            targetObj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            targetObj1.transform.position =
                                new Vector3(random.Next((int) (myPos.x - 10), (int) (myPos.x + 10)), 0,
                                    random.Next((int) (myPos.z - 10), (int) (myPos.z + 10)));

                            callback();
                        };

                        // 步骤2
                        Action<Action> step2 = (callback) =>
                        {
                            // 发射跟踪
                            EffectsFactory.Single.CreatePointToObjEffect("test/TrailPrj", null, StartPos,
                                targetObj1, new Vector3(1, 1, 1), 100, TrajectoryAlgorithmType.Line, callback).Begin();
                        };

                        // 步骤3
                        Action<Action> step3 = (callback) =>
                        {
                            Debug.Log("销毁目标");
                            Destroy(targetObj1);
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType)
                            .After(new Formula(step2, Formula.FormulaWaitType))
                            .After(new Formula(step3));

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }

                }
                    break;
                case 12:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 13:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 14:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 15:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 16:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 17:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 18:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 19:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
                case 20:
                    {
                        Debug.Log("粘附燃料");
                        Action<Action> step1 = (callback) =>
                        {
                            // 结算与特效
                            Debug.Log("创建伤害检测碰撞区域, 并创建特效");
                            EffectsFactory.Single.CreatePointEffect("test/PointEffect", null,
                                myPos, new Vector3(3, 3, 3), 10, 1, callback).Begin();
                        };
                        var formula = new Formula(step1, Formula.FormulaWaitType);

                        // 执行技能效果
                        SkillManager.Single.DoFormula(formula);
                    }
                    break;
            }
        }


        // ------------------------主动技能--------------------------
    }
}
