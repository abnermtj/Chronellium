using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Split Intersector has one input and two outputs
// The step output branch must consist of only output nodes
public class SplitIntersector : Pipe
{
    private LayeredVirus stepOutput;
    [SerializeField] public int SplitCount { get; set; }
    [SerializeField] public Pipe StepChildPipe { get; set; }

    public override void DetermineOutput() {
        DetermineStepOutput();
    }
    
    private void DetermineStepOutput() {
        stepOutput = input.Split(SplitCount);
        output = input;
        Debug.Log($"Step output is {stepOutput}");
    }

    public LayeredVirus GetStepOutput() {
        return stepOutput;
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
        Debug.Log($"Split intersector input {input}");
    }
}
