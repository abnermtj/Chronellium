// MergeIntersectors have two main inputs and one output
public class MergeIntersector : Pipe
{
    public Pipe StepParentPipe { get; set; }
    private LayeredVirus stepInput;

    public override LayeredVirus GetOutput() {
        if (output != null) return output;
        output = input.WrapWith(stepInput);
        return output;
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
        stepInput = StepParentPipe.GetOutput();
    }
}
