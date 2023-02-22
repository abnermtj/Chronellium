using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public string sceneName;
  
    // Assumes every scene will have a scene transition gameobject.
    void Start() {
        EventManager.InvokeEvent(EventManager.Event.SCENE_START);
    }

    public void SceneTransit() {
        GameManager.instance.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public static void SceneTransit(string sceneName) {
        GameManager.instance.lastScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}
