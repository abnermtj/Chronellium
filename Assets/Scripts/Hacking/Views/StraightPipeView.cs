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
        Vector3 start = content.transform.position;
        // Vector3 vectorToRotate = unitEndPoint * transform.localScale.x;
        // vectorToRotate = transform.rotation * vectorToRotate;

        float startPointSpeed = upstream.isSplitIntersector() ? streamSpeed : (streamSpeed + upstream.GetStreamSpeed()) / 2;
        float timeElaped = 0;
        float firstHalfAvgSpeed = (startPointSpeed + streamSpeed) / 2;
        float timeFrame = transform.localScale.x / 2 / firstHalfAvgSpeed;

        // From startpoint to midpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = startPointSpeed + (streamSpeed - startPointSpeed) * (timeElaped / timeFrame);
            // content.transform.position += vectorToRotate.normalized * lerpedSpeed * Time.fixedDeltaTime;
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        timeElaped = 0;
        float endPointSpeed = downstream.isSplitIntersector() ? streamSpeed : (streamSpeed + downstream.GetStreamSpeed()) / 2;
        float secondHalfAvgSpeed = (streamSpeed + endPointSpeed) / 2;
        timeFrame = transform.localScale.x / 2 / secondHalfAvgSpeed;

        // From midpoint to endpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = streamSpeed + (endPointSpeed - streamSpeed) * (timeElaped / timeFrame);
            // content.transform.position += vectorToRotate.normalized * lerpedSpeed * Time.fixedDeltaTime;
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (isMain) {
            downstream.CallMoveStream(content);
        } else {
            if (!(downstream is MergeIntersectorView)) {
                Debug.LogError("isMain can only be false when attached to merge intersector");
            } else {
                ((MergeIntersectorView)downstream).CallMoveSidestream(content);
            }
        }
    }
}
