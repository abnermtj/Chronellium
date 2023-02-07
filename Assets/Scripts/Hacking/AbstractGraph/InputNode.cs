using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : Pipe
{
    [SerializeField] public VirusBase SpecifiedInput { get; set; }

    public override LayeredVirus GetOutput() {
        return input;
    }

    public override void SetInput() {
        input = new LayeredVirus(SpecifiedInput);
        ParentPipe = null;
    }

    public void CreateInput(VirusBase baseVirus) {
        SpecifiedInput = baseVirus;
        SetInput();
    }
}
