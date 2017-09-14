using System;
using System.Collections.Generic;
using TongFramework.Persistence;

namespace TongFramework.IA.NeuralNets
{
	public class Neuron : IGenericSerializable<Packet>
	{
		private static Random _rnd = new Random ();
		public static float LearningRate = 0.2f;
	
		public List<NeuronConnection> dentrites = new List<NeuronConnection> ();
		public List<NeuronConnection> axons = new List<NeuronConnection> ();
		
		public void ConnectAxonWith (Neuron anotherNeuron)
		{
			float weight = (float)(_rnd.NextDouble () - _rnd.NextDouble ());
			axons.Add (new NeuronConnection { neuron = anotherNeuron, weight = weight});
			anotherNeuron.dentrites.Add (new NeuronConnection { neuron = this, weight = weight});
		}
		
		public float InValue;
		public float DesiredValue;
		public float Error;
		public float Value;
		
		/// <summary>
		/// Calculates and returns the value of the neuron.
		/// </summary>
		/// <returns>
		/// The value.
		/// </returns>
		public float CalculateValue ()
		{
			//If im from the input layer
			if (dentrites.Count == 0)
				return InValue;
			
			//If im not from the input layer
			float val = 0;
			for (int i = 0; i < dentrites.Count; i++)
			{
				val += dentrites [i].neuron.Value * dentrites [i].weight;
			}

			return (float)(1f / (1f + System.Math.Pow (System.Math.E, -val)));
		}
		
		/// <summary>
		/// Calculates and returns the error of the neuron.
		/// </summary>
		/// <returns>
		/// The error.
		/// </returns>
		public float CalculateError ()
		{
			//If im from input layer
			if (dentrites.Count == 0)
				return 0;
			
			//If im from output layer
			if (axons.Count == 0)
				return (DesiredValue - Value) * (Value * (1 - Value));
			
			//If im from the hidden layer
			float val = 0;
			
			for (int i = 0; i < axons.Count; i++)
			{
				val = axons [i].neuron.Error * axons [i].weight;			
			}
			
			return val * (Value * (1 - Value));
		}
		
		/// <summary>
		/// Calculates and stores the error of the neuron.
		/// </summary>
		public void UpdateError ()
		{
			Error = CalculateError ();
		}
		
		/// <summary>
		/// Calculates and stores the value of the neuron.
		/// </summary>
		public void UpdateValue ()
		{
			Value = CalculateValue ();
		}
		
		/// <summary>
		/// Calculates and stores the weight of the neuron.
		/// </summary>
		public void UpdateWeight ()
		{
			for (int i = 0; i < dentrites.Count; i++)
			{
				dentrites [i].weight += dentrites [i].neuron.Value * Error * LearningRate;			
			}
		}

		public Packet serializeData
		{
			get
			{
				Packet packet = new Packet ();

				List<float> weights = new List<float> ();

				foreach (NeuronConnection dentrite in dentrites)
				{
					weights.Add (dentrite.weight);
				}

				packet.Add (weights.ToArray ());

				return packet;
			}
			set
			{
				float[] weights = value.GetArray<float> ();
                
				for (int i = 0; i < dentrites.Count; i++)
				{
					dentrites [i].weight = weights [i];
				}
			}
		}
	}
	
	public class NeuronConnection
	{
		public Neuron neuron;
		public float weight;
	}
}

