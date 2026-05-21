using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
        public int cost;
    }

    [Header("Бюджет Атакуючого")]
    public List<EnemyType> availableEnemies = new List<EnemyType>();
    public int startAttackBudget = 100;
    public int budgetIncreasePerWave = 50;
    public int maxEnemiesPerWave = 50;

    [Header("UI та Налаштування")]
    public TextMeshProUGUI waveText;
    public float spawnInterval = 1f;
    public Transform[] waypoints;

    public static int enemiesAlive = 0;
    private int currentWaveIndex = 0;

    public void StartNextWave()
    {
        if (GameManager.instance.currentState != GameManager.GameState.Preparation) return;

        enemiesAlive = 0;

        GameManager.instance.ChangeState(GameManager.GameState.Battle);
        StartCoroutine(GenerateAndSpawnWave());
    }

    IEnumerator GenerateAndSpawnWave()
    {
        currentWaveIndex++;

        if (waveText != null)
        {
            waveText.text = "Wave: " + currentWaveIndex;
        }

        int currentBudget = startAttackBudget + (currentWaveIndex - 1) * budgetIncreasePerWave;

        List<GameObject> waveQueue = new List<GameObject>();

        while (currentBudget >= 10 && waveQueue.Count < maxEnemiesPerWave)
        {
            EnemyType randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];

            if (randomEnemy != null && randomEnemy.prefab != null && currentBudget >= randomEnemy.cost)
            {
                waveQueue.Add(randomEnemy.prefab);
                currentBudget -= randomEnemy.cost;
            }
            else
            {
                bool canAffordAnything = false;

                foreach (EnemyType enemyType in availableEnemies)
                {
                    if (enemyType != null && enemyType.prefab != null && currentBudget >= enemyType.cost)
                    {
                        canAffordAnything = true;
                        break;
                    }
                }

                if (!canAffordAnything)
                {
                    break;
                }
            }
        }

        foreach (GameObject enemyPrefab in waveQueue)
        {
            SpawnEnemy(enemyPrefab);
            yield return new WaitForSeconds(spawnInterval);
        }

        while (enemiesAlive > 0)
        {
            yield return null;
        }

        if (currentWaveIndex < 10)
        {
            GameManager.instance.ChangeState(GameManager.GameState.RoundEnd);
        }
        else
        {
            GameManager.instance.WinGame();
        }
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;
        if (waypoints == null || waypoints.Length == 0) return;

        GameObject newEnemy;

        if (ObjectPool.instance != null)
        {
            newEnemy = ObjectPool.instance.GetObject(
                prefab,
                waypoints[0].position,
                Quaternion.identity
            );
        }
        else
        {
            newEnemy = Instantiate(prefab, waypoints[0].position, Quaternion.identity);
        }

        enemiesAlive++;

        EnemyMovement movement = newEnemy.GetComponent<EnemyMovement>();

        if (movement != null)
        {
            movement.waypoints = waypoints;
            movement.ResetEnemy();
        }
    }
}