using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Designer's responsibility to ensure that the main, side branch assignment matches orientation of the merge intersector on screen
// Standard config, upside down T-shaped. Main branch start point (-0.5, 0), alt branch start point (0, -0.5), main branch end point (0.5, 0)
// Pivots at intersection point
// NOTE: Refrain from scaling the intersectors as the movement logic is dependent on the fixed pipe length
// NOTE: Intersectors can only be connected to BasicPipe
// NOTE: Always use y scale to mirror invert
public class MergeIntersectorView : PipeView
{
    [SerializeField] private Vector3 unitEndPoint = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitMainStartPoint = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitSideStartPoint = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 unitIntersectPoint = Vector3.zero;
    [SerializeField] private PipeView sideUpstream;
    private MergeIntersector mergeIntersector = new MergeIntersector();
    [SerializeField] private int virusArrivalCount = 0;
    [SerializeField] private GameObject coreContent;

    void Awake() {
        mergeIntersector.ParentPipe = upstream.GetPipe();
        mergeIntersector.StepParentPipe = sideUpstream.GetPipe();
        if (!(upstream is PurePipeView) || !(downstream is PurePipeView) || !(sideUpstream is PurePipeView)) Debug.LogError("Pipes connected to split intersector must be pure pipes");
    }

    public override float GetStreamSpeed() { return 0; }

    public override Pipe GetPipe() { return mergeIntersector; }

    // MoveStream and MoveSidestream are identical only because the start point uses the gamobject position and not the intersector shape.
    protected override IEnumerator MoveStream(GameObject content) {
        // Vector3 start = content.transform.position;
        coreContent = content;

        float startPointSpeed = upstream.GetStreamSpeed() / 2;
        float timeElaped = 0;
        float avgSpeed = startPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;

        // From startpoint to midpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = startPointSpeed - startPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (ComponentsReady()) {
            mergeIntersector.GetOutput();
            StartCoroutine(MoveDownstream());
        }
    }
    IEnumerator MoveSidestream(GameObject content) {
        float startPointSpeed = sideUpstream.GetStreamSpeed() / 2;
        float timeElaped = 0;
        float avgSpeed = startPointSpeed / 2;
        float timeFrame = Mathf.Abs(transform.localScale.y) / 2 / avgSpeed;

        // From startpoint to midpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = startPointSpeed - startPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.up * Mathf.Sign(transform.localScale.y)) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (ComponentsReady()) {
            mergeIntersector.GetOutput();
            Destroy(content);
            StartCoroutine(MoveDownstream());
        }
    }

    IEnumerator MoveDownstream() {
        float timeElaped = 0;
        float endPointSpeed = downstream.GetStreamSpeed() / 2;
        float avgSpeed = endPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;

        // From midpoint to endpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = endPointSpeed * (timeElaped / timeFrame);
            coreContent.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        downstream.CallMoveStream(coreContent);
    }

    public void CallMoveSidestream(GameObject content) {
        Debug.Log($"Moving {content.name} in side stream");
        AbsorbFromUpstream();
        StartCoroutine(MoveSidestream(content));
    } 

    // Caveat need wait for 
    protected override void AbsorbFromUpstream() {
        Debug.Log($"Intersector absorbfrom called");
        virusArrivalCount += 1;
        if (ComponentsReady()) mergeIntersector.SetInput();
    }

    bool ComponentsReady() {
        return virusArrivalCount == 2;
    }
}
