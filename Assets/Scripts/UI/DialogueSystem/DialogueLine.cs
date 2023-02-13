using UnityEngine;

[System.Serializable]
public class DialogueLine 
{
    public Speaker speaker;
    [TextArea]
    public string dialogue;
    public bool isPlayer;
}
