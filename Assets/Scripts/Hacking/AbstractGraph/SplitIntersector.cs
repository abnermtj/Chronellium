using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Split Intersector has one input and two outputs
// The step output branch must consist of only output nodes
public class SplitIntersector : Pipe
{
    private LayeredVirus stepOutput;
    private bool hasSplitted = false;
    [SerializeField] private int splitCount;
    [SerializeField] private Pipe stepChildPipe;

    public override LayeredVirus GetOutput() {
        // Defensive
        if (!hasSplitted) GetStepOutput();
        return input;
    }
    
    public LayeredVirus GetStepOutput() {
        hasSplitted = true;
        return input.PeelOff(splitCount);
    }

    public override void SetInput() {
        input = parentPipe.GetOutput();
    }
}
