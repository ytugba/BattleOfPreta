using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    public GameObject ava;
    public GameObject panel;
    private void OnTriggerEnter(Collider other)
    {
        panel.SetActive(true);
        if(other.CompareTag("Player"))
        {
            //Debug.Log("Game Over!!");
            other.gameObject.SetActive(false);
        }
    }
}
