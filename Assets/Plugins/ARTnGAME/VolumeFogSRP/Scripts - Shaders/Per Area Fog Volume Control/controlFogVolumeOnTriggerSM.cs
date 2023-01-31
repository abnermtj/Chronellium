using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controlFogVolumeOnTriggerSM : MonoBehaviour
{
    //private float speed = 2f;
    //Renderer rend;
    //int colorPicker = 0;
    public bool powerUpFog = false;
    public connectSuntoVolumeFogURP etherealFog;
    public float noisePower = 0.64f;

    private void Start()
    {
        //rend = GetComponent<Renderer>();
        //transform.position = Vector3.zero;
        //transform.rotation = Quaternion.Euler(0, 90, 0);

        //Create two GameObjects to act as walls
        //GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ////Move the walls to the correct positions
        //leftWall.transform.position = new Vector3(-10, 0, 0);
        //rightWall.transform.position = new Vector3(10, 0, 0);
        ////Scale the walls
        //leftWall.transform.localScale = new Vector3(1, 2, 1);
        //rightWall.transform.localScale = new Vector3(1, 2, 1);
    }
    public float maxBlendPower = 2.5f;
    public float minBlendPower = 1;
    //moves the Primitive 2 units a second in the forward direction
    void Update()
    {
        if (etherealFog != null)
        {
            if (powerUpFog)
            {
                if (etherealFog.blendVolumeLighting != maxBlendPower)
                {
                    etherealFog.blendVolumeLighting = Mathf.Lerp(etherealFog.blendVolumeLighting, maxBlendPower, Time.deltaTime);
                }
                if (etherealFog.LightRaySamples != 9)
                {
                    etherealFog.LightRaySamples = Mathf.Lerp(etherealFog.LightRaySamples, 9, Time.deltaTime);
                }
                if (etherealFog.stepsControl.w != noisePower)
                {
                    etherealFog.stepsControl.w = Mathf.Lerp(etherealFog.stepsControl.w, noisePower, Time.deltaTime);
                }
            }
            else
            {
                if (etherealFog.blendVolumeLighting != minBlendPower)
                {
                    etherealFog.blendVolumeLighting = Mathf.Lerp(etherealFog.blendVolumeLighting, minBlendPower, Time.deltaTime);
                }
                if (etherealFog.LightRaySamples != 7.3f)
                {
                    etherealFog.LightRaySamples = Mathf.Lerp(etherealFog.LightRaySamples, 7.3f, Time.deltaTime);
                }
                if (etherealFog.stepsControl.w != 0)
                {
                    etherealFog.stepsControl.w = Mathf.Lerp(etherealFog.stepsControl.w, 0, Time.deltaTime);
                }
            }
        }
        //transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    //When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {

        if(etherealFog != null && other.gameObject.name == "COLLIDER_HERO")
        {
            powerUpFog = true;
        }

        //speed = speed * -1;
        //colorPicker = Random.Range(0, 10);
    }

    //When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        if (etherealFog != null && other.gameObject.name == "COLLIDER_HERO")
        {
            powerUpFog = false;
        }

        //switch (colorPicker)
        //{
        //    case 0: rend.material.color = Color.white; break;
        //    case 1: rend.material.color = Color.cyan; break;
        //    case 2: rend.material.color = Color.blue; break;
        //    case 3: rend.material.color = Color.black; break;
        //    case 4: rend.material.color = Color.red; break;
        //    case 5: rend.material.color = Color.green; break;
        //    case 6: rend.material.color = Color.grey; break;
        //    case 7: rend.material.color = Color.magenta; break;
        //    case 8: rend.material.color = Color.yellow; break;
        //    case 9: rend.material.color = Color.gray; break;
        //}
    }
}

