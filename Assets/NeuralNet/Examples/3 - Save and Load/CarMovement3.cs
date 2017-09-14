using UnityEngine;
using System.Collections;
using TongFramework.Unity.Common;
using TongFramework.Math;
using TongFramework.Persistence;

public class CarMovement3 : MonoBehaviour
{
    private float[] _ins;
    private float[] _outs;

    public MonoNeuralNet neuralNet;
    
    public float minimalDistance = 1;
    public float maximalDistance = 5;
    public Transform leftSensor;
    public Transform rightSensor;
    public int trainLoops;
    public Vector3 startPos;
    public Quaternion startRot;
    
    public string path = "Assets/NeuralNet/Examples/3 - Save and Load/neuralNet.bin";

    public void Awake()
    {
		//Initializes the arrays where we gonna stores the input and output values of the neural net
        _ins = new float[2];
        _outs = new float[1];
        
        //Stores the initial position to relocate when collides with a wall
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void Update()
    {
    	//Create a raycast and returns the distance between the origin and the collide point
        _ins[0] = MonoUtils.GetDistance(leftSensor.position, leftSensor.forward);
        _ins[1] = MonoUtils.GetDistance(rightSensor.position, rightSensor.forward);

        var left = _ins[0];
        var right = _ins[1];
		//Returns the percentage of the sensor between the minimalDistance and the maximalDistance, 
		//if less than the minimal returns 0, it greater that the maximal returns 1, the idea is give 
		//the neural net values between 0 and 1, is the only way it works
        _ins[0] = MathUtils.Range(minimalDistance, maximalDistance, _ins[0]);
        _ins[1] = MathUtils.Range(minimalDistance, maximalDistance, _ins[1]);
        
        //If the sensors reaches 0, that means that the neural net doesnt return the correct value, 
        //so we need to train in every hit so the next time wont fail, also relocates to the initial 
        //position and stops the current update to prevent undesired behaviours
        if(_ins[0] == 0)
        {
            Debug.Log(1);
        	for (int i = 0; i < trainLoops; i++) 
        		neuralNet.Train (_ins, new float[]{ 1f });
        		
			transform.position = startPos;
        	transform.rotation = startRot;
        	return;
        }
		else if (_ins [1] == 0)
        {
            Debug.Log(2);
			for (int i = 0; i < trainLoops; i++) 
				neuralNet.Train (_ins, new float[]{ 0f });
        		
			transform.position = startPos;
			transform.rotation = startRot;
			return;
		}
        
        
		//Check the neural net return value
        neuralNet.Calculate(_ins, out _outs);
		
		//Get the only value that in this case the neural net return
		//Check the MonoNeuralNet script in the gameobject IA in the 
		//gameObject car and see where it specifies one
        float val = _outs[0];
		
		//Turns base in the the direction that the neural net says
        if (val < 0.2f) 
        {
			transform.Rotate (0, (1 - MathUtils.Range (0, 0.2f, val)) * -100 * Time.deltaTime, 0);
		} 
		else if (val > 0.8f) 
		{
			transform.Rotate (0, MathUtils.Range (0.8f, 1, val) * 100 * Time.deltaTime, 0);
		}

        /* USE THIS IF YOU WANT A NON SMOOTH TURN
        
        if (val < 0.2f)
            transform.Rotate(0, -100 * Time.deltaTime, 0);
        else if (val > 0.8f)
            transform.Rotate(0, 100 * Time.deltaTime, 0);
        
        */
		
		//Constant move forwa		
		//Constant move forward
		transform.Translate(0,0,0.2f);
    }
    
    public void OnGUI()
    {
    	if(GUI.Button(new Rect(0,0,100,50), "Save"))
    	{	
			neuralNet.Persist (path);
		}
    	if (GUI.Button (new Rect (0, 50, 100, 50), "Load")) 
    	{
			neuralNet.Read(path);
			
			transform.position = startPos;
			transform.rotation = startRot;
		}
    }
}
