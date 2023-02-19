using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A standard curve pipe is the upper right quarter of a circle, with radius 1, pivot at center
// start point at (1,0) end point at (0, 1) with standard coordinate system
// Limited to 90 degrees curves
public class CurvePipeView : PurePipeView
{
    [SerializeField] private Vector3 unitStartPoint = new Vector3(1f, 0f, 0f);
    [SerializeField] private Vector3 unitEndPoint = new Vector3(0f, 1f, 0f);

    protected override IEnumerator MoveStream(GameObject content) {
        // X and Y should be scaled simultaneously
        float radius = transform.localScale.x;
        // In radians z axis
        float upstreamAngularSpeed = upstream.GetStreamSpeed() / radius;
        float currStreamAngularSpeed = streamSpeed / radius;
        float downstreamAngularSpeed = downstream.GetStreamSpeed() / radius;

        float timeElapsed = 0;
        float startPointAngularSpeed = upstream.isSplitIntersector() ? currStreamAngularSpeed : (upstreamAngularSpeed + currStreamAngularSpeed) / 2;
        float firstHalfAvgAngularSpeed = (startPointAngularSpeed + currStreamAngularSpeed) / 2;
        float timeFrame = Mathf.PI / 4 / firstHalfAvgAngularSpeed;
        float leftoverAngle = Mathf.PI / 4 - startPointAngularSpeed * timeFrame;

        // Assumes content enters centered on the curve bounds
        while (timeElapsed < timeFrame) {
            float angleOffset = VectorUtils.SquareLerpFloat(0, leftoverAngle, timeElapsed / timeFrame);
            content.transform.position = transform.position + Quaternion.Euler(0, 0, Mathf.Rad2Deg * (startPointAngularSpeed * timeElapsed + angleOffset)) * Vector3.right * transform.localScale.x;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        timeElapsed = 0;
        float endPointAngularSpeed = downstream.isSplitIntersector() ? currStreamAngularSpeed : (currStreamAngularSpeed + downstreamAngularSpeed) / 2;
        float secondHalfAvgAngularSpeed = (currStreamAngularSpeed + endPointAngularSpeed) / 2;
        timeFrame = Mathf.PI / 4 / secondHalfAvgAngularSpeed;
        leftoverAngle = Mathf.PI / 4 - currStreamAngularSpeed * timeFrame;

        while (timeElapsed < timeFrame) {
            float angleOffset = VectorUtils.SquareLerpFloat(0, leftoverAngle, timeElapsed / timeFrame);
            content.transform.position = transform.position + Quaternion.Euler(0, 0, Mathf.Rad2Deg * (currStreamAngularSpeed * timeElapsed + angleOffset + Mathf.PI / 4)) * Vector3.right * transform.localScale.x;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        if (providesMainInput) {
            downstream.CallMoveStream(content, this);
        } else {
            if (downstream.GetComponent<MergeIntersectorView>() == null) {
                Debug.LogError("providesMainInput can only be false when attached to merge intersector");
            } else {
                ((MergeIntersectorView)downstream).CallMoveSidestream(content, this);
            }
        }
    }
}
