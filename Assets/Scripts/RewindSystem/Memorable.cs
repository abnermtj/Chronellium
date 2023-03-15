using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// T should
public abstract class Memorable : MonoBehaviour
{
    [SerializeField]
    private string pathInDb;

    // TESTCODE should change to Awake()
    void Start() {
        SnapshotManager.instance.memorables.Add(this);
        Init();
    }

    public void Init() {
        if (!SnapshotManager.instance.ActiveSnapshot.isNull()) {
            Debug.Log($"Loading snapshot from active scene in {name}");
            loadActiveSnapshot();
        }
    }
    public abstract void takeSnapShot(Snapshot snapshot);
    public void loadActiveSnapshot() {
        loadSnapShot(SnapshotManager.instance.ActiveSnapshot);
    }
    // NOTE: Retrieve state from DB
    protected abstract void loadSnapShot(Snapshot snapshot);
    // NOTE: If loading from local cache failed delegate call to loadSnapShot(snapshot)
    public abstract void loadSnapShot(int offset);
}
