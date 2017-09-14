using UnityEngine;
using TongFramework.IA.NeuralNets.Trainers;
using TongFramework.Unity.Common;

public class MonoNeuralTrainCase : MonoBehaviour
{
    public NeuralNetTrainCase trainCase = new NeuralNetTrainCase();

    public MonoNeuralTrainer trainer;

    public float[] ins;
    public float[] outs;

    public void Awake()
    {
        trainer = this.GetNeededComponent<MonoNeuralTrainer>(trainer, "No trainer associated");
        trainCase.ins = ins;
        trainCase.outs = outs;
        trainer.neuralTrainer.AddTrainCase(trainCase);
        Destroy(this);
    }
}