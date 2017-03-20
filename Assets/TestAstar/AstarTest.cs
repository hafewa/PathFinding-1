﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AstarTest : MonoBehaviour {

    private IList<GameObject> pathPoint = new List<GameObject>();
    private GameObject main;

    /// <summary>
    /// 寻路X轴宽度
    /// </summary>
    public int DiameterX = 4;

    /// <summary>
    /// 寻路Y轴宽度
    /// </summary>
    public int DiameterY = 2;

    /// <summary>
    /// 随机
    /// </summary>
    //public int RandomRate = 40;

    /// <summary>
    /// 地图宽度
    /// </summary>
    public int MapWidth = 50;

    /// <summary>
    /// 地图高度
    /// </summary>
    public int MapHeight = 50;

    /// <summary>
    /// 单位宽度
    /// </summary>
    public int UnitWidth = 1;

    /// <summary>
    /// 其实x
    /// </summary>
    public int StartX = 0;

    /// <summary>
    /// 起始Y
    /// </summary>
    public int StartY = 0;

    /// <summary>
    /// 目标X
    /// </summary>
    public int TargetX = 0;

    /// <summary>
    /// 目标Y
    /// </summary>
    public int TargetY = 0;

    /// <summary>
    /// 是否提供跳点路径
    /// </summary>
    public bool IsJumpPoint = false;

    /// <summary>
    /// 地图加载
    /// </summary>
    public LoadMap LoadMap;

    /// <summary>
    /// 路径点对象
    /// </summary>
    public GameObject PathPoint;

    /// <summary>
    /// 路径点父级
    /// </summary>
    public GameObject PathPointFather;

    /// <summary>
    /// 创建集群个数
    /// </summary>
    public int ItemCount = 10;
    

    void Start () {
        // 构建地图
	    //var map = RandomMap(MapWidth, MapWidth);

        //var path = SearchRoad(map, 0, 0, MapWidth - 1, MapWidth - 1, Diameter);

	    //init(map);

        //StartCoroutine(Step(path));

        Application.targetFrameRate = 60;

	    main = GameObject.Find("Main Camera");


    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            // 加载文件内容
            var mapInfoPath = Application.dataPath + Path.AltDirectorySeparatorChar + "mapinfo";
            var mapInfoStr = Utils.LoadFileInfo(mapInfoPath);
            var mapInfoData = DeCodeInfo(mapInfoStr);
            //var map = RandomMap(MapWidth, MapWidth);

            MapWidth = mapInfoData[0].Length;
            MapHeight = mapInfoData.Length;
            if (TargetX >= MapWidth || TargetX < 0)
            {
                TargetX = MapWidth - 1;
            }
            if (TargetY >= MapHeight || TargetY < 0)
            {
                TargetY = MapHeight - 1;
            }
            var path = AStarPathFinding.SearchRoad(mapInfoData, StartX, StartY, TargetX, TargetY, DiameterX, DiameterY, IsJumpPoint);
           
            init(mapInfoData);
            StartCoroutine(Step(path));
            // 根据path放地标, 使用组队寻路跟随过去

            StartMoving(path);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x, main.transform.localPosition.y, main.transform.localPosition.z + 1);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x, main.transform.localPosition.y, main.transform.localPosition.z - 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x - 1, main.transform.localPosition.y, main.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x + 1, main.transform.localPosition.y, main.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x, main.transform.localPosition.y + 1, main.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            main.transform.localPosition = new Vector3(main.transform.localPosition.x, main.transform.localPosition.y - 1, main.transform.localPosition.z);
        }

    }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="map">地图数据</param>
    void init(int[][] map)
    {
        LoadMap.Init(map);


        //var cube = GameObject.Find("Cube");
        //var cubeFather = GameObject.Find("cubeFather");
        //Destroy(cubeFather);
        //cubeFather = new GameObject("cubeFather");
        //按照map数组生成地图
        //for (var row = 0; row < map.Length; row++)
        //{
        //    var rowObj = map[row];
        //    for (var col = 0; col < rowObj.Length; col++)
        //    {
        //        var cell = rowObj[col];
        //        if (cell == Obstacle)
        //        {
        //            var ObstacleObj = Instantiate(cube);
        //            ObstacleObj.transform.parent = cubeFather.transform;
        //            //Debug.Log("X:" + col + " Y:" + row);
        //            ObstacleObj.transform.localPosition = new Vector3((float)(0 + col * 1), 0, (float)(0 - row * 1));
        //        }
        //    }
        //}
    }
    
    /// <summary>
    /// 携程移动
    /// </summary>
    /// <param name="path">移动路径</param>
    IEnumerator Step(IList<Node> path)
    {
        // 删除所有
        pathPoint.All((item) => {
            Destroy(item);
            return item;
        });
        pathPoint.Clear();
        var pathArray = path.ToArray();
        Array.Reverse(pathArray);
        foreach (var node in pathArray)
        {
            for (var x = 0; x < DiameterX; x++)
            {
                for (var y = 0; y < DiameterY; y++)
                {
                    var newSphere = Instantiate(PathPoint);
                    newSphere.transform.parent = PathPointFather == null ? null : PathPointFather.transform;
                    newSphere.transform.position = Utils.NumToPosition(LoadMap.transform.position, new Vector2(node.X, node.Y), UnitWidth, MapWidth, MapHeight)
                        + new Vector3(UnitWidth * x, 0, UnitWidth * y);
                    pathPoint.Add(newSphere);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    /// <summary>
    /// 随机生成地图
    /// </summary>
    /// <param name="x">地图宽度</param>
    /// <param name="y">地图高度</param>
    /// <returns>地图数据</returns>
    //int[][] RandomMap(int x, int y)
    //{
    //    var random = new Random();
    //    var map = new int[y][];
    //    for (var i = 0; i < y; i++)
    //    {
    //        if (map[i] == null)
    //        {
    //            map[i] = new int[x];
    //        }
    //        for (var j = 0; j < x; j++)
    //        {

    //            map[i][j] = random.Next(RandomRate) > 1 ? Accessibility : Obstacle;
    //        }
    //    }
    //    return map;
    //}


    /// <summary>
    /// TODO 解码地图数据
    /// </summary>
    /// <param name="mapInfoJson">地图数据json</param>
    /// <returns>地图数据数组</returns>
    private int[][] DeCodeInfo(string mapInfoJson)
    {
        if (string.IsNullOrEmpty(mapInfoJson))
        {
            return null;
        }
        //var mapData = new List<List<int>>();
        // 读出数据
        var mapLines = mapInfoJson.Split('\n');

        int[][] mapInfo = new int[mapLines.Length - 1][];
        for (var row = 0; row < mapLines.Length; row++)
        {
            var line = mapLines[row];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }
            var cells = line.Split(',');
            // Debug.Log(line);
            mapInfo[row] = new int[cells.Length];
            for (int col = 0; col < cells.Length; col++)
            {
                if (string.IsNullOrEmpty(cells[col].Trim()))
                {
                    continue;
                }
                //Debug.Log(cells[col]);
                mapInfo[row][col] = int.Parse(cells[col]);
            }
        }

        return mapInfo;
    }

    /// <summary>
    /// 集群功能
    /// 组建集群开始根据路径点移动
    /// </summary>
    /// <param name="pathList"></param>
    private void StartMoving(IList<Node> pathList)
    {
        // 清除所有组
        SchoolManager.ClearAll();
        GameObject schoolItem = null;
        SchoolBehaviour school = null;
        var cloneList = new List<Node>(pathList);
        var target = Utils.NumToPosition(LoadMap.transform.position, new Vector2(cloneList[cloneList.Count - 1].X, cloneList[cloneList.Count - 1].Y), UnitWidth, MapWidth, MapHeight);
       
        for (int i = 0; i < ItemCount; i++)
        {
            schoolItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            school = schoolItem.AddComponent<SchoolBehaviour>();
            school.GroupId = i > 10 ? 2 : 1;
            school.Speed = 10;
            school.RotateSpeed = 1;
            school.RotateWeight = 1;
            school.transform.localPosition = new Vector3((i % 3) * 2, 1, i / 3 * 2);
            school.name = "item" + i;
            school.TargetPos = target;
            school.Diameter = 4;
            //school.Moveing = (a) => { Debug.Log(a.name + "Moving");};

            //school.Wait = (a) => { Debug.Log(a.name + "Wait"); };
            //school.Complete = (a) => { Debug.Log(a.name + "Complete"); };
            SchoolManager.MemberList.Add(school);
        }

        // TODO 将障碍物加入列表

        //school.Group.Target = Utils.NumToPosition(LoadMap.transform.position, new Vector2(cloneList[cloneList.Count - 1].X, cloneList[cloneList.Count - 1].Y), UnitWidth, MapWidth, MapHeight); 
        

        Action<SchoolGroup> lambdaComplete = (thisGroup) =>
        {
            //Debug.Log("GroupComplete:" + thisGroup.Target);
            // 数据本地化
            // 数据结束
            if (cloneList.Count == 0)
            {
                return;
            }
            cloneList.RemoveAt(cloneList.Count - 1);
            if (cloneList.Count == 0)
            {
                return;
            }
            var node = cloneList[cloneList.Count - 1];
            thisGroup.Target = Utils.NumToPosition(LoadMap.transform.position, new Vector2(node.X, node.Y), UnitWidth, MapWidth, MapHeight);
        };
        school.Group.ProportionOfComplete = 10;
        school.Group.Complete = lambdaComplete;
    }



}