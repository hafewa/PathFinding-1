using System.Collections;

namespace TongFramework.IA.NeuralNets.Trainers
{
    public class NeuralNetTrainCase
    {
        public float[] ins;
        public float[] outs;

        public void Train(NeuralNet pNeuralNet)
        {
            pNeuralNet.Train(ins, outs);
        }
    }
}