using UnityEngine;
using System.Collections;
using TongFramework.IA.NeuralNets;
using TongFramework.Persistence;

public class MonoNeuralNet : MonoBehaviour, IGenericSerializable<Packet>
{
	public NeuralNet neuralNet;
	
	public int inNeurons;
	public int[] hiddenNeurons;
	public int outNeurons;
	
	protected virtual void Awake()
	{
		neuralNet = new NeuralNet(inNeurons, hiddenNeurons, outNeurons);
	}

	/// <summary>
	/// Train the neural net with the specified ins and outs.
	/// </summary>
	/// <param name='ins'> Inputs of the neural net </param>
	/// <param name='outs'> Outputs ot the neural net </param>
	public void Train(float[] ins, float[] outs)
	{
		neuralNet.Train(ins, outs);	
	}
	
	/// <summary>
	/// Calculate the return value of the nerual net
	/// </summary>
	/// <param name='ins'> Input of the neural net </param>
	/// <param name='outs'> Outputs of the neural net </param>
	public void Calculate(float[] ins, out float[] outs)
	{
		neuralNet.Calculate(ins, out outs);
	}

    public Packet serializeData
    {
        get
        {
            return neuralNet.serializeData;
        }
        set
        {
            neuralNet.serializeData = value;
        }
    }
}