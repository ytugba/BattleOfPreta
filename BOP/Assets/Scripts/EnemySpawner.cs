using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{

    [Serializable]
    public class Wave
    {
        public bool isActive;
        public GameObject enemySpawnZone;
        public int maxEnemiesinScene;
        public Transform[] enemyTypes;
        public bool hasBoss;
        public Transform boss;
    };

    public Text canMoveText; //done
    public Wave[] waves;
    public float maxSpawnDelay;
    internal bool isPlayerinBattleZone = false;
    internal bool isAnyWaveActive = false;
    private int currentEnemyType = 0;
    private int currentWaveID = 0;
    private float currentSpawnDelay = 0;
    public int maxEnemiesInSingleScene;
    private int remainedEnemy;

    public void StartWave()
    {
        if(currentWaveID < waves.Length)
        {
            canMoveText.gameObject.SetActive(false);
            waves[currentWaveID].isActive = true;

            if (isAnyWaveActive)
            {
                isAnyWaveActive = false;
                Debug.Log("Wave: " + currentWaveID + "has started.");
                StartCoroutine(Spawn(waves[currentWaveID]));
                SpawnBoss(waves[currentWaveID]);
            }
        }
        else
        {
            Debug.Log("Wave is not defined.");
        }
    }

    private IEnumerator SpawnBoss(Wave wave)
    {
        currentSpawnDelay = Random.Range(0.0f, maxSpawnDelay);
        Vector3 spawnMidPoint = wave.enemySpawnZone.transform.position;
        float range = wave.enemySpawnZone.gameObject.GetComponent<MeshRenderer>().bounds.max.z / 2 - 0.5f;
        Vector3 spawnPoint = new Vector3(spawnMidPoint.x, spawnMidPoint.y, Random.Range(spawnMidPoint.z - range, spawnMidPoint.z + range));
        currentEnemyType = Random.Range(0, wave.enemyTypes.Length);
        Instantiate(wave.boss, spawnPoint, transform.rotation);

        yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length == 0);
        StopWave();
        Debug.Log("Killed the boss. GG");
        
    }

    private IEnumerator Spawn(Wave wave)
    {
        Debug.Log("Enemy spawn has started.");
        for (int i = 0; i < wave.maxEnemiesinScene; i++)
        {
            currentSpawnDelay = Random.Range(0.0f, maxSpawnDelay);
            Vector3 spawnMidPoint = wave.enemySpawnZone.transform.position;
            float range = wave.enemySpawnZone.gameObject.GetComponent<MeshRenderer>().bounds.max.z / 2 - 0.5f;
            Vector3 spawnPoint = new Vector3(spawnMidPoint.x, spawnMidPoint.y, Random.Range(spawnMidPoint.z - range, spawnMidPoint.z + range));
            currentEnemyType = Random.Range(0, wave.enemyTypes.Length);
            Instantiate(wave.enemyTypes[currentEnemyType], spawnPoint, transform.rotation);
            remainedEnemy++;
            if (GameObject.FindGameObjectsWithTag("Enemy").Length /3 >= maxEnemiesInSingleScene)
            {
                yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length <= maxEnemiesInSingleScene);
            }
            else
            {
                yield return new WaitForSeconds(currentSpawnDelay);
            }
        }
        if (wave.hasBoss)
        {
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Enemy").Length==0);
        }
        Debug.Log("Wave has ended.");
        StartCoroutine(SpawnBoss(wave));
        //StopWave();
    }

    public void StopWave()
    {
        if(currentWaveID < waves.Length)
        {
            waves[currentWaveID].isActive = false;
        }
        canMoveText.gameObject.SetActive(true);
        remainedEnemy = 0;
        isAnyWaveActive = false;
        isPlayerinBattleZone = false;

        Debug.Log("Next Wave: " + currentWaveID + "is ready.");
        foreach (var obj in GameObject.FindGameObjectsWithTag("Wasted"))
        {
            Destroy(obj);
        }
        currentWaveID++;
    }

    public void Update()
    {
        if (isPlayerinBattleZone && currentWaveID < waves.Length)
        {
            if(!EnemyOnScene())
            {
                Debug.Log("Next Wave: " + currentWaveID + "is ready.");
                foreach (var obj in GameObject.FindGameObjectsWithTag("Wasted"))
                {
                    Destroy(obj);
                }
                isAnyWaveActive = false;
                isPlayerinBattleZone = false;
                StopWave();
                remainedEnemy = 0;
                currentWaveID++;
            }
        }
        else if (currentWaveID >= waves.Length)
        {
            Debug.Log("Finish the game!");
            if(GameObject.FindGameObjectsWithTag("WavePoints").Length > 0)
            {
                foreach (GameObject fixcams in GameObject.FindGameObjectsWithTag("WavePoints"))
                {
                    fixcams.SetActive(false);
                }
            }

        }
    }

    private bool EnemyOnScene()
    {
        if(remainedEnemy == (waves[currentWaveID].hasBoss ? waves[currentWaveID].maxEnemiesinScene+1 : waves[currentWaveID].maxEnemiesinScene))
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }
}
