using UnityEngine;
using System.Collections;

public class DragonController : MonoBehaviour {

	private Animator animator;
	private AnimatorStateInfo currentStateInfo;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator> ();
		if (animator) {
			animator.SetBool("Fly",false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (animator) {
			currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			if(currentStateInfo.IsName("Base Layer.Idle"))
			{
				if(Input.GetButton("Fire1"))
				{
					animator.SetBool("Fly",true);
				}
			}
			else
			{
				animator.SetBool("Fly",false);
			}
		}
	}
}
