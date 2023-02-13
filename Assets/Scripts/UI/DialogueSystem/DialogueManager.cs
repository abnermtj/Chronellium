using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Placed under a singleton parent
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    private void Awake() 
    {
        if (DialogueManager.instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    public GameObject dialogBox;
    public Text speakerName, dialogue;
    public Image speakerSprite, playerSprite;
    private int currIndex;
    private Conversation currentConvo;
    public bool inDialogue = false;

    private void Update()
    {
        if (InputManager.dialogButtonActivated && inDialogue)
        {
            InputManager.dialogButtonActivated = false;
            ReadNext();
        }
    }

    public void StartConversation(Conversation convo)
    {
        dialogBox.SetActive(true);
        UiStatus.OpenUI();
        
        currIndex = 0;
        currentConvo = convo;
        speakerName.text = "";
        dialogue.text = "";

        ReadNext();    
        inDialogue = true;  
    }

    public void ReadNext()
    {
        if (currIndex == currentConvo.allLines.Length)
        {    
            EndDialogue();      
        }
        else 
        {
            speakerName.text = currentConvo.allLines[currIndex].speaker.speakerName;
            dialogue.text = currentConvo.allLines[currIndex].dialogue;
            if (currentConvo.allLines[currIndex].isPlayer) {
                playerSprite.enabled = true;
                playerSprite.sprite = currentConvo.allLines[currIndex].speaker.speakerSprite;
                speakerSprite.enabled = false;
            } else {
                speakerSprite.enabled = true;
                speakerSprite.sprite = currentConvo.allLines[currIndex].speaker.speakerSprite;
                playerSprite.enabled = false;
            }
            currIndex++;
        }
    }

    // EXPLANATION: Perviously when "Z" is pressed at the last line of conversation,
    // the TryInteract() method will catch the signal whilst inDialogue might have already been set to false.
    public void EndDialogue() {
        inDialogue = false;
        if (!currentConvo.endWithChoice) {
            UiStatus.CloseUI();
            dialogBox.SetActive(false);
        }
    }
}
