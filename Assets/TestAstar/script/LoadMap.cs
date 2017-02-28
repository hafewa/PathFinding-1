﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 该类创建在战场
/// 由外部调用创建
/// 将初始化方法暴露, 流程具体功能不在此处描述
/// 提供抽象的流程控制
/// </summary>
public class LoadMap : MonoBehaviour
{
    //-------------------------公共属性-----------------------------

    /// <summary>
    /// 地图底板
    /// </summary>
    public GameObject MapPlane;

    /// <summary>
    /// 障碍物对象, 如果该对象为空则创建cube
    /// </summary>
    public GameObject Obstacler;

    /// <summary>
    /// 障碍物父级
    /// </summary>
    public GameObject ObstaclerList;

    /// <summary>
    /// 单位宽度
    /// </summary>
    public int UnitWidth = 1;

    /// <summary>
    /// 是否显示
    /// </summary>
    public bool IsShow;

    /// <summary>
    /// 网格线颜色
    /// </summary>
    public Color LineColor = Color.red;


    //-------------------------公共常量-----------------------------


    //-------------------------私有属性-----------------------------
    /// <summary>
    /// map数据
    /// </summary>
    private int[][] mapData = null;

    /// <summary>
    /// 地图宽度(X)
    /// </summary>
    private int mapWidth = -1;

    /// <summary>
    /// 地图高度(Y)
    /// </summary>
    private int mapHeight = -1;

    /// <summary>
    /// 地图状态
    /// </summary>
    private Dictionary<string, GameObject> mapStateDic = new Dictionary<string, GameObject>();


    //-------------------计算优化属性---------------------
    /// <summary>
    /// 半地图宽度
    /// </summary>
    private float halfMapWidth;

    /// <summary>
    /// 半地图长度
    /// </summary>
    private float halfMapHight;

    /// <summary>
    /// 地图四角位置
    /// 初始化时计算
    /// </summary>
    private Vector3 leftup = Vector3.zero;
    private Vector3 leftdown = Vector3.zero;
    private Vector3 rightup = Vector3.zero;
    private Vector3 rightdown = Vector3.zero;


    private List<GameObject> ObstaclerArray = new List<GameObject>();

    void Start () {
    }
	
	void Update () {

        // 画格子
	    DrawLine();

	}


    /// <summary>
    /// 初始化 将地图数据传入
    /// </summary>
    /// <param name="map">地图数据</param>
    public void Init(int[][] map)
    {
        // 设置本地数据
        mapData = map;

        // 验证数据
        if (!ValidateData())
        {
            Debug.LogError("参数错误!");
            return;
        }

        // TODO 初始化地图宽度长度
        mapHeight = mapData.Length;
        mapWidth = mapData[0].Length;

        // 初始化优化数据
        halfMapWidth = mapWidth / 2.0f * UnitWidth;
        halfMapHight = mapHeight / 2.0f * UnitWidth;

        // 获得起始点
        Vector3 startPosition = MapPlane.transform.position;
        // 初始化四角点
        leftup = new Vector3(-halfMapWidth + startPosition.x, (MapPlane.transform.localScale.y) / 2 + startPosition.y, halfMapHight + startPosition.z);
        leftdown = new Vector3(-halfMapWidth + startPosition.x, (MapPlane.transform.localScale.y) / 2 + startPosition.y, -halfMapHight + startPosition.z);
        rightup = new Vector3(halfMapWidth + startPosition.x, (MapPlane.transform.localScale.y) / 2 + startPosition.y, halfMapHight + startPosition.z);
        rightdown = new Vector3(halfMapWidth + startPosition.x, (MapPlane.transform.localScale.y) / 2 + startPosition.y, -halfMapHight + startPosition.z);

        // 刷新地图
        RefreshMap();
    }

