using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VLB;

public class FogTransitionCity : MonoBehaviour
{
    [SerializeField]
    public GameObject beamLight;

    private void OnTriggerExit(Collider other)
    {
        beamLight.GetComponent<VolumetricLightBeam>().sortingOrder = 400;
        beamLight.GetComponent<VolumetricLightBeam>().intensityInside = 0.35f;
        beamLight.GetComponent<VolumetricLightBeam>().intensityOutside = 0.35f;
        Debug.Log("LERP");
        StartCoroutine(LerpFunction(0, 0.35f, 2f));
        //beamLight.transform.eulerAngles = new Vector3(-11.459f, -175.378f, 0);
    }

    private void OnTriggerEnter(Collider collision)
    {
        beamLight.GetComponent<VolumetricLightBeam>().sortingOrder = 37;
        beamLight.GetComponent<VolumetricLightBeam>().intensityInside = 0.01f;
        beamLight.GetComponent<VolumetricLightBeam>().intensityOutside = 0.01f;
        StartCoroutine(LerpFunction(0.35f, 0.0f, 2f));
        //beamLight.transform.eulerAngles = new Vector3(-11.284f, -165.24f, -2.004f);
    }

    private IEnumerator LerpFunction(float startValue, float endValue, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            beamLight.GetComponent<VolumetricLightBeam>().intensityInside = Mathf.Lerp(startValue, endValue, time / duration);
            beamLight.GetComponent<VolumetricLightBeam>().intensityOutside = Mathf.Lerp(startValue, endValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        beamLight.GetComponent<VolumetricLightBeam>().intensityInside = endValue;
        beamLight.GetComponent<VolumetricLightBeam>().intensityOutside = endValue;
    }
}