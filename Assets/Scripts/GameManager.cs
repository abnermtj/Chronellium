using UnityEngine;

// All data that must be persisted through scene in the session will be stored here.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public string lastScene;

    private void Awake()
    {
        if (GameManager.instance != null) {
            Destroy(gameObject);
            return;
        }
        // TESTCODE
        Inventory gameInventory = new Inventory();
        Inventory.AssignNewInventory(gameInventory);
       
        DontDestroyOnLoad(gameObject);
        instance = this;
    }
}