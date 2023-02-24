using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    // Dummy class to start Coroutine (needs an instance that extends MonoBehaviour)
    private class LoadingMonoBehaviour : MonoBehaviour { }

    // Load without transition
    public static void Load(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    private static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;

    // Load with transition
    public static void LoadWithLoading(string scene, string loading = "DefaultLoading")
    {
        // Set the loader callback action to load the target scene
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new GameObject("Loading Game Object");

            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
        };

        // Load the loading scene
        SceneManager.LoadScene(loading);
    }

    private static IEnumerator LoadSceneAsync(string scene)
    {
        // Running as Coroutine will run over several frames, but yield will prevent it from going past one frame
        yield return null;

        loadingAsyncOperation = SceneManager.LoadSceneAsync(scene);

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
}
