using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PurePipeView : PipeView
{
    // Whether the pipe is main branch or side branch when attached to merge intersector
    [SerializeField][Tooltip("Differentiate input branch roles connected to merge intersector")] protected bool providesMainInput = true;
    [SerializeField][Tooltip("Differentiate output branch roles connected to split intersector")] protected bool absorbsMainOutput = true;
    protected BasicPipe basicPipe = new BasicPipe();
    [SerializeField] protected float streamSpeed;

    protected override void AbsorbFromUpstream() {
        if (absorbsMainOutput) {
            basicPipe.SetInput();
            // Since no visual processing is done DetermineOutput() can be called here
            basicPipe.DetermineOutput();
        } else {
            if (upstream.GetComponent<SplitIntersectorView>() == null) {
                Debug.LogError("@field providesMainOutput can only be marked false when connected to split intersector");
            } else {
                basicPipe.SetSpecialInput(upstream.GetComponent<SplitIntersectorView>().SplittedVirus);
                basicPipe.DetermineOutput();
            }
        }
    }

    public override Pipe GetPipe() { return basicPipe; }

    public override float GetStreamSpeed() { return streamSpeed; }
}
