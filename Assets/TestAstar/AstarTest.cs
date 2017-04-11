﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Random = System.Random;

public class AstarTest : MonoBehaviour {
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

    ///// <summary>
    ///// 其实x
    ///// </summary>
    //public int StartX = 0;

    ///// <summary>
    ///// 起始Y
    ///// </summary>
    //public int StartY = 0;

    ///// <summary>
    ///// 目标X
    ///// </summary>
    //public int TargetX = 0;

    ///// <summary>
    ///// 目标Y
    ///// </summary>
    //public int TargetY = 0;

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

    /// <summary>
    /// 集群引用
    /// </summary>
    public SchoolManager schoolManager;
    


    /// <summary>
    /// 路径点列表
    /// </summary>
    private IList<GameObject> pathPoint = new List<GameObject>();

    /// <summary>
    /// 主相机
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// 上次目标点X
    /// </summary>
    private int lastTimeTargetX = 0;

    /// <summary>
    /// 上次目标点Y
    /// </summary>
    private int lastTimeTargetY = 0;
    

    void Start () {

        // 设定帧数
        Application.targetFrameRate = 60;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        var loadMapPos = LoadMap.GetLeftBottom();
        schoolManager.Init(loadMapPos.x, loadMapPos.z, MapWidth, MapHeight, UnitWidth, null);
        InitMapInfo();
    }

    void Update()
    {
        // 控制
        Control();
    }

    /// <summary>
    /// 控制
    /// </summary>
    private void Control()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 获取地图上的点击点
            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            // 点击到底板
            if (hit.collider != null && hit.collider.name.Equals(LoadMap.MapPlane.name))
            {
                var posOnMap = Utils.PositionToNum(LoadMap.MapPlane.transform.position, hit.point, UnitWidth, MapWidth, MapHeight);
                // 加载文件内容
                var mapInfoData = InitMapInfo();
                var path = AStarPathFinding.SearchRoad(mapInfoData, lastTimeTargetX, lastTimeTargetY, posOnMap[0], posOnMap[1], DiameterX, DiameterY, IsJumpPoint);
                // 根据path放地标, 使用组队寻路跟随过去
                StartCoroutine(Step(path));
                
                var loadMapPos = LoadMap.GetLeftBottom();
                schoolManager.Init(loadMapPos.x, loadMapPos.z, MapWidth, MapHeight, UnitWidth, mapInfoData);
                StartMoving(path, mapInfoData, lastTimeTargetX, lastTimeTargetY);

                // 缓存上次目标点
                lastTimeTargetX = posOnMap[0];
                lastTimeTargetY = posOnMap[1];

            }
        }

        // 上下左右移动
        if (Input.GetKey(KeyCode.UpArrow))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z + 1);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z - 1);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x - 1, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x + 1, mainCamera.transform.localPosition.y, mainCamera.transform.localPosition.z);
        }
        // 升高下降
        if (Input.GetKey(KeyCode.PageUp))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y + 1, mainCamera.transform.localPosition.z);
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, mainCamera.transform.localPosition.y - 1, mainCamera.transform.localPosition.z);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public int[][] InitMapInfo()
    {
        var mapInfoPath = Application.dataPath + Path.AltDirectorySeparatorChar + "mapinfo";
        var mapInfoStr = Utils.LoadFileInfo(mapInfoPath);
        var mapInfoData = DeCodeInfo(mapInfoStr);
        LoadMap.Init(mapInfoData, UnitWidth);
        schoolManager.Init(-MapWidth / 2, -MapHeight / 2, MapWidth, MapHeight, 10, mapInfoData);
        MapWidth = mapInfoData[0].Length;
        MapHeight = mapInfoData.Length;
        return mapInfoData;
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
    /// <param name="pathList">列表</param>
    /// <param name="map">地图信息</param>
    private void StartMoving(IList<Node> pathList, int[][] map, int startX, int startY)
    {
        if (pathList == null || pathList.Count == 0)
        {
            return;
        }
        // 清除所有组
        schoolManager.ClearAll();
        GameObject schoolItem = null;
        SchoolBehaviour school = null;
        var cloneList = new List<Node>(pathList);
        var target = Utils.NumToPosition(LoadMap.transform.position, new Vector2(cloneList[cloneList.Count - 1].X, cloneList[cloneList.Count - 1].Y), UnitWidth, MapWidth, MapHeight);
        var start = Utils.NumToPosition(LoadMap.transform.position, new Vector2(startX, startY), UnitWidth, MapWidth, MapHeight);
        for (int i = 0; i < ItemCount; i++)
        {
            schoolItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            school = schoolItem.AddComponent<SchoolBehaviour>();
            school.GroupId = i > 10 ? 2 : 1;
            school.PhysicsInfo.MaxSpeed = 10;
            school.RotateSpeed = 1;
            school.RotateWeight = 1;
            school.transform.localPosition = new Vector3((i % 3) * 2 + start.x, start.y, i / 3 * 2 + start.z);
            school.name = "item" + i;
            school.TargetPos = target;
            school.Diameter = (i == 0 ? 5 : 2) * UnitWidth;
            //school.Moveing = (a) => { Debug.Log(a.name + "Moving"); };

            //school.Wait = (a) => { Debug.Log(a.name + "Wait"); };
            //school.Complete = (a) => { Debug.Log(a.name + "Complete"); };

            schoolManager.Add(school);
        }

        GameObject fixItem = null;
        FixtureBehaviour fix = null;

        // 遍历地图将障碍物加入列表
        for (var i = 0; i < map.Length; i++)
        {
            var row = map[i];
            for (int j = 0; j < row.Length; j++)
            {
                switch (row[j])
                {
                    case Utils.Obstacle:
                        fixItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        fixItem.name += i;
                        fix = fixItem.AddComponent<FixtureBehaviour>();
                        fix.transform.localScale = new Vector3(UnitWidth, UnitWidth, UnitWidth);
                        fix.transform.position = Utils.NumToPosition(transform.position, new Vector2(j, i), UnitWidth, MapWidth, MapHeight);
                        fix.Diameter = 1*UnitWidth;
                        schoolManager.Add(fix);
                        break;
                }
            }
        }

        //school.Group.Target = Utils.NumToPosition(LoadMap.transform.position, new Vector2(cloneList[cloneList.Count - 1].X, cloneList[cloneList.Count - 1].Y), UnitWidth, MapWidth, MapHeight); 
        

        Action<SchoolGroup> lambdaComplete = (thisGroup) =>
        {
            // Debug.Log("GroupComplete:" + thisGroup.Target);
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
        school.Group.ProportionOfComplete = 1;
        school.Group.Complete = lambdaComplete;
    }



}