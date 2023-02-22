using System;
using UnityEngine;

public class UiStatus : MonoBehaviour
{
    public static bool isOpen;
    public static event Action onOpenUI;
    public static event Action onCloseUI;

    void Awake() {
        // Need to reset static variable isOpen to false
        // otherwise it might not reset automatically when 
        // the scene restarts
        isOpen = false;
    }

    public static void OpenUI()
    {
        isOpen = true;
        if (onOpenUI != null) {
            onOpenUI();
        }
    }

    public static void CloseUI()
    {
        isOpen = false;
        if (onCloseUI != null) {
            onCloseUI();
        }
    }
}