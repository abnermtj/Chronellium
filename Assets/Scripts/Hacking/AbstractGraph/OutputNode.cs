using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OutputNode : Pipe
{
    [SerializeField] public VirusBase Target { get; set; }
    // Consequences of destroying the node registers their listener here
    public UnityEvent onDestroyed;

    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }

    public override LayeredVirus GetOutput() {
        return ProcessOutput(input);
    }

    public LayeredVirus ProcessOutput(LayeredVirus output) {
        if (Target == output.PeekLayer()) {
            onDestroyed?.Invoke();
            output.Peel(1);
        }
  
        return output;
    }
}
