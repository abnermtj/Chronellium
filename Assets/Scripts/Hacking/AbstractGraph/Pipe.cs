using UnityEngine;

// NOTE: Prefabs of pipe should always have length 1 and its pivot centered, one collider at the end, one collider at the start
// A Pipe's end point can be an InputNode, OutputNode or Intersector
public abstract class Pipe
{
    public Pipe ParentPipe { get; set; } 
    public Pipe ChildPipe { get; set; }
    protected LayeredVirus input;

    public abstract LayeredVirus GetOutput();
    public abstract void SetInput();
}
