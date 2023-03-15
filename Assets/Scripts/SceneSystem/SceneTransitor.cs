using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitor : MonoBehaviour
{
    public enum Activation
    {
        OnCollide,
        OnInteract,
        OnButtonPressK,
    }

    public SceneAsset NextScene;
    public Animator Transition = null;
    public float TransitionTime = 1f;
    public SceneAsset LoadingScene = null;          // Default is "DefaultLoading"
    public bool UseLoading = false;
    public Activation Trigger;
    public bool AdditiveAddToCurrentScene = false;


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            return;
        }

        Debug.Log("Collision Detected");

        if (Trigger == Activation.OnCollide)
        {
            Debug.Log("Collide Triggered");
            TransitionToNextScene();
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.tag != "Player")
        {
            return;
        }

        Debug.Log("CollisionStay Detected");

        // Quick hack to allow interact with SceneTransitors, might want to adapt to interaction system in game. 
        // For example, show an "E to interact" option when player overlaps transitor.
        if (Trigger == Activation.OnInteract && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interact Triggered");
            TransitionToNextScene();
        }
    }

    private void LoadNextScene()
    {
        // Check if using additive scene loading
        if (AdditiveAddToCurrentScene)
        {
            SceneLoader.EnableAdditive();
        }

        if (UseLoading)
        {
            if (LoadingScene != null)
            {
                SceneLoader.LoadWithLoading(NextScene.name, LoadingScene.name);
            }
            else
            {
                SceneLoader.LoadWithLoading(NextScene.name);
            }
        }
        else
        {
            SceneLoader.Load(NextScene.name);
        }
        Debug.Log("Now in Scene: " + NextScene.name);
    }

    private void TransitionToNextScene()
    {
        if (Transition != null)
        {
            Debug.Log("Using Transition: " + Transition.ToString());
            StartCoroutine(LoadTransitionThenScene());
        }
        else
        {
            LoadNextScene();
        }

    }

    private IEnumerator LoadTransitionThenScene()
    {
        Transition.SetTrigger("Start");
        Debug.Log("Transition done");

        yield return new WaitForSeconds(TransitionTime);

        LoadNextScene();
    }

    // Update is called once per frame
    void Update()
    {
        if (Trigger == Activation.OnButtonPressK)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                Debug.Log("K Pressed");
                TransitionToNextScene();
            }
        }
    }
}
