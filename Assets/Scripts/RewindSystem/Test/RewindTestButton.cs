using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewindTestButton : MonoBehaviour
{
    public TMP_InputField inputField;

    void Start() {
        inputField.onSubmit.AddListener(RewindBy);
    }

    public void RewindBy(string input) {
        int steps;
        if (int.TryParse(input, out steps)) {
            Debug.Log($"Trying to rewind by {steps}");
            SnapshotManager.instance.loadSnapShot(steps);
        }
    }
}
