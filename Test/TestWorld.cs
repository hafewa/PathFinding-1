using System;
using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TestWorld : MonoBehaviour 
{
	public GameObject cubeObject;
	public GameObject pathObject;

	public Camera mainCamera;
	public SceneGrid sceneGrid;

	private AStarUtils aStarUtils;

	private AStarNode beginNode;

	public int cols = 100;
    public int rows = 100;

    public int unit = 10;
    
	private IList<GameObject> pathList;

    // 每帧寻路次数
    private int pathFindCount = 0;

    // 上次寻路计数
    private long lastPathFindCount = 0L;
    

	void Awake()
	{
        Application.targetFrameRate = 600;
		this.pathList = new List<GameObject> ();
		this.aStarUtils = new AStarUtils (this.cols, this.rows);
        Debug.Log("start");
		// cols
		for(int i = 0; i < this.cols; i++)
		{
			// rows
			for(int j = 0; j < this.rows; j++)
			{
				AStarUnit aStarUnit = new AStarUnit();

				if(i != 0 && j != 0 && Random.Range(1, 10) <= 1)
				{
					aStarUnit.isPassable = false;

					GameObject gameObject = (GameObject)Instantiate(cubeObject);
					if(gameObject != null)
					{
					    var x = i - this.cols * 0.5f + 0.5f;
					    var y = 0f;
                        var z = j - this.rows * 0.5f + 0.5f;
                        //Debug.Log(string.Format("x: {0}, y: {1}, z: {2}", x, y, z));
						gameObject.transform.localPosition = new Vector3(x, y, z);
					}

				}else{
					aStarUnit.isPassable = true;
				}

				this.aStarUtils.GetNode(i,j).AddUnit(aStarUnit);
			}
		}
        // 设置底板格子数量
        var mt = sceneGrid.GetComponent<Renderer>().material;
        Debug.Log(mt);
	    if (mt != null)
	    {
            //Debug.Log("setMaterial");
            mt.SetTextureScale("_MainTex", new Vector2(cols, rows));
	    }

        // 设置底板宽度
        sceneGrid.transform.localScale = new Vector3(cols / unit, 1 , rows / unit);
	}

	private void FindPath(int x, int y)
	{
        //Debug.Log(string.Format("FindPath-x:{0}, y:{1}", x, y));
        // 创建起始Node
		AStarNode endNode = this.aStarUtils.GetNode(x, y);

        // 设置起始点
		if (this.beginNode == null) 
		{
			this.beginNode = endNode;
			return;
		}

        // 清空历史路径
		if (this.pathList != null && this.pathList.Count > 0) 
		{
			foreach (GameObject xxObject in this.pathList) 
			{
				Destroy(xxObject);
			}
		}
		
        // 验证目标node的有效性
		if(endNode != null && endNode.walkable)
		{

			System.DateTime dateTime = System.DateTime.Now;

			IList<AStarNode> pathList = this.aStarUtils.FindPath(this.beginNode, endNode);

			System.DateTime currentTime = System.DateTime.Now;

			System.TimeSpan timeSpan = currentTime.Subtract(dateTime);

			//Debug.Log(timeSpan.Seconds + "秒" + timeSpan.Milliseconds + "毫秒");

			if(pathList != null && pathList.Count > 0)
			{
				foreach(AStarNode nodeItem in pathList)
				{
					GameObject gameObject = (GameObject)Instantiate(this.pathObject);
					this.pathList.Add(gameObject);
				    var localX =nodeItem.nodeX - this.cols * 0.5f + 0.5f;
                    var localY = 0f;
                    var localZ = nodeItem.nodeY - this.rows * 0.5f + 0.5f;
                    gameObject.transform.localPosition = new Vector3(localX, localY, localZ);
				}
			}
			this.beginNode = endNode;
		}
	}
	
	void Update()
	{
		if (Input.GetMouseButtonDown (0)) 
		{
            //Debug.Log("mouseButtonDown");
			Ray ray = this.mainCamera.ScreenPointToRay(Input.mousePosition);

			RaycastHit raycastHit = new RaycastHit();
			if(Physics.Raycast(ray, out raycastHit))
			{
                //Debug.Log("hitAnyThing");
				if(raycastHit.collider.gameObject.tag == "Plane")
				{
                    Vector3 pointItem = this.sceneGrid.transform.InverseTransformPoint(raycastHit.point) * this.cols/10f;
                    //Debug.Log(sceneGrid.transform.InverseTransformPoint(raycastHit.point));
                    Debug.Log(string.Format("1: x:{0}, z:{1}", pointItem.x, pointItem.z));
					pointItem.x = this.cols * 0.5f + Mathf.Ceil(pointItem.x) - 1f;
					pointItem.z = this.rows * 0.5f + Mathf.Ceil(pointItem.z) - 1f;
                    Debug.Log(string.Format("2: x:{0}, z:{1}", pointItem.x, pointItem.z));
					this.FindPath((int)pointItem.x, (int)pointItem.z);
				}
			}
		}
        // 自动寻路, 并计算时间
        // 生成图内随机点
	    var randomNodeX = 0;
	    var randomNodeY = 0;
        // 将endNode指向随机点
        // 验证随机点有效性
	    AStarNode targetNode = null;
        while(targetNode == null || !targetNode.walkable)
        {
            randomNodeX = Random.Range(0, cols);
            randomNodeY = Random.Range(0, rows);

            //Debug.Log(randomNodeX + "," + randomNodeY);
            targetNode = aStarUtils.GetNode(randomNodeX, randomNodeY);
        }
        // 开始寻路
        this.FindPath(randomNodeX, randomNodeY);
        // 寻路计数+1
	    pathFindCount++;
        
        // 计时
	    if (DateTime.Now.Ticks - lastPathFindCount >= 10000000)
        {
            Debug.Log(DateTime.Now.Ticks + "," + lastPathFindCount + "," + pathFindCount);
	        pathFindCount = 0;
            lastPathFindCount = DateTime.Now.Ticks;
        }
	}
}
