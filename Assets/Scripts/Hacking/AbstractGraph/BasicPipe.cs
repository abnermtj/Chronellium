public class BasicPipe : Pipe
{
    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }
    
    public override void DetermineOutput() {
        output = input;
    }
    
    public void SetSpecialInput(LayeredVirus specifiedInput) {
        input = specifiedInput;
    }
}
