using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Conversation : ScriptableObject
{
    [SerializeField]
    public DialogueLine[] allLines; 
    public bool endWithChoice;  
}
