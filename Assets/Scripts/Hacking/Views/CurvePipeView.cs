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

    void Awake() {
        basicPipe.ParentPipe = upstream.GetPipe();
    }

    protected override IEnumerator MoveStream(GameObject content) {
        // X and Y should be scaled simultaneously
        float radius = transform.localScale.x;
        // In radians
        Vector3 upstreamAngularSpeed = new Vector3(0f, 0f, upstream.GetStreamSpeed() / radius);
        Vector3 currStreamAngularSpeed = new Vector3(0f, 0f, streamSpeed / radius);
        Vector3 downstreamAngularSpeed = new Vector3(0f, 0f, downstream.GetStreamSpeed() / radius);

        float timeElapsed = 0;
        Vector3 startPointAngularSpeed = upstream.isSplitIntersector() ? currStreamAngularSpeed : (upstreamAngularSpeed + currStreamAngularSpeed) / 2;
        Vector3 firstHalfAvgAngularSpeed = (startPointAngularSpeed + currStreamAngularSpeed) / 2;
        float timeFrame = Mathf.PI / 4 / firstHalfAvgAngularSpeed.z;
        Vector3 angleTravelled = Vector3.zero;

        // Assumes content enters centered on the curve bounds
        while (timeElapsed < timeFrame) {
            Vector3 lerpedAngularSpeed = startPointAngularSpeed + (currStreamAngularSpeed - startPointAngularSpeed) * (timeElapsed / timeFrame);
            angleTravelled += lerpedAngularSpeed * Time.fixedDeltaTime;
            Vector3 currAngle = transform.rotation.eulerAngles + angleTravelled;
            content.transform.position = transform.position + Quaternion.Euler(currAngle.x, currAngle.y, currAngle.z) * unitStartPoint * radius;
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        timeElapsed = 0;
        Vector3 endPointAngularSpeed = downstream.isSplitIntersector() ? currStreamAngularSpeed : (currStreamAngularSpeed + downstreamAngularSpeed) / 2;
        Vector3 secondHalfAvgAngularSpeed = (currStreamAngularSpeed + endPointAngularSpeed) / 2;
        timeFrame = Mathf.PI / 4 / secondHalfAvgAngularSpeed.z;

        while (timeElapsed < timeFrame) {
            Vector3 lerpedAngularSpeed = currStreamAngularSpeed + (endPointAngularSpeed - currStreamAngularSpeed) * (timeElapsed / timeFrame);
            angleTravelled += lerpedAngularSpeed * Time.fixedDeltaTime;
            Vector3 currAngle = transform.rotation.eulerAngles + angleTravelled;
            content.transform.position = transform.position + Quaternion.Euler(currAngle.x, currAngle.y, currAngle.z) * unitStartPoint * radius;
            timeElapsed += Time.fixedDeltaTime;
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
