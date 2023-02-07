public class BasicPipe : Pipe
{
    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }
    
    public override LayeredVirus GetOutput() {
        return input;
    }
}
