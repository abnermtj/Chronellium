using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Designer's responsibility to ensure that the main, side branch assignment matches orientation of the split intersector on screen
// Standard config, upside down T-shaped. Main branch start point (-0.5, 0), alt branch end point (0, -0.5), main branch end point (0.5, 0)
// Pivots at intersection point
public class SplitIntersectorView : PipeView
{
    [SerializeField] private Vector3 unitMainEndPoint = new Vector3(0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitStartPoint = new Vector3(-0.5f, 0f, 0f);
    [SerializeField] private Vector3 unitSideEndPoint = new Vector3(0f, -0.5f, 0f);
    [SerializeField] private Vector3 unitIntersectPoint = Vector3.zero;
    [SerializeField] private GameObject layeredVirusPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PipeView sideDownstream;
    [SerializeField] private int splitCount;
    private SplitIntersector splitIntersector = new SplitIntersector();
    public LayeredVirus SplittedVirus { get; private set; }
 
    void Awake() {
        splitIntersector.ParentPipe = upstream.GetPipe();
        splitIntersector.StepChildPipe = sideDownstream.GetPipe();
        splitIntersector.SplitCount = splitCount;
        if (!(upstream is PurePipeView) || !(downstream is PurePipeView) || !(sideDownstream is PurePipeView)) Debug.LogError("Pipes connected to split intersector must be pure pipes");
    }

    protected override IEnumerator MoveStream(GameObject content) {
        float startPointSpeed = upstream.GetStreamSpeed() / 2;
        float timeElaped = 0;
        float avgSpeed = startPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;

        // From startpoint to midpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = startPointSpeed - startPointSpeed * (timeElaped / timeFrame);
            // content.transform.position += (transform.position - start).normalized * lerpedSpeed * Time.fixedDeltaTime;
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        SplittedVirus = splitIntersector.GetStepOutput();
        GameObject splittedVirusObject = Instantiate(layeredVirusPrefab, spawnPoint.position, Quaternion.identity);
        splittedVirusObject.GetComponent<VirusView>()?.InitVirus(SplittedVirus);

        StartCoroutine(MoveMainDownstream(content));
        StartCoroutine(MoveSideDownstream(splittedVirusObject));
    }

    IEnumerator MoveMainDownstream(GameObject content) {
        float timeElaped = 0;
        float endPointSpeed = downstream.GetStreamSpeed() / 2;
        float avgSpeed = endPointSpeed / 2;
        float timeFrame = transform.localScale.x / 2 / avgSpeed;

        // From midpoint to endpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = endPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.right) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        downstream.CallMoveStream(content);
    }

    IEnumerator MoveSideDownstream(GameObject content) {
        float timeElaped = 0;
        float endPointSpeed = sideDownstream.GetStreamSpeed() / 2;
        float avgSpeed = endPointSpeed / 2;
        float timeFrame = Mathf.Abs(transform.localScale.y) / 2 / avgSpeed;

        // From midpoint to endpoint
        while (timeElaped < timeFrame) {
            float lerpedSpeed = endPointSpeed * (timeElaped / timeFrame);
            content.transform.position += transform.TransformDirection(Vector3.down * Mathf.Sign(transform.localScale.y)) * lerpedSpeed * Time.fixedDeltaTime;
            timeElaped += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        sideDownstream.CallMoveStream(content);
    }

    protected override void AbsorbFromUpstream() {
        splitIntersector.SetInput();
    }
   
    public override Pipe GetPipe() { return splitIntersector; }
    public override float GetStreamSpeed() { return -1; }
    public override bool isSplitIntersector() { return true; }
}
