﻿using UnityEngine;
using System.Collections;
using Util;

public class TestTimer : MonoBehaviour {
    
	void Update () {
        if (Input.GetMouseButtonDown(1))
        {
            var timer = new Timer(1);
            timer.Start().OnCompleteCallback(() => { Debug.Log("TimesUp"); });
            // LooperImpl.Single.Remove(10000);
            //for(var i = 0; i < 1000; i++)
            //    key = LooperImpl.Single.Add(new LoopItemImpl()
            //    {
            //        DoAction = () => { double x = Math.Sqrt(100); },
            //        DoOnDestory = () => { Debug.Log("destory"); }
            //    });

        }
    }
}
