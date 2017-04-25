﻿using UnityEngine;
using System.Collections;

public class TestFormulaConstruct : MonoBehaviour
{
    public GameObject Parent;


    private string formulaStr = @"SkillNum(1000)
{
        PointToPoint(1,test/TrailPrj,1,0,10,1),
        Point(1,test/ExplordScope,0,0,3),
        PointToPoint(1,test/TrailPrj,0,1,10,1),
        Point(1,test/ExplordScope,1,0,3),
}";


    private SkillInfo skillInfo = null;

	void Start () {

        // 加载技能内容
        skillInfo = FormulaConstructor.Constructor(formulaStr);

    }
	

	void Update () {
	    if (Input.GetMouseButtonUp(0))
	    {
            // 创建技能
	        var formula = skillInfo.GetFormula(new FormulaParamsPacker()
	        {
	            StartPos = new Vector3(10, 0, 10),
                TargetPos = new Vector3(100, 0, 0),
            });

            // TODO 如何封装数据?
            SkillManager.Single.DoFormula(formula.GetFirst());
	    }
	}
}