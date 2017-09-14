using UnityEngine;
using System.Collections;
using TongFramework.Unity.Common;
using TongFramework.Math;

public class CarMovement1 : MonoBehaviour
{
    private float[] _ins;
    private float[] _outs;

    public MonoNeuralNet neuralNet;

    public Transform leftSensor;
    public Transform rightSensor;

    public void Awake()
    {
    	//Initializes the arrays where we gonna stores the input and output values of the neural net
        _ins = new float[2];
        _outs = new float[1];
    }

    public void Update()
    {
		//Create a raycast and returns the distance between the origin and the collide point
        _ins[0] = MonoUtils.GetDistance(leftSensor.position, leftSensor.forward);
        _ins[1] = MonoUtils.GetDistance(rightSensor.position, rightSensor.forward);

		//Returns the percentage of the sensor between the minimalDistance and the maximalDistance, 
		//if less than the minimal returns 0, it greater that the maximal returns 1, the idea is give 
		//the neural net values between 0 and 1, is the only way it works
        _ins[0] = MathUtils.Range(0, 5, _ins[0]);
        _ins[1] = MathUtils.Range(0, 5, _ins[1]);
		
		//Check the neural net return value
        neuralNet.Calculate(_ins, out _outs);

		//Get the only value that in this case the neural net return
		//Check the MonoNeuralNet script in the gameobject IA in the 
		//gameObject car and see where it specifies one
        float val = _outs[0];
		
		//Turns base in the the direction that the neural net says
        if (val < 0.2f)
            transform.Rotate(0, (1 - MathUtils.Range(0, 0.2f, val)) *  -100 * Time.deltaTime, 0);
        else if (val > 0.8f)
            transform.Rotate(0, MathUtils.Range(0.8f, 1, val) * 100 * Time.deltaTime, 0);
        
        /* USE THIS IF YOU WANT A NON SMOOTH TURN
        
        if (val < 0.2f)
            transform.Rotate(0, -100 * Time.deltaTime, 0);
        else if (val > 0.8f)
            transform.Rotate(0, 100 * Time.deltaTime, 0);
        
        */
        
		//Constant move forward
		transform.Translate(0,0,0.2f);
    }
}
