using UnityEngine;
// MergeIntersectors have two main inputs and one output
public class MergeIntersector : Pipe
{
    public Pipe StepParentPipe { get; set; }
    private LayeredVirus stepInput;

    public override void DetermineOutput() {
        output = input.WrapWith(stepInput);
        Debug.Log($"Merge result of {input} and {stepInput} {output}");
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
        stepInput = StepParentPipe.GetOutput();
    }
}
