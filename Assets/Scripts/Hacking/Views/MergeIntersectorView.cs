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
    [SerializeField] private bool mainVirusWaiting = false;
    [SerializeField] private bool sideVirusWaiting = false;
    [SerializeField] private GameObject coreContent;
    [SerializeField] private GameObject sideContent;

    void Awake() {
        mergeIntersector.ParentPipe = upstream.GetPipe();
        mergeIntersector.StepParentPipe = sideUpstream.GetPipe();
        if (!(upstream is PurePipeView) || !(downstream is PurePipeView) || !(sideUpstream is PurePipeView)) Debug.LogError("Pipes connected to split intersector must be pure pipes");
    }

    public override float GetStreamSpeed() { return 0; }

    public override Pipe GetPipe() { return mergeIntersector; }

    // MoveStream and MoveSidestream are identical only because the start point uses the gamobject position and not the intersector shape.
    protected override IEnumerator MoveStream(GameObject content) {
        coreContent = content;

        float startPointSpeed = upstream.GetStreamSpeed() / 2;
        float timeElapsed = 0;
        float avgSpeed = startPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;
        float leftoverDist = transform.localScale.x / 2 - startPointSpeed * timeFrame;

        // From startpoint to midpoint
        while (timeElapsed < timeFrame) {
            float distOffset = VectorUtils.SquareLerpFloat(0, leftoverDist, timeElapsed / timeFrame);
            content.transform.position = transform.TransformPoint(new Vector3(-transform.localScale.x / 2 + startPointSpeed * timeElapsed + distOffset, 0, 0));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        mainVirusWaiting = true;
        if (sideVirusWaiting) {
            mergeIntersector.GetOutput();
            Destroy(sideContent);
            StartCoroutine(MoveDownstream());
        }
    }
    IEnumerator MoveSidestream(GameObject content) {
        sideContent = content;

        float startPointSpeed = sideUpstream.GetStreamSpeed() / 2;
        float timeElapsed = 0;
        float avgSpeed = startPointSpeed / 2;
        float timeFrame = Mathf.Abs(transform.localScale.y) / 2 / avgSpeed;
        float leftoverDist = Mathf.Abs(transform.localScale.y) / 2 - startPointSpeed * timeFrame;

        // From startpoint to midpoint
        while (timeElapsed < timeFrame) {
            float distOffset = VectorUtils.SquareLerpFloat(0, leftoverDist, timeElapsed / timeFrame);
            if (Mathf.Sign(transform.localScale.y) == 1) {
                content.transform.position = transform.TransformPoint(new Vector3(0, -transform.localScale.y / 2 + startPointSpeed * timeElapsed + distOffset, 0));
            } else {
                content.transform.position = transform.TransformPoint(new Vector3(0, -transform.localScale.y / 2 + startPointSpeed * timeElapsed + distOffset + transform.localScale.y, 0));
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        sideVirusWaiting = true;
        if (mainVirusWaiting) {
            mergeIntersector.GetOutput();
            Destroy(sideContent);
            StartCoroutine(MoveDownstream());
        }
    }

    IEnumerator MoveDownstream() {
        float timeElapsed = 0;
        float endPointSpeed = downstream.GetStreamSpeed() / 2;
        float avgSpeed = endPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;
        float leftoverDist = transform.localScale.x / 2;

        // From midpoint to endpoint
        while (timeElapsed < timeFrame) {
            float distOffset = VectorUtils.SquareLerpFloat(0, leftoverDist, timeElapsed / timeFrame);
            coreContent.transform.position = transform.TransformPoint(new Vector3(distOffset, 0, 0));
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        mainVirusWaiting = false;
        sideVirusWaiting = false;
        downstream.CallMoveStream(coreContent);
    }

    public void CallMoveSidestream(GameObject content) {
        // Debug.Log($"Moving {content.name} in side stream");
        AbsorbFromUpstream();
        StartCoroutine(MoveSidestream(content));
    } 

    // Caveat need wait for 
    protected override void AbsorbFromUpstream() {
        // Debug.Log($"Intersector absorbfrom called");
        virusArrivalCount += 1;
        if (ComponentsReady()){
            mergeIntersector.SetInput();
            virusArrivalCount = 0;
        }
    }

    // Whether both viruses have entered the bounds of the intersector
    bool ComponentsReady() {
        return virusArrivalCount == 2;
    }
}
