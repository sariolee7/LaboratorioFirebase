using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
        public int minWave;
        [Range(0, 100)]
        public int spawnWeight = 50;
    }

    [Header("Enemy Types")]
    public List<EnemyType> enemyTypes;

    [Header("Spawn Config")]
    public Transform[] spawnPoints;
    public float baseSpawnInterval = 1.5f;
    public int baseEnemiesPerWave = 5;
    public int enemiesAddedPerWave = 2;

    private int enemiesAlive;
    private int enemiesToSpawn;
    private bool waveActive;
    private bool waveEnded;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartWave(int wave)
    {
        enemiesToSpawn = baseEnemiesPerWave + (wave - 1) * enemiesAddedPerWave;
        enemiesAlive = enemiesToSpawn;
        waveActive = true;
        waveEnded = false;

        StartCoroutine(SpawnRoutine(wave));
    }

    IEnumerator SpawnRoutine(int wave)
    {
        float interval = Mathf.Max(0.3f, baseSpawnInterval - (wave - 1) * 0.08f);
        List<EnemyType> pool = enemyTypes.FindAll(e => e.minWave <= wave);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (!GameManager.Instance.IsRunning) yield break;

            EnemyType chosen = PickWeightedRandom(pool);
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject obj = Instantiate(chosen.prefab, spawnPoint.position, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            enemy.speed += (wave - 1) * 0.1f;

            yield return new WaitForSeconds(interval);
        }

        waveActive = false;
        CheckWaveComplete();
    }

    EnemyType PickWeightedRandom(List<EnemyType> pool)
    {
        int total = 0;
        foreach (EnemyType e in pool) total += e.spawnWeight;

        int roll = Random.Range(0, total);
        int current = 0;

        foreach (EnemyType e in pool)
        {
            current += e.spawnWeight;
            if (roll < current) return e;
        }

        return pool[pool.Count - 1];
    }

    public void OnEnemyKilled()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        CheckWaveComplete();
    }

    void CheckWaveComplete()
    {
        if (!waveActive && enemiesAlive <= 0 && !waveEnded)
        {
            waveEnded = true;
            GameManager.Instance.WaveCompleted();
        }
    }

    public void ClearEnemies()
    {
        StopAllCoroutines();
        waveActive = false;
        waveEnded = true;
        enemiesAlive = 0;

        Enemy[] active = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in active) Destroy(e.gameObject);
    }
}