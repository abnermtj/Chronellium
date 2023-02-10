using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : Pipe
{
    [SerializeField] public VirusBase SpecifiedInput { get; set; }

    public override void DetermineOutput() {
        output = input;
    }

    public override void SetInput() {
        input = new LayeredVirus(SpecifiedInput);
        // Debug.Log($"Specified input in input node is {input.PeekLayer()}");
        ParentPipe = null;
    }

    public void CreateInput(VirusBase baseVirus) {
        SpecifiedInput = baseVirus;
        SetInput();
    }

    public void Clear() {
        SpecifiedInput = null;
        input = null;
        output = null;
    }
}
