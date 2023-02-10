using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HackGameManager : MonoBehaviour
{
    public static HackGameManager instance;
    public InputNodeView[] allInputNodes;
    public Camera gameCamera;
    public bool gameInProgess;
    [SerializeField] private int endPointCount;
    [SerializeField] private int endPointReached = 0;

    void Awake() {
        if (instance == null) {
            instance = this;
            endPointCount = allInputNodes.Length;
        }
    }

    public void Kickoff() {
        if (gameInProgess) return;
        endPointReached = 0;
        gameInProgess = true;

        foreach (InputNodeView inputNodeView in allInputNodes) {
            inputNodeView.StartStreamWithInput();
        }
    }

    // Game considered ended when all output node destroy incoming virus,
    // since they are located at the end points of the graph.
    public void CheckGameEnded(int layersRemoved) {
        endPointReached += layersRemoved;
        gameInProgess = endPointReached != endPointCount;
    }
}
