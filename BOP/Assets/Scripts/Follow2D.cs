using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow2D : MonoBehaviour
{
    public GameObject ava;
    public GameObject camBoundaries;
    public GameObject gameManager;
    public GameObject playButton;
    public string sentence;
    public TMPro.TMP_Text storyText;
    // Start is called before the first frame update
    void Start()
    {
        ava = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(ShowText(sentence));
    }

    string currentText = "";
    private IEnumerator ShowText(string sentence)
    {
        for (int i = 0; i < sentence.Length; i++)
        {
            currentText = sentence.Substring(0, i);
            storyText.text = currentText;
            yield return new WaitForSeconds(0.05f);
        }
        playButton.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.GetComponent<EnemySpawner>().isPlayerinBattleZone && !gameManager.GetComponent<EnemySpawner>().isAnyWaveActive)
        {
            Vector3 desiredPos = new Vector3(ava.transform.position.x + 2, transform.position.y, transform.position.z);
            Vector3 smooth = Vector3.Lerp(transform.position, desiredPos, 0.1f);
            transform.position = smooth;
            camBoundaries.gameObject.SetActive(false);
        }
        else if (!gameManager.GetComponent<EnemySpawner>().isPlayerinBattleZone && gameManager.GetComponent<EnemySpawner>().isAnyWaveActive)
        {
            gameManager.GetComponent<EnemySpawner>().isPlayerinBattleZone = true;
            camBoundaries.gameObject.SetActive(true);
            gameManager.GetComponent<EnemySpawner>().StartWave();
        }
    }
}
