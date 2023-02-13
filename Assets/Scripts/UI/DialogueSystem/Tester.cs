using UnityEngine;

public class Tester : MonoBehaviour
{
    public Conversation convo;

    public void StartConvo()
    {
        DialogueManager.instance.StartConversation(convo);
    }
}
