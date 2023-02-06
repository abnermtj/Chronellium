// MergeIntersectors have two main inputs and one output
public class MergeIntersector : Pipe
{
    private Pipe stepParentPipe;
    private LayeredVirus stepInput;

    public override LayeredVirus GetOutput() {
        return input.WrapWith(stepInput);
    }

    public override void SetInput() {
        input = parentPipe.GetOutput();
        stepInput = stepParentPipe.GetOutput();
    }
}
