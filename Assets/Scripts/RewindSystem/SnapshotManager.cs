using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnapshotManager : MonoBehaviour
{
    public static SnapshotManager instance;
    public List<Memorable> memorables = new List<Memorable>();
    public int snapshotCounter;
    public Stack<Snapshot> snapshots = new Stack<Snapshot>();
    // The snapshot chrnologically closest to the current game time
    public Snapshot ActiveSnapshot { get; private set; }

    void Awake() {
        if (instance == null) {
            instance = this;
            ActiveSnapshot = Snapshot.noSnapshot;
        }
    }
    
    // Init using latest snapshot recorded in DB
    public void Init() {
        // TESTCODE
        snapshotCounter = 0;
    }

    public void takeSnapShot() {
        Debug.Log("Snapshot taken");
        Snapshot snapshot = new Snapshot(snapshotCounter, SceneManager.GetActiveScene().name);
        ActiveSnapshot = snapshot;
        snapshots.Push(snapshot);
        // TODO: Await and ask DataManager to save snapshot identifier to DB first before any memorables belonging to the snapshot is saved
        foreach (Memorable mem in memorables) {
            mem.takeSnapShot(snapshot);
        }
        snapshotCounter += 1;
    }

    public void loadSnapShot(int offset) {
        // TODO: Ask dataManager to remove these snapshots as well as the components that belong to them
        for (int i = 0; i < offset; i++) {
            snapshots.Pop();
        }
        ActiveSnapshot = snapshots.Peek();

        if (SceneManager.GetActiveScene().name != ActiveSnapshot.SceneName) {
            SceneManager.LoadSceneAsync(ActiveSnapshot.SceneName);
            memorables.Clear();
        } else {
            foreach (Memorable mem in memorables) {
                mem.loadSnapShot(offset);
            }
        }
    }
}
