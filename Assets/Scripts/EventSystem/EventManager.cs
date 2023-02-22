using System.Collections.Generic;
using UnityEngine.Events;

// Aside from the Game Event scriptable object,
// the class maintains core events such as mode and character switch and game wide events, save game etc.
public class EventManager
{
    public enum Event {
        Open_UI,
        Close_UI,
        SCENE_START,
        CHOICE_ONE,
        CHOICE_TWO,
        CHOICE_THREE,
        CHOICE_FOUR
    }

    private static Dictionary<Event, UnityEvent<object>> eventTable = new Dictionary<Event, UnityEvent<object>>();

    public static void StartListening(EventManager.Event eventType, UnityAction<object> listener) {
        UnityEvent<object> thisEvent = null;
        // Find the UnityEvent matching this eventName and add a listener to it
        if (eventTable.TryGetValue(eventType, out thisEvent)) {
            thisEvent.AddListener(listener);

            // If it doesn't exist, create a new UnityEvent corresponding to this eventName
            // and add the listener to it
        } else {
            thisEvent = new UnityEvent<object>();
            thisEvent.AddListener(listener);
            eventTable.Add(eventType, thisEvent);
        }
    }

    // Removes only a single specified listener from the eventTable. Maybe I should
    // create a method that removes all listeners from an event?
    public static void StopListening(EventManager.Event eventType, UnityAction<object> listener) {
        UnityEvent<object> thisEvent = null;
        if (eventTable.TryGetValue(eventType, out thisEvent)) {
            thisEvent.RemoveListener(listener);
        }
    }

    public static void StopListeningAll(EventManager.Event eventType)
    {
        UnityEvent<object> thisEvent = null;
        if (eventTable.TryGetValue(eventType, out thisEvent)) {
            thisEvent.RemoveAllListeners();
        }
    }

    // Get a UnityEvent which can be invoked. 
    // Tries to invoke an existing UnityEvent signified by the eventName string.
    public static void InvokeEvent(EventManager.Event eventType, object inputParam = null) {
        // Debug.Log("Invoking " + eventType.ToString() + " event...");
        UnityEvent<object> thisEvent = null;
        if (eventTable.TryGetValue(eventType, out thisEvent)) {
            thisEvent.Invoke(inputParam);
        } else {
            // Do nothing
            // Debug.LogWarning($"{eventType.ToString()} event does not exist in EventManager!");
        }
    }
}
