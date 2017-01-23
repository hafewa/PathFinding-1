using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Random = System.Random;

public class AstarTest : MonoBehaviour {

    private IList<GameObject> pathPointList = new List<GameObject>();
    private GameObject main;

    /// <summary>
    /// 寻路直径
    /// </summary>
    public int Diameter = 4;

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
    

    void Start () {
        // 构建地图
	    //var map = RandomMap(MapWidth, MapWidth);

        //var path = SearchRoad(map, 0, 0, MapWidth - 1, MapWidth - 1, Diameter);

	    //init(map);

        //StartCoroutine(Step(path));

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

            var now = Time.realtimeSinceStartup;
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

            var path = SearchRoad(mapInfoData, 0, 0, TargetX, TargetY, Diameter, IsJumpPoint);
            Debug.Log("Time1:" + (Time.realtimeSinceStartup - now));
            init(mapInfoData);
            StartCoroutine(Step(path));
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
        pathPointList.All((item) => { Destroy(item);
                                     return item;
        });
        pathPointList.Clear();
        var pathArray = path.ToArray();
        Array.Reverse(pathArray);
        foreach (var node in pathArray)
        {
            for (var x = 0; x < Diameter; x++)
            {
                for (var y = 0; y < Diameter; y++)
                {
                    var newSphere = Instantiate(PathPoint);
                    newSphere.transform.position = Utils.NumToPosition(LoadMap.transform.position, new Vector2(node.X, node.Y), UnitWidth, MapWidth, MapHeight)
                        + new Vector3(x * UnitWidth, 0, y * UnitWidth);
                    pathPointList.Add(newSphere);
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

    // ----------------------------AStar------------------------------

    /// <summary>
    /// 结束节点
    /// 是否存在路径
    /// </summary>
    private Node endNode = null;

    /// <summary>
    /// 寻找路径
    /// </summary>
    /// <param name="map">地图数组</param>
    /// <param name="startX">起始点X</param>
    /// <param name="startY">起始点Y</param>
    /// <param name="endX">目标点X</param>
    /// <param name="endY">目标点Y</param>
    /// <param name="diameter">物体半径</param>
    /// <param name="isJumpPoint">是否为跳跃式点, 如果为true 则路径只会给出拐点处的关键点</param>
    IList<Node> SearchRoad(int[][] map, int startX, int startY, int endX, int endY, int diameter, bool isJumpPoint = false, Action completeCallback = null)
    {
        //Debug.Log(map.Length+":" + map[0].Length);
        Debug.Log(string.Format("Path Form {0},{1} to {2},{3}", startX, startY, endX, endY));
        Queue<Node> openList = new Queue<Node>();
        IList<Node> closeList = new List<Node>();
        // 初始化开始节点
        openList.Enqueue(new Node(startX, startY));

        // 计算结束偏移
        endX = endX - diameter;
        endY = endY - diameter;
        var counter = 0;
        do
        {
            counter++;
            // 获取当前所在节点
            var currentPoint = openList.Dequeue();
            // 将当前节点放入关闭列表
            closeList.Add(currentPoint);
            // 获取当前节点周围的节点
            var surroundPointArray = SurroundPoint(currentPoint);
            foreach (var tmpPoint in surroundPointArray)
            {
                // 是否可以通过
                // 判断位置
                // 判断是否障碍
                // 斜向是否可移动
                // 判断周围节点合理性
                if (IsPassable(map, tmpPoint, currentPoint, diameter) && ExistInList(tmpPoint, closeList) == null)
                {
                    // 计算G值 上下左右为10, 四角为14
                    var g = currentPoint.G + ((currentPoint.X - tmpPoint.X)*(currentPoint.Y - tmpPoint.Y)) == 0
                        ? 10
                        : 14;
                    // 该点是否在开启列表中
                    if (ExistInList(tmpPoint, openList.ToArray()) == null)
                    {
                        // 计算H值, 通过水平和垂直距离确定
                        tmpPoint.H = Math.Abs(endX - tmpPoint.X)*10 + Math.Abs(endY - tmpPoint.Y)*10;
                        tmpPoint.G = g;
                        tmpPoint.F = tmpPoint.H + tmpPoint.G;
                        tmpPoint.Parent = currentPoint;
                        openList.Enqueue(tmpPoint);
                    }
                    else // 存在于开启列表, 比较当前的G值与之前的G值大小
                    {
                        var node = ExistInList(tmpPoint, openList.ToArray());
                        if (g < node.G)
                        {
                            node.Parent = currentPoint;
                            node.G = g;
                            node.F = g + node.H;
                        }
                    }
                }
            }
            

            // 如果开放列表为空, 则没有通路
            if (openList.Count == 0)
            {
                break;
            }

            // 重新排列, 将F值最小的放在最先取出的位置
            var tmpList = openList.ToArray();
            Array.Sort(tmpList, (a, b) => a.F - b.F);
            openList = new Queue<Node>(tmpList);

            // 如果搜索次数大于(w+h) * 4 则停止搜索
            //if (counter > (map.Length + map[0].Length) * 4)
            //{
            //    openList.Clear();
            //    break;
            //}

        } while ((endNode = ExistInList(new Node(endX, endY), openList.ToArray())) == null);

        Debug.Log(counter);
        IList<Node> path = new List<Node>();
        // 如果有可行路径
        if (endNode != null)
        {
            // 将路径回退并放入列表
            var currentNode = endNode;
            do
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            } while (currentNode.X != startX || currentNode.Y != startY);
        }

        // 处理跳点路径
        if (isJumpPoint && path.Count > 0)
        {
            // 如果路径方向发生变化则变化的奇点为拐点放入列表
            var jumpPointPath = new List<Node>();
            var startPoint = path[0];
            jumpPointPath.Add(startPoint);
            var xOff = startPoint.X;
            var yOff = startPoint.Y;
            foreach (var pathPoint in path)
            {
                // 不是第一个元素
                if (pathPoint.Parent == null)
                {
                    continue;
                }

                // x,y差值是否与上一次相同, 不相同则保存拐点
                var nowXOff = pathPoint.X - pathPoint.Parent.X;
                var nowYOff = pathPoint.Y - pathPoint.Parent.Y;
                if (nowXOff != xOff || nowYOff != yOff)
                {
                    xOff = nowXOff;
                    yOff = nowYOff;
                    jumpPointPath.Add(pathPoint);
                }
            }
            path.Clear();
            path = jumpPointPath;
            Debug.Log(jumpPointPath.Count);
        }

        if (completeCallback != null)
        {
            completeCallback();
        }

        // 返回路径, 如果路径数量为0 则没有可行路径
        return path;
    }

    /// <summary>
    /// 当前位置是否可以通过
    /// </summary>
    /// <param name="computeMap">地图</param>
    /// <param name="nowNode">当前位置</param>
    /// <param name="prvNode">父节点</param>
    /// <param name="diameter">移动物体直径</param>
    /// <returns></returns>
    private bool IsPassable(int[][] computeMap, Node nowNode, Node prvNode, int diameter)
    {
        // 定义 物体位置为左上角(主要指直径大于1的)
        // 验证参数是否合法
        if (diameter <= 0 || computeMap == null || nowNode == null || prvNode == null)
        {
            return false;
        }
        // TODO 优化方案 差值判断不同区域, 重复区域忽略
        // 遍历直径内的点
        for (var i = 0; i < diameter; i++)
        {
            for (var j = 0; j < diameter; j++)
            {
                var tmpNode = new Node(nowNode.X + i, nowNode.Y + j);

                try
                {
                    // 判断点的位置是否合法
                    if (tmpNode.X < 0 ||
                        tmpNode.Y < 0 ||
                        tmpNode.X >= computeMap[0].Length ||
                        tmpNode.Y >= computeMap.Length ||
                        computeMap[tmpNode.Y][tmpNode.X] == Utils.Obstacle)
                    {
                        return false;
                    }
                }
                catch
                {
                    int debug = 0;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 获取当前点周围的点
    /// </summary>
    /// <param name="curPoint">当前点</param>
    /// <returns>周围节点的数组</returns>
    Node[] SurroundPoint(Node curPoint)
    {
        var x = curPoint.X;
        var y = curPoint.Y;
        return new[]
        {
            new Node(x - 1, y - 1), 
            new Node(x, y - 1), 
            new Node(x + 1, y - 1), 
            new Node(x + 1, y), 
            new Node(x + 1, y + 1), 
            new Node(x, y + 1), 
            new Node(x - 1, y + 1), 
            new Node(x - 1, y)
        };
    }


    /// <summary>
    /// 在列表中查找节点
    /// 如果列表中存在该节点返回true 否则返回false
    /// </summary>
    /// <param name="node">被查找节点</param>
    /// <param name="nodeList">查找列表</param>
    /// <returns></returns>
    Node ExistInList(Node node, IList<Node> nodeList)
    {
        foreach (var tmpNode in nodeList)
        {
            if (node.X == tmpNode.X && node.Y == tmpNode.Y)
            {
                return tmpNode;
            }
        }
        return null;
    }




    /// <summary>
    /// 地图节点
    /// </summary>
    class Node
    {
        public Node(int x, int y, int g = 0, int h = 0)
        {
            X = x;
            Y = y;
            G = g;
            H = h;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F { get; set; }
        public Node Parent { get; set; }

    }

    // ----------------------------AStar------------------------------
}