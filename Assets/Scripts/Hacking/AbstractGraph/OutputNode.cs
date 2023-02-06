using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OutputNode : Pipe
{
    [SerializeField] private VirusBase target;
    // Consequences of destroying the node registers their listener here
    public UnityEvent onDestroyed;

    public override void SetInput() {
        input = parentPipe.GetOutput();
    }

    public override LayeredVirus GetOutput() {
        return ProcessOutput(input);
    }

    public LayeredVirus ProcessOutput(LayeredVirus output) {
        if (target == output.PeekLayer()) {
            onDestroyed?.Invoke();
            output.PopLayer();
        }

        return output;
    }
}
