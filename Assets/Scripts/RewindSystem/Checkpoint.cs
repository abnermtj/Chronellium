using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField]
    private bool isActive = true;

    void OnTriggerEnter(Collider collider) {
        // Debug.Log("Checking trigger");
        if (isActive && collider.CompareTag("Player")) {
            SnapshotManager.instance.takeSnapShot();
            // NOTE: Could be changed
            isActive = false;
        }
    }
}
