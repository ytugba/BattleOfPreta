using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public Queue<string> sentences;
    public new TMPro.TMP_Text name;
    public TMPro.TMP_Text dialogue;
    public GameObject boss;
    public Animator animator;
    public GameObject player;
    public BoxCollider trigger;
    public GameObject dialoguePanel;
    internal void StartDialogue(Dialogue dialogue)
    {
        boss.SetActive(true);
        StartCoroutine(Arise());
        sentences = new Queue<string>();
        name.text = dialogue.name;
        player.GetComponent<Combat>().enabled = false;
        sentences.Clear();

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }

    private IEnumerator Arise()
    {
        animator.SetTrigger("Raise");
        trigger.enabled = false;
        yield break;
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count ==0)
        {
            EndDialogue();

            return;
        }

        string sentence = sentences.Dequeue();
        StartCoroutine(ShowText(sentence));
        dialogue.text = sentence;

    }

    string currentText = "";
    private IEnumerator ShowText(string sentence)
    {
        for(int i =0; i < sentence.Length; i++)
        {
            currentText = sentence.Substring(0,i);
            dialogue.text = currentText;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void EndDialogue()
    {
        Debug.Log("Finish the level");
        StartCoroutine(Conversation());
        dialoguePanel.SetActive(false);
        Cursor.visible = false;
        player.GetComponent<Combat>().enabled = true;
    }

    IEnumerator Conversation()
    {
        animator.SetTrigger("GoBack");
        yield return new WaitForSeconds(2);
        boss.SetActive(false);
    }
}