    /// <summary>
    /// 刷新地图
    /// </summary>
    public void RefreshMap()
    {
        // 验证数据
        if (!ValidateData())
        {
            Debug.LogError("参数错误!");
            return;
        }
        // 清除障碍物
        CleanObstaclerList();

        // 缩放地图
        MapPlane.transform.localScale = new Vector3(mapWidth * UnitWidth, 1, mapHeight * UnitWidth);

        // 不显示逻辑地图障碍物则返回
        if (!IsShow)
        {
            return;
        }

        // 验证障碍物, 如果为空则修改为cube
        // 按照map数据构建地图大小

        // 创建格子
        for (var row = 0; row < mapData.Length; row++)
        {
            var oneRow = mapData[row];
            if (oneRow == null)
            {
                continue;
            }
            for (int col = 0; col < oneRow.Length; col++)
            {
                var cell = oneRow[col];
                var key = String.Format("{0}:{1}", col, row);
                switch (cell)
                {
                    case Utils.Obstacle:

                        // 有障碍则在该位置创建障碍物 
                        var isExist = mapStateDic.ContainsKey(key);
                        if (isExist && mapStateDic[key] == null || !isExist)
                        {
                            var newObstacler = CreateObstacler();
                            newObstacler.transform.parent = ObstaclerList == null ? null : ObstaclerList.transform;
                            newObstacler.transform.localScale = new Vector3(UnitWidth, UnitWidth, UnitWidth);
                            newObstacler.transform.position = Utils.NumToPosition(MapPlane.transform.position, new Vector2(col, row), UnitWidth, mapWidth, mapHeight);
                            mapStateDic[key] = newObstacler;
                        }
                        break;
                    case Utils.Accessibility:
                        // 无障碍 如果有则清除障碍
                        if (mapStateDic.ContainsKey(key))
                        {
                            if (mapStateDic[key] != null)
                            {
                                Destroy(mapStateDic[key]);
                                mapStateDic[key] = null;
                            }
                        }
                        break;
                }
            }
        }
    }

    //------------------------私有方法----------------------------

    /// <summary>
    /// 在地图上画出网格
    /// </summary>
    private void DrawLine()
    {
        // 不显示逻辑地图则返回
        if (!IsShow)
        {
            return;
        }
        // 在底板上画出格子
        // 画四边
        Debug.DrawLine(leftup, rightup, LineColor);
        Debug.DrawLine(leftup, leftdown, LineColor);
        Debug.DrawLine(rightdown, rightup, LineColor);
        Debug.DrawLine(rightdown, leftdown, LineColor);

        // 获得格数
        var xCount = mapWidth;
        var yCount = mapHeight;

        for (var i = 1; i <= xCount; i++)
        {
            Debug.DrawLine(leftup + new Vector3(i * UnitWidth, 0, 0), leftdown + new Vector3(i * UnitWidth, 0, 0), LineColor);
        }
        for (var i = 1; i <= yCount; i++)
        {
            Debug.DrawLine(leftdown + new Vector3(0, 0, i * UnitWidth), rightdown + new Vector3(0, 0, i * UnitWidth), LineColor);
        }
    }


    /// <summary>
    /// 创建障碍物对象
    /// 如果障碍物引用为空则创建cube
    /// </summary>
    /// <returns>障碍物引用</returns>
    private GameObject CreateObstacler()
    {
        if (Obstacler == null)
        {
            Obstacler = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(Obstacler.GetComponent<BoxCollider>());
            Obstacler.name = "Obstacler";
            Obstacler.transform.localPosition = leftup;
        }
        var result = Instantiate(Obstacler);
        ObstaclerArray.Add(result);

        return result;
    }

    /// <summary>
    /// 清空障碍物
    /// </summary>
    private void CleanObstaclerList()
    {
        if (ObstaclerArray != null && ObstaclerArray.Count > 0)
        {
            foreach (var ob in ObstaclerArray)
            {
                Destroy(ob);
            }
            ObstaclerArray.Clear();
        }
    }

    /// <summary>
    /// 验证数据
    /// </summary>
    /// <returns>是否验证通过</returns>
    private bool ValidateData()
    {

        if (mapData == null || MapPlane == null)
        {
            return false;
        }

        return true;
    }
    
}