using UnityEngine;
using System.Collections.Generic;
using TongFramework.IA.NeuralNets.Trainers;
using TongFramework.Unity.Common;

public class MonoNeuralTrainer : MonoBehaviour
{
    public NeuralNetTrainer neuralTrainer = new NeuralNetTrainer();
    public MonoNeuralNet neuralNet;

    public int trainLoops = 1000;

    public void Start()
    {
        neuralNet = this.GetNeededComponent<MonoNeuralNet>(neuralNet, "No trainer associated");
        neuralTrainer.trainLoops = trainLoops;
        neuralTrainer.Train(neuralNet.neuralNet);
        Destroy(this);
    }
}