using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : Pipe
{
    [SerializeField] public VirusBase SpecifiedInput { get; set; }

    public override LayeredVirus GetOutput() {
        if (output != null) return output;
        output = input;
        return output;
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
}
