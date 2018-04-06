using System;
using UnityEngine;
using System.Collections;
using System.Text;
using TongFramework.Unity.Common;
using TongFramework.Math;

public class CarMovement2 : MonoBehaviour
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

    public void Awake()
    {
		//Initializes the arrays where we gonna stores the input and output values of the neural net
        _ins = new float[2];
        _outs = new float[1];
        
        //Stores the initial position to relocate when collides with a wall
        startPos = transform.position;
        startRot = transform.rotation;
    }


    public void OnGUI()
    {
        GUI.color = Color.red;
        // TODO 绘制神经网络的结构
        GUI.Label(new Rect(100, 100, 500, 800), GetNeuronNetStructureStr());
    }

    public void Update()
    {
    	//Create a raycast and returns the distance between the origin and the collide point
        _ins[0] = MonoUtils.GetDistance(leftSensor.position, leftSensor.forward);
        _ins[1] = MonoUtils.GetDistance(rightSensor.position, rightSensor.forward);
		
		//Returns the percentage of the sensor between the minimalDistance and the maximalDistance, 
		//if less than the minimal returns 0, it greater that the maximal returns
        _ins[0] = MathUtils.Range(minimalDistance, maximalDistance, _ins[0]);
        _ins[1] = MathUtils.Range(minimalDistance, maximalDistance, _ins[1]);
        
        //If the sensors reaches 0, that means that the neural net doesnt return the correct value, 
        //so we need to train in every hit so the next time wont fail, also relocates to the initial 
        //position and stops the current update to prevent undesired behaviours
        if(_ins[0] == 0)
        {
        	for (int i = 0; i < trainLoops; i++) 
        		neuralNet.Train (_ins, new float[]{ 1f });
        		
			transform.position = startPos;
        	transform.rotation = startRot;
        	return;
        }
		else if (_ins [1] == 0) 
		{
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
        if (val < 0.3f) 
        {
			transform.Rotate (0, (1 - MathUtils.Range (0, 0.2f, val)) * -100 * Time.deltaTime, 0);
		} 
		else if (val > 0.7f) 
		{
			transform.Rotate (0, MathUtils.Range (0.8f, 1, val) * 100 * Time.deltaTime, 0);
		}
		
		/* USE THIS IF YOU WANT A NON SMOOTH TURN
        
        if (val < 0.2f)
            transform.Rotate(0, -100 * Time.deltaTime, 0);
        else if (val > 0.8f)
            transform.Rotate(0, 100 * Time.deltaTime, 0);
        
        */
				
		//Constant move forward
		transform.Translate(0,0,0.2f);
    }

    /// <summary>
    /// 获取神经网络结构字符串
    /// </summary>
    /// <returns></returns>
    private string GetNeuronNetStructureStr()
    {
        StringBuilder result = new StringBuilder();

        result.Append("In:" + _ins[0] + ":" + _ins[1]);
        result.Append("-----------------------\n");
        // 遍历input节点
        foreach (var inItem in neuralNet.neuralNet.inLayer.neurons)
        {
            
            foreach (var axon in inItem.axons)
            {
                result.Append(inItem.Value);
                result.Append("\t:\t");
                result.Append(axon.weight);
                result.Append(",\n");
            }
        }
        result.Append("\n");

        // 遍历hide节点
        foreach (var hiddenLayer in neuralNet.neuralNet.hiddenLayers)
        {
            foreach (var hiddenItem in hiddenLayer.neurons)
            {
                foreach (var axon in hiddenItem.axons)
                {
                    result.Append(hiddenItem.Value);
                    result.Append("\t:\t");
                    result.Append(axon.weight);
                    result.Append(",\n");
                }
            }
        }
        result.Append("\n");

        // 遍历out节点
        foreach (var outItem in neuralNet.neuralNet.outLayer.neurons)
        {
            foreach (var axon in outItem.axons)
            {
                result.Append(outItem.Value);
                result.Append("\t:\t");
                result.Append(axon.weight);
                result.Append(",\n");
            }
        }

        result.Append("\n-----------------------");
        result.Append("out:" + _outs[0]);
        return result.ToString();
    }
}
