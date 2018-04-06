using System;
using TongFramework.Persistence;
using System.Collections.Generic;
using System.Diagnostics;

namespace TongFramework.IA.NeuralNets
{
	public class NeuralNet : IGenericSerializable<Packet>
	{
		public NeuronLayer inLayer;
		public NeuronLayer[] hiddenLayers;
		public NeuronLayer outLayer;
		
		/// <summary>
		/// Creates a neural net
		/// </summary>
		/// <param name='inLayerNeuronCount'> In layer neuron count. </param>
		/// <param name='hiddenLayerNeuronCounts'> Array containing the hidden layers neuron count. </param>
		/// <param name='outLayerNeuronCount'> Out layer neuron count.</param>
		/// <exception cref='Exception'> Throws an exception if the hidden layers are 0. </exception>
		public NeuralNet (int inLayerNeuronCount, int[] hiddenLayerNeuronCounts, int outLayerNeuronCount)
		{
			if (hiddenLayerNeuronCounts.Length == 0) 
				throw new Exception ("The neural net must have at least one hidden layer");
		
			inLayer = new NeuronLayer (inLayerNeuronCount);
			hiddenLayers = new NeuronLayer[hiddenLayerNeuronCounts.Length];
			outLayer = new NeuronLayer (outLayerNeuronCount);
		    UnityEngine.Debug.Log(hiddenLayerNeuronCounts.Length);
			for (int i = 0; i < hiddenLayerNeuronCounts.Length; i++)
			{
				hiddenLayers [i] = new NeuronLayer (hiddenLayerNeuronCounts [i]);
			}
			
			inLayer.ConnectAxonsWith (hiddenLayers [0]);
			
			for (int i = 0; i < hiddenLayers.Length - 1; i++)
			{
				hiddenLayers [i].ConnectAxonsWith (hiddenLayers [i + 1]);
			}
			
			
			hiddenLayers [hiddenLayers.Length - 1].ConnectAxonsWith (outLayer);
		}
		
		/// <summary>
		/// Train the neural net with the specified ins and outs.
		/// </summary>
		/// <param name='ins'> Inputs of the neural net </param>
		/// <param name='outs'> Outputs ot the neural net </param>
		public void Train (float[] ins, float[] outs)
		{
			if (inLayer.neurons.Count != ins.Length ||
				outLayer.neurons.Count != outs.Length)
			{
				throw new Exception ("Wrong train input or outputs quantity");
			}

			for (int i = 0; i < inLayer.neurons.Count; i++)
			{
				inLayer.neurons [i].InValue = ins [i];			
			}
			
			for (int i = 0; i < outLayer.neurons.Count; i++)
			{
				outLayer.neurons [i].DesiredValue = outs [i];			
			}
			
			inLayer.CalculateValues ();
			for (int i = 0; i < hiddenLayers.Length; i++)
			{
				hiddenLayers [i].CalculateValues ();
			}
			outLayer.CalculateValues ();
			
			outLayer.CalculateErrors ();
			for (int i = hiddenLayers.Length - 1; i >= 0; i--)
			{
				hiddenLayers [i].CalculateErrors ();
			}
			inLayer.CalculateErrors ();
			
			outLayer.CalculateWeights ();
			for (int i = hiddenLayers.Length - 1; i >= 0; i--)
			{
				hiddenLayers [i].CalculateWeights ();
			}
			inLayer.CalculateWeights ();
		}
		
		/// <summary>
		/// Calculate the return value of the nerual net
		/// </summary>
		/// <param name='ins'> Input of the neural net </param>
		/// <param name='outs'> Outputs of the neural net </param>
		public void Calculate (float[] ins, out float[] outs)
		{
			for (int i = 0; i < inLayer.neurons.Count; i++)
			{
				inLayer.neurons [i].InValue = ins [i];			
			}
			
			inLayer.CalculateValues ();
			for (int i = 0; i < hiddenLayers.Length; i++)
			{
				hiddenLayers [i].CalculateValues ();
			}
			outLayer.CalculateValues ();
			
			outs = new float[outLayer.neurons.Count];
			
			for (int i = 0; i < outLayer.neurons.Count; i++)
			{
				outs [i] = outLayer.neurons [i].Value;			
			}
		}

		public Packet serializeData
		{
			get
			{
				int[] hiddenCount = new int[hiddenLayers.Length];
				for (int i = 0; i < hiddenCount.Length; i++)
				{
					hiddenCount [i] = hiddenLayers [i].neurons.Count;
				}
			
				Packet packet = new Packet ();

				packet.Add (inLayer.neurons.Count);
				packet.Add (hiddenCount);
				packet.Add (outLayer.neurons.Count);
				
				packet.Add (inLayer.neurons.ToArray ());
				foreach (NeuronLayer item in hiddenLayers)
				{
					packet.Add (item.neurons.ToArray ());
				}
				packet.Add (outLayer.neurons.ToArray ());

				return packet;
			}
			set
			{
				//Start Layers
				inLayer = new NeuronLayer (value.GetInt ());
				
				int[] quantities = value.GetArray<int> ();
				hiddenLayers = new NeuronLayer[quantities.Length];
				for (int i = 0; i < quantities.Length; i++)
					hiddenLayers [i] = new NeuronLayer (quantities [i]);
				
				outLayer = new NeuronLayer (value.GetInt ());
				
				//Connect Layers
				inLayer.ConnectAxonsWith (hiddenLayers [0]);
				for (int i = 0; i < hiddenLayers.Length - 1; i++)
					hiddenLayers [i].ConnectAxonsWith (hiddenLayers [i + 1]);
				hiddenLayers [hiddenLayers.Length - 1].ConnectAxonsWith (outLayer);
				
				//Start neurons
				Packet[] inNeuronsPackets = value.GetBytesArray<Packet> ();
				for (int i = 0; i < inNeuronsPackets.Length; i++)
				{
					inLayer.neurons [i].serializeData = inNeuronsPackets [i];
				}
				
				for (int j = 0; j < hiddenLayers.Length; j++)
				{
					Packet[] hidNeuronsPackets = value.GetBytesArray<Packet> ();
					for (int i = 0; i < hidNeuronsPackets.Length; i++)
					{
						hiddenLayers [j].neurons [i].serializeData = hidNeuronsPackets [i];
					}
				}
				
				Packet[] outNeuronsPackets = value.GetBytesArray<Packet> ();
				for (int i = 0; i < outNeuronsPackets.Length; i++)
				{
					outLayer.neurons [i].serializeData = outNeuronsPackets [i];
				}
			}
		}
	}
}

