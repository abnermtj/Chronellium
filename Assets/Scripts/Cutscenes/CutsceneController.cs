using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneController : MonoBehaviour
{
    private static bool isPlayed;
    public GameObject timeline;

    // Update is called once per frame
    void Update()
    {
        if (!isPlayed) {
            timeline.SetActive(true);
            isPlayed = true;
        }
    }
}
