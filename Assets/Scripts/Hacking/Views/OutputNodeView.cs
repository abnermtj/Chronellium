using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputNodeView : PipeView
{
    [SerializeField] private VirusBase target;
    private OutputNode outputNode;

    void Awake() {
        outputNode = new OutputNode(target);
        outputNode.ParentPipe = upstream.GetPipe();
        downstream = null;
    }

    public override float GetStreamSpeed() { return 0; }

    protected override IEnumerator MoveStream(GameObject content) {
        // Any animation logic here
        yield return null;
        EndStream(content);
    }

    protected override void AbsorbFromUpstream() {
        outputNode.SetInput();
    }
    public override Pipe GetPipe() { return outputNode; }

    void EndStream(GameObject content) {
        outputNode.GetOutput();
        Destroy(content);
    }
}
