using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Placed under a singleton parent
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    private void Awake()
    {
        if (DialogueManager.instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public GameObject dialogBox;
    public Text speakerName, dialogue;
    public Image leftSprite, rightSprite;
    public DialogueAudioManager dialogueAudioManager;
    private int currIndex;
    private Conversation currentConvo;
    public bool inDialogue = false;
    [SerializeField]
    private float typingSpeed = 0.005f;
    private Coroutine dialogueLineCoroutine;

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
        if (convo.startingLeftSpeaker.speakerSprite != null)
        {
            leftSprite.sprite = convo.startingLeftSpeaker.speakerSprite;
        }
        if (convo.startingRightSpeaker.speakerSprite != null)
        {
            rightSprite.sprite = convo.startingRightSpeaker.speakerSprite;
        }

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
            if (dialogueLineCoroutine != null)
            {
                StopCoroutine(dialogueLineCoroutine);
            }

            dialogueLineCoroutine = StartCoroutine(DisplayLine(currentConvo.allLines[currIndex].dialogue, currentConvo.allLines[currIndex].speaker.speakerName));

            if (currentConvo.allLines[currIndex].isLeft)
            {
                leftSprite.color = new Color32(255, 255, 255, 255);
                leftSprite.sprite = currentConvo.allLines[currIndex].speaker.speakerSprite;
                rightSprite.color = new Color32(110, 110, 110, 255);
            }
            else
            {
                rightSprite.color = new Color32(255, 255, 255, 255);
                rightSprite.sprite = currentConvo.allLines[currIndex].speaker.speakerSprite;
                leftSprite.color = new Color32(110, 110, 110, 255);
            }

            speakerName.text = currentConvo.allLines[currIndex].speaker.speakerName;
            currIndex++;
        }
    }

    private IEnumerator DisplayLine(string line, string speakerName)
    {
        dialogue.text = "";

        // for Dialogue Audio use
        int characterCount = 0;
        dialogueAudioManager.SetCurrentAudioInfo(speakerName);

        foreach (char letter in line.ToCharArray())
        {
            yield return new WaitForSeconds(typingSpeed);

            dialogueAudioManager.PlayDialogueSound(characterCount, letter);
            characterCount++;
            dialogue.text += letter;
        }
    }

    // Call CloseUI() only when the dialogue does not end with a choice,
    // otherwise, OpenUI() in ChoiceManager might be called before CloseUI()
    // in DialogueManager, which leads to isOpen in UiStatus set to false
    // even when the UI is still oopen
    public void EndDialogue()
    {
        inDialogue = false;
        if (dialogueLineCoroutine != null)
        {
            StopCoroutine(dialogueLineCoroutine);
        }
        if (!currentConvo.endWithChoice)
        {
            UiStatus.CloseUI();
            dialogBox.SetActive(false);
        }
    }
}