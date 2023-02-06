using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputNode : Pipe
{
    [SerializeField] private VirusBase specifiedInput;

    public override LayeredVirus GetOutput() {
        return input;
    }

    public override void SetInput() {
        input = new LayeredVirus(specifiedInput);
        parentPipe = null;
    }
}
