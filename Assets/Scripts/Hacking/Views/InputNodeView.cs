using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows players to choose input
public class InputNodeView : PipeView
{
    private InputNode inputNode = new InputNode();
    [SerializeField] private GameObject layeredVirusPrefab;
    [SerializeField] private Transform spawnPoint;

    void Awake() {
        upstream = null;
    }

    public void SetInput(VirusBase baseVirus) {
        inputNode.CreateInput(baseVirus);
    }

    public void StartStreamWithInput() {
        GameObject simpleVirus = Instantiate(layeredVirusPrefab, spawnPoint.position, Quaternion.identity);
        simpleVirus.GetComponent<VirusView>()?.InitVirus(inputNode.GetOutput());
        CallMoveStream(simpleVirus);
    }

    // Do nothing
    protected override void AbsorbFromUpstream() {}

    public override Pipe GetPipe() { return inputNode; }

    public override float GetStreamSpeed() { return 0; }
    protected override IEnumerator MoveStream(GameObject content) {
        // Addition animation logic can be added here
        yield return null;
        downstream.CallMoveStream(content);
    }
}
