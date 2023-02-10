using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GeneralOutputView : PipeView
{
    [SerializeField] protected VirusBase target;
    protected OutputNode outputNode;

    protected override void AbsorbFromUpstream() {
        outputNode.SetInput();
    }

    public override Pipe GetPipe() { return outputNode; }

    public override float GetStreamSpeed() { return 0; }
}
