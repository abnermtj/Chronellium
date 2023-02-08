public class BasicPipe : Pipe
{
    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }
    
    public override LayeredVirus GetOutput() {
        return input;
    }
    
    public void SetSpecialInput(LayeredVirus specifiedInput) {
        input = specifiedInput;
    }
}
