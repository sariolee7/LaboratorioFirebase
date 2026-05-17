using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int maxLives = 3;

    public int Lives { get; private set; }
    public int Score { get; private set; }
    public int Wave { get; private set; }
    public int Kills { get; private set; }
    public bool IsRunning { get; private set; }
    public string PlayerName { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnWaveChanged;
    public event Action<int> OnKillsChanged;
    public event Action OnGameOver;
    public event Action<string> OnPlayerNameSet;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame(string playerName)
    {
        PlayerName = playerName;
        Lives = maxLives;
        Score = 0;
        Wave = 1;
        Kills = 0;
        IsRunning = true;

        OnPlayerNameSet?.Invoke(PlayerName);
        OnLivesChanged?.Invoke(Lives);
        OnScoreChanged?.Invoke(Score);
        OnWaveChanged?.Invoke(Wave);
        OnKillsChanged?.Invoke(Kills);

        EnemySpawner.Instance.StartWave(Wave);
    }

    public void AddScore(int points)
    {
        Score += points;
        OnScoreChanged?.Invoke(Score);
    }

    public void RegisterKill()
    {
        Kills++;
        OnKillsChanged?.Invoke(Kills);
    }

    public void EnemyReachedBase()
    {
        Lives--;
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0) TriggerGameOver();
    }

    public void WaveCompleted()
    {
        Wave++;
        OnWaveChanged?.Invoke(Wave);
        StartCoroutine(NextWaveDelay());
    }

    IEnumerator NextWaveDelay()
    {
        yield return new WaitForSeconds(3f);
        if (IsRunning) EnemySpawner.Instance.StartWave(Wave);
    }

    void TriggerGameOver()
    {
        IsRunning = false;
        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        EnemySpawner.Instance.ClearEnemies();
        StartGame(PlayerName);
    }
}