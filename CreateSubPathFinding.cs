using UnityEngine;
using System.Collections;

public class CreateSubPathFinding : MonoBehaviour
{

    public GameObject SubPathFinding;
    public GameObject MainCamera;
    public int CopyCount = 0;
    public int SubPathFindingWight = 50;
	void Start () {
        // 原始子寻路是否存在
	    if (SubPathFinding != null)
	    {
            // 方阵阶数
            int sqCount = 1;
	        int count = 1;
            while (CopyCount > count)
	        {
                count = ++sqCount * sqCount;
	        }
	        if (MainCamera != null)
	        {
                MainCamera.transform.localPosition = new Vector3(
                    MainCamera.transform.localPosition.x,
                    MainCamera.transform.localPosition.y * sqCount, 
                    MainCamera.transform.localPosition.z);
	        }

            for (int i = 0; i < CopyCount; i++)
            {
                var copy = (GameObject)Object.Instantiate(SubPathFinding);
                copy.transform.localPosition = new Vector3(
                    (-sqCount / 2 + i % sqCount) * SubPathFindingWight, 
                    0,
                    (-sqCount / 2 + (i / sqCount) % sqCount) * SubPathFindingWight);
            }
	    }
	
	}
}
