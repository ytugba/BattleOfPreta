using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public DialogueManager dialogueManager;
    public GameObject dialoguePanel;
    public GameObject boss;
    public void OnTriggerEnter()
    {
        boss.SetActive(true);
        dialogueManager.enabled = true;
        dialoguePanel.SetActive(true);
        Cursor.visible = true;
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }
}
