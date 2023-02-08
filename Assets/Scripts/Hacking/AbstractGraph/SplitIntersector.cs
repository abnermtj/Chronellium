using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Split Intersector has one input and two outputs
// The step output branch must consist of only output nodes
public class SplitIntersector : Pipe
{
    private LayeredVirus stepOutput;
    private bool hasSplitted = false;
    [SerializeField] public int SplitCount { get; set; }
    [SerializeField] public Pipe StepChildPipe { get; set; }

    public override LayeredVirus GetOutput() {
        // Defensive
        if (!hasSplitted) GetStepOutput();
        return input;
    }
    
    public LayeredVirus GetStepOutput() {
        if (hasSplitted) return stepOutput;
        hasSplitted = true;
        stepOutput = input.Split(SplitCount);
        return stepOutput;
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }
}
