using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NavMash寻路管理器
/// </summary>
public class NavMeshManager : MonoBehaviour
{
    /// <summary>
    /// 寻路对象列表
    /// </summary>
    public IList<NavMeshObj> NavMeshObjList = new List<NavMeshObj>();

    /// <summary>
    /// 起始创建寻路对象数量
    /// </summary>
    public int NavMeshObjCount = 1;

    /// <summary>
    /// 源对象
    /// </summary>
    public NavMeshObj SourceObj;

    /// <summary>
    /// 寻路平台大小
    /// </summary>
    public Vector2 WAndH;

    /// <summary>
    /// 移动目标点列表
    /// </summary>
    //public IList<NavMeshTarget> NavMeshTargetList = new List<NavMeshTarget>();

    /// <summary>
    /// 增加目标点对象
    /// </summary>
    /// <param name="target">目标点</param>
//    public void AddTarget(NavMeshTarget target)
//    {
//        if (target != null)
//        {
//            //NavMeshTargetList.Add(target);
//        }
//        else
//        {
//            Debug.Log("无效目标点");
//        }
//    }

    void Start()
    {
        // 创建指定数量的NavMeshObj 并随机位置
//        for (int i = 0; i < NavMeshObjCount; i++)
//        {
//            Debug.Log("123");
//            var obj = Instantiate(SourceObj);
//            obj.Target = new NavMeshTarget();
//            obj.Target.TargetPoint = new Vector3(Random.Range(0, WAndH.x), 0, Random.Range(0, WAndH.y));
//            obj.FindPath();
//        }
        //Destroy(SourceObj);
    }

    void Update()
    {
        // 为每一个NavMeshObj指定随机目标点
        // 直到其走到终点再次为其随机生成目标点
        if (Input.GetMouseButtonDown(0))
        {
            //摄像机到点击位置的的射线  
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //判断点击的是否地形  
                if (!hit.collider.name.Equals("Terrain"))
                {
                    return;
                }
                //点击位置坐标  
                Vector3 point = hit.point;
                //转向  
                transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
                Debug.Log("x,y:" + point);
                //设置寻路的目标点  
                SourceObj.GetComponent<NavMeshAgent>().SetDestination(point);
            }
        }  
  
    }






}
