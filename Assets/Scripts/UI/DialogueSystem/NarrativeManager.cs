using UnityEngine;
using UnityEngine.UI;

// Same as Dialogue Manager but with a larger dialogue box
// and no speaker. Used for stories etc.

// Must be placed under a singleton parent.
public class NarrativeManager : MonoBehaviour
{
    public GameObject dialogBox;
    public Text dialogue;
    private int currIndex;
    private Conversation currentConvo;
    public static bool inDialogue = false;

    private void Awake()
    {
        // TODO: Should change to SetActive()
        dialogBox.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (inDialogue)
                ReadNext();
        }
    }

    public void StartConversation(Conversation convo)
    {
        UiStatus.OpenUI();
        inDialogue = true;
        dialogBox.transform.localScale = Vector3.one;
        currIndex = 0;
        currentConvo = convo;

        ReadNext();
    }

    public void ReadNext()
    {
        if (currIndex >= currentConvo.allLines.Length)
        {
            dialogBox.transform.localScale = Vector3.zero;
            inDialogue = false;
            UiStatus.CloseUI();
        }
        else
        {
            dialogue.text = currentConvo.allLines[currIndex].dialogue;
            currIndex++;
        }
    }
}
