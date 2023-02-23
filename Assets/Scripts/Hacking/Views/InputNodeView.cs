using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Allows players to choose input
public class InputNodeView : PipeView
{
    private InputNode inputNode = new InputNode();
    [SerializeField] private GameObject layeredVirusPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private VirusBase selectedInput = null;
    [SerializeField] private VirusBaseMenu menu;

    void Awake() {
        upstream = null;
    }

    void OnEnable() {
        menu.onChoiceChanged += ChangeSelectedInput;
    }

    void OnDisable() {
        menu.onChoiceChanged -= ChangeSelectedInput;
    }

    void OnMouseDown() {
        menu.gameObject.SetActive(true);
    }

    bool HasInput() { return selectedInput != null; }

    public void ChangeSelectedInput(VirusBase selectedVirus) {
        selectedInput = selectedVirus;
    }

    void SetInput() {
        inputNode.CreateInput(selectedInput);
    }

    public void StartStreamWithInput() {
        inputNode.Clear();
        if (!HasInput()) selectedInput = menu.allVirusBases[0];
        // Debug.Log($"Starting stream at {name}");
        SetInput();
        inputNode.DetermineOutput();
        GameObject simpleVirus = Instantiate(layeredVirusPrefab, spawnPoint.position, Quaternion.identity);
        simpleVirus.GetComponent<VirusView>()?.InitVirus(inputNode.GetOutput());
        CallMoveStream(simpleVirus, this);
    }

    // Do nothing
    protected override void AbsorbFromUpstream(PipeView providedUpstream = null) {}

    public override Pipe GetPipe() { return inputNode; }

    public override float GetStreamSpeed() { return 0; }
    protected override IEnumerator MoveStream(GameObject content) {
        // Addition animation logic can be added here
        yield return null;
        downstream.CallMoveStream(content, this);
    }
}
