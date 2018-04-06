using System.Collections;
using System.Collections.Generic;

namespace TongFramework.IA.NeuralNets.Trainers
{
    public class NeuralNetTrainer
    {
        public int trainLoops = 1000;
        public NeuralNet neuralNet;
        public List<NeuralNetTrainCase> trainCases = new List<NeuralNetTrainCase>();

        public NeuralNetTrainer(NeuralNet pNeuralNet = null, int pTrainLoops = 1000)
        {
            neuralNet = pNeuralNet;
            trainLoops = pTrainLoops;
        }

        public void AddTrainCase(NeuralNetTrainCase pTrainCase)
        {
            trainCases.Add(pTrainCase);
        }

        public void Train(NeuralNet pNeuralNet = null)
        {
            NeuralNet trainNet = pNeuralNet ?? neuralNet;

            for (int i = 0; i < trainLoops; i++)
            {
                for (int j = 0; j < trainCases.Count; j++)
                {
                    trainCases[j].Train(trainNet);
                }
            }
        }
    }
}
