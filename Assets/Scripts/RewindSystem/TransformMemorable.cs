using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Saveable;
// TESTCODE
using KinematicCharacterController;

public class TransformMemorable : Memorable
{
    // Should always be matching pairs starting from stack top with SnapshotManager.memorables
    [SerializeField]
    private Stack<SaveableTransform> transformMemory = new Stack<SaveableTransform>();

    public override void takeSnapShot(Snapshot snapshot) {
        transformMemory.Push(new SaveableTransform(transform));
        Debug.Log($"{name} position {transform.position} saved");
    }

    public override void loadSnapShot(int offset) {
        Debug.Log($"Loading snapshot for {name}");
        if (offset >= transformMemory.Count) {
            Debug.Log("Local cache insufficient");
            transformMemory.Clear();
            // need retrieve from db
            loadActiveSnapshot();
            return;
        }

        for (int i = 0; i < offset; i++) {
            transformMemory.Pop();
        }

        SaveableTransform targetTransform = transformMemory.Peek();
        Debug.Log($"{name} position {targetTransform.position}");

        // TESTCODE: Should have a unified scheme for all transform
        if (name == "Scientist") {
            Debug.Log("Setting scientist position");
            GetComponent<KinematicCharacterMotor>().SetPositionAndRotation(targetTransform.position, targetTransform.rotation, true);
            return;
        }

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = targetTransform.scale;
    }

    protected override void loadSnapShot(Snapshot snapshot) {
        // TODO: Get from db
    }
}
