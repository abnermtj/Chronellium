using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// These output nodes are attached directly on the layered virus's path. 
// If outermost layer of the virus does not match the target, no unwrap will occur.
// Std config, start point at (-0.5, 0) end point at (0.5, 1), process point at (0, 0)
public class OptionalOutputNodeView : GeneralOutputView
{
    [SerializeField] private Vector3 unitEndPoint = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitStartPoint = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitIntersectPoint = Vector3.zero;
    [SerializeField] private bool providesMainInput = true;

    void Awake() {
        outputNode = new OutputNode(target);
    }
    
    protected override IEnumerator MoveStream(GameObject content) {
        float startPointSpeed = upstream.GetStreamSpeed() / 2;
        float timeElapsed = 0;
        float firstHalfAvgSpeed = startPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / firstHalfAvgSpeed;

        // From startpoint to midpoint
        while (timeElapsed < timeFrame) {
            float lerpedSpeed = startPointSpeed - startPointSpeed * (timeElapsed / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.deltaTime;
            timeElapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        ProcessContent();

        timeElapsed = 0;
        float endPointSpeed = downstream.GetStreamSpeed() / 2;
        float secondHalfAvgSpeed = endPointSpeed / 2;
        timeFrame = transform.localScale.x / 2 / secondHalfAvgSpeed;

        // From midpoint to endpoint
        while (timeElapsed < timeFrame) {
            float lerpedSpeed = endPointSpeed * (timeElapsed / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.deltaTime;
            timeElapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (providesMainInput) {
            downstream.CallMoveStream(content, this);
        } else {
            if (!(downstream is MergeIntersectorView)) {
                Debug.LogError("providesMainInput can only be false when attached to merge intersector");
            } else {
                ((MergeIntersectorView)downstream).CallMoveSidestream(content, this);
            }
        }
    }

    void ProcessContent() {
        outputNode.DetermineOutput();
    }
}
