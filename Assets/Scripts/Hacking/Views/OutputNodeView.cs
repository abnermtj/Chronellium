using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutputNodeView : GeneralOutputView
{
    void Awake() {
        outputNode = new OutputNode(target);
        outputNode.ParentPipe = upstream.GetPipe();
        downstream = null;
    }

    protected override IEnumerator MoveStream(GameObject content) {
        // Any animation logic here
        yield return null;
        EndStream(content);
    }

    void EndStream(GameObject content) {
        outputNode.DetermineOutput();
        Destroy(content);
        HackGameManager.instance.CheckGameEnded(content.GetComponent<VirusView>().LayerCount());
    }
}
