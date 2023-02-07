// MergeIntersectors have two main inputs and one output
public class MergeIntersector : Pipe
{
    public Pipe StepParentPipe { get; set; }
    private LayeredVirus stepInput;

    public override LayeredVirus GetOutput() {
        return input.WrapWith(stepInput);
    }

    public override void SetInput() {
        input = ParentPipe.GetOutput();
        stepInput = StepParentPipe.GetOutput();
    }
}
