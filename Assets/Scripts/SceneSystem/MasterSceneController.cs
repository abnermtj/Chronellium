using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MasterSceneController : MonoBehaviour
{
    [SerializeField]
    private SceneAsset FirstScene;

    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene(FirstScene.name, LoadSceneMode.Additive);
    }
}
