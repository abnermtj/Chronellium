using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PurePipeView : PipeView
{
    // Whether the pipe is main branch or side branch when attached to merge intersector
    [SerializeField] protected bool isMain = true;
    protected BasicPipe basicPipe = new BasicPipe();
    [SerializeField] protected float streamSpeed;

    protected override void AbsorbFromUpstream() {
        basicPipe.SetInput();
    }

    public override Pipe GetPipe() { return basicPipe; }

    public override float GetStreamSpeed() { return streamSpeed; }
}
