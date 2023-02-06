using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Prefabs of pipe should always have length 1 and its pivot centered, one collider at the end, one collider at the start
// A Pipe's end point can be an InputNode, OutputNode or Intersector
public abstract class Pipe : MonoBehaviour
{
    protected Pipe parentPipe;
    protected Pipe childPipe;
    protected LayeredVirus input;
    protected LayeredVirus output;

    public abstract LayeredVirus GetOutput();
    public abstract void SetInput();
}
