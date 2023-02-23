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

    // NOTE: Similar construct for all nodes to prevent the method from being called more than once. e.g. ProcessContent with output node and from the child pipe SetInput
    public override void DetermineOutput() {
        output = ProcessInput(input);
        Debug.Log($"{output} after processing");
    }

    public LayeredVirus ProcessInput(LayeredVirus output) {
        if (target == output.PeekLayer()) {
            onDestroyed?.Invoke();
            Debug.Log($"{target} matched output node destroyed");
            output.Peel(1);
            HackGameManager.instance.CheckGameEnded(1);
        }
  
        return output;
    }
}
