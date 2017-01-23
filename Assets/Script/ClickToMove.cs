using UnityEngine;
using System.Collections.Generic;

public class ClickToMove : MonoBehaviour
{

	private List<Vector3> path;
	public float speed = 0.2f;
	public enum MoveState
	{
		STOP,
		UP,
		DOWN,
		LEFT,
		RIGHT
	}

	public MoveState GetNowMoveState(Vector3 position,Vector3 targetPosition)
	{
		if (position.x == targetPosition.x && position.y < targetPosition.y)
			return MoveState.UP;
		else if (position.x == targetPosition.x && position.y > targetPosition.y)
			return MoveState.DOWN;
		else if (position.y == targetPosition.y && position.x > targetPosition.x)
			return MoveState.LEFT;
		else if (position.y == targetPosition.y && position.x < targetPosition.x)
			return MoveState.RIGHT;
		else
			return MoveState.STOP;
	}
	
	void Start ()
	{
		CreateMap ();

	}

	void Update ()
	{
		float step = speed * Time.deltaTime; 
		if (path.Count > 0) {
			transform.position = Vector3.MoveTowards(transform.position,path[0],step);
			if (transform.position == path [0])
				path.RemoveAt (0);
		}
	}

	void CreateMap ()
	{
		int[,] array = {
			{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			{ 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1},
			{ 1, 0, 0, 1, 1, 0, 0, 0, 1, 1, 0, 1},
			{ 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1},
			{ 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 1},
			{ 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1},
			{ 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1},
			{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
		};
		
		Maze maze = new Maze (array);
		Point start = new Point (2, 2);
		Point end = new Point (3, 9);
		path = maze.Get2DPath (start, end, false);
		for (int i = 0; i<path.Count-1; i++) {
			Debug.DrawLine (path [i], path [i + 1], Color.red, 120, true);
		}
	}

	//Message from Agent
	void OnDestinationReached ()
	{

		//do something here...
	}

	//Message from Agent
	void OnDestinationInvalid ()
	{

		//do something here...
	}
}
