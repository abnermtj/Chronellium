using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Each pipe view is responsible for moving the layered virus
// Straight pipe prefab must be of unit length, centered pivot, horizontal
// Standard definition end point on the right, start point on the left
public class StraightPipeView : PurePipeView
{
    [SerializeField] private Vector3 unitEndPoint = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitStartPoint = new Vector3(-0.5f, 0f, 0f);

    void Awake() {
        basicPipe.ParentPipe = upstream.GetPipe();
    }

    // TODO: Substitute all custom directional vectors
    protected override IEnumerator MoveStream(GameObject content) {
        float startPointSpeed = upstream.isSplitIntersector() ? streamSpeed : (streamSpeed + upstream.GetStreamSpeed()) / 2;
        float timeElapsed = 0;
        float firstHalfAvgSpeed = (startPointSpeed + streamSpeed) / 2;
        float timeFrame = transform.localScale.x / 2 / firstHalfAvgSpeed;
        float leftoverDist = transform.localScale.x / 2 - startPointSpeed * timeFrame;

        // From startpoint to midpoint
        while (timeElapsed < timeFrame) {
            float distOffset = VectorUtils.SquareLerpFloat(0, leftoverDist, timeElapsed / timeFrame);
            content.transform.position = transform.TransformPoint(new Vector3(-transform.localScale.x / 2 + startPointSpeed * timeElapsed + distOffset, 0, 0));
            timeElapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        timeElapsed = 0;
        float endPointSpeed = downstream.isSplitIntersector() ? streamSpeed : (streamSpeed + downstream.GetStreamSpeed()) / 2;
        float secondHalfAvgSpeed = (streamSpeed + endPointSpeed) / 2;
        timeFrame = transform.localScale.x / 2 / secondHalfAvgSpeed;
        leftoverDist = transform.localScale.x / 2 - streamSpeed * timeFrame;

        // From midpoint to endpoint
        while (timeElapsed < timeFrame) {
            float distOffset = VectorUtils.SquareLerpFloat(0, leftoverDist, timeElapsed / timeFrame);
            content.transform.position = transform.TransformPoint(new Vector3(streamSpeed * timeElapsed + distOffset, 0, 0));
            timeElapsed += Time.deltaTime;
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
}
