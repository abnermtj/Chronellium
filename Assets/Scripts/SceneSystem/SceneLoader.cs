using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // Dummy class to start Coroutine (needs an instance that extends MonoBehaviour)
    private class LoadingMonoBehaviour : MonoBehaviour { }

    // Defaults to false after each load scene
    private static bool isAdditive = false;
    private static HashSet<string> loadedScenes = new HashSet<string>();

    public static void EnableAdditive()
    {
        isAdditive = true;
    }

    // Load without transition
    public static void Load(string scene)
    {
        // GameManager will not have a null reference when starting from Master scene.
        // Otherwise, comment the next line out to try without using GameManager.
        GameManager.instance.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (isAdditive)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);
            loadedScenes.Add(scene);
            isAdditive = false;
        }
        else
        {
            SceneManager.LoadScene(scene);
        }

        // TODO: Null reference if EventManager not present in next scene.
        // EventManager.InvokeEvent(EventManager.Event.SCENE_START);
    }

    private static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;

    // Load with transition
    public static void LoadWithLoading(string scene, string loading = "DefaultLoading")
    {
        // GameManager will not have a null reference when starting from Master scene.
        // Otherwise, comment the next line out to try without using GameManager.
        GameManager.instance.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Set the loader callback action to load the target scene
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new GameObject("Loading Game Object");

            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));

            // TODO: Null reference if EventManager not present in next scene.
            // EventManager.InvokeEvent(EventManager.Event.SCENE_START);
        };

        // Load the loading scene
        SceneManager.LoadScene(loading);
    }

    private static IEnumerator LoadSceneAsync(string scene)
    {
        // Running as Coroutine will run over several frames, but yield will prevent it from going past one frame
        yield return null;

        if (isAdditive)
        {
            loadingAsyncOperation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            loadedScenes.Add(scene);
            isAdditive = false;
        }
        else
        {
            loadingAsyncOperation = SceneManager.LoadSceneAsync(scene);
        }

        while (!loadingAsyncOperation.isDone)
        {
            yield return null;
        }
    }

    public static float GetLoadingProgress()
    {
        if (loadingAsyncOperation != null)
        {
            return loadingAsyncOperation.progress;
        }
        else
        {
            return 1f;
        }
    }

    public static void LoaderCallback()
    {
        // Triggered after the first Update which lets the screen refresh
        // Execute the loader callback action which will load the target scene
        if (onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }

    public static void UnloadScene(string scene)
    {
        if (loadedScenes.Contains(scene))
        {
            loadedScenes.Remove(scene);
            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
