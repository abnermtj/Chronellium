using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.PostProcessing;
using PRISM.Utils;

namespace PRISM.Utils {


    public enum PostProcessPropertyToChangeType { Exposure = 1, ColorTemp = 2, Nothing = 3 };

    public class CUPPRuntimePropertyAdjuster : MonoBehaviour
    {
        private PRISMEffects targetCUPPEffectsToChange;
        private float t = 0f;
        private float lerpSpeed = 1f;

        private float originalExposure = 1f;
        private float originalColorTemp = 1f;

        public PostProcessPropertyToChangeType propertyToChange = PostProcessPropertyToChangeType.Exposure;

        private void Start()
        {
            Volume volume = gameObject.GetComponent<Volume>();
            PRISMEffects tmp;
            if (volume.profile.TryGet<PRISMEffects>(out tmp))
            {
                targetCUPPEffectsToChange = tmp;
            }
        }

        // Update is called once per frame
        void Update()
        {
            t += Time.deltaTime * lerpSpeed;

            if(t > 1f)
            {
                lerpSpeed = -1f;
            }

            if(t <= 0f)
            {
                lerpSpeed = 1f;
            }
            
            if(propertyToChange == PostProcessPropertyToChangeType.ColorTemp)
            {
                targetCUPPEffectsToChange.colorTemperature.value = originalColorTemp * t;
            } else if(propertyToChange == PostProcessPropertyToChangeType.Exposure)
            {
                targetCUPPEffectsToChange.exposure.value = originalExposure * t;
            } else //Nothing
            {
                targetCUPPEffectsToChange.colorTemperature.value = originalColorTemp;
                targetCUPPEffectsToChange.exposure.value = originalExposure;
            }
        }
    }

   

}
