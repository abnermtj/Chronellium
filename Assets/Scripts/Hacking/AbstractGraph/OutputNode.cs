using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OutputNode : Pipe
{
    [SerializeField] private VirusBase target;
    // Consequences of destroying the node registers their listener here
    public UnityEvent onDestroyed = new UnityEvent();

    public OutputNode(VirusBase target) {
        this.target = target;
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }

    public override LayeredVirus GetOutput() {
        if (output != null) return output;
        output = ProcessInput(input);
        return output;
    }

    public LayeredVirus ProcessInput(LayeredVirus output) {
        if (target == output.PeekLayer()) {
            onDestroyed?.Invoke();
            Debug.Log($"{target} matched output node destroyed");
            output.Peel(1);
        }
  
        return output;
    }
}
