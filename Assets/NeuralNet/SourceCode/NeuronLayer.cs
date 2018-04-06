using System;
using System.Collections.Generic;
using TongFramework.Persistence;

namespace TongFramework.IA.NeuralNets
{
	public class NeuronLayer
	{
		public NeuronLayer previousLayer;
		public NeuronLayer nextLayer;
		public List<Neuron> neurons = new List<Neuron> ();
		
		/// <summary>
		/// Create neural layer
		/// </summary>
		/// <param name='quantity'>
		/// Quantity of neurons to create in layer
		/// </param>
		public NeuronLayer (int quantity)
		{
			for (int i = 0; i < quantity; i++)
			{
				neurons.Add (new Neuron ());
			}
		}
		public NeuronLayer ()
		{
		}
		
		/// <summary>
		/// Connects all the axons of the layers with another layer.
		/// </summary>
		/// <param name='pNextLayer'>
		/// Layer To Connect.
		/// </param>
		public void ConnectAxonsWith (NeuronLayer pNextLayer)
		{
			nextLayer = pNextLayer;
			nextLayer.previousLayer = this;
			
			for (int i = 0; i < neurons.Count; i++)
			{
				for (int j = 0; j < nextLayer.neurons.Count; j++)
				{
					neurons [i].ConnectAxonWith (nextLayer.neurons [j]);
				}			
			}
		}
		
		/// <summary>
		/// Calculates and stores the out values of the neurons.
		/// </summary>
		public void CalculateValues ()
		{
			for (int i = 0; i < neurons.Count; i++)
			{
				neurons [i].UpdateValue ();			
			}
		}
		
		/// <summary>
		/// Calculates and stores the error of the neurons.
		/// </summary>
		public void CalculateErrors ()
		{
			for (int i = 0; i < neurons.Count; i++)
			{
				neurons [i].UpdateError ();			
			}
		}
		
		/// <summary>
		/// Calculates and stores the weights of the neurons.
		/// </summary>
		public void CalculateWeights ()
		{
			for (int i = 0; i < neurons.Count; i++)
			{
				neurons [i].UpdateWeight ();			
			}
		}
	}
}

