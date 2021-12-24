using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    public GameObject helpPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void HelpPanelVisibility()
    {
        if(helpPanel.activeInHierarchy)
        {
            helpPanel.SetActive(false);
        }
        else
        {
            helpPanel.SetActive(true);
        }
    }
}
