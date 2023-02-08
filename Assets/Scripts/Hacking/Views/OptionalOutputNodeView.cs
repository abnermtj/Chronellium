using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These output nodes are attached directly on the layered virus's path. 
// If outermost layer of the virus does not match the target, no unwrap will occur.
// Std config, start point at (-0.5, 0) end point at (0.5, 1), process point at (0, 0)
public class OptionalOutputNodeView : PipeView
{
    [SerializeField] private Vector3 unitEndPoint = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitStartPoint = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitIntersectPoint = Vector3.zero;
    [SerializeField] private VirusBase target;
    [SerializeField] private bool providesMainInput = true;
    private OutputNode outputNode = new OutputNode();

    void Awake() {
        outputNode.ParentPipe = upstream.GetPipe();
    }

    public override float GetStreamSpeed() { return 0; }

    protected override IEnumerator MoveStream(GameObject content) {
        float startPointSpeed = upstream.GetStreamSpeed() / 2;
        float timeElaped = 0;
        float firstHalfAvgSpeed = startPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / firstHalfAvgSpeed;

        // From startpoint to midpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = startPointSpeed - startPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        ProcessContent();

        timeElaped = 0;
        float endPointSpeed = downstream.GetStreamSpeed() / 2;
        float secondHalfAvgSpeed = endPointSpeed / 2;
        timeFrame = transform.localScale.x / 2 / secondHalfAvgSpeed;

        // From midpoint to endpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = endPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (providesMainInput) {
            downstream.CallMoveStream(content);
        } else {
            if (!(downstream is MergeIntersectorView)) {
                Debug.LogError("providesMainInput can only be false when attached to merge intersector");
            } else {
                ((MergeIntersectorView)downstream).CallMoveSidestream(content);
            }
        }
    }

    protected override void AbsorbFromUpstream() {
        outputNode.SetInput();
    }
    public override Pipe GetPipe() { return outputNode; }

    void ProcessContent() {
        outputNode.GetOutput();
    }
}
