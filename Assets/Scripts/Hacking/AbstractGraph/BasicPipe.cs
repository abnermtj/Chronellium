public class BasicPipe : Pipe
{
    public override void SetInput() {
        input = ParentPipe.GetOutput();
    }
    
    public override LayeredVirus GetOutput() {
        if (output != null) return output;
        output = input;
        return output;
    }
    
    public void SetSpecialInput(LayeredVirus specifiedInput) {
        input = specifiedInput;
    }
}
