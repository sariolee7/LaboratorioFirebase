using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    FirebaseFirestore db;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        // Escucha el game over del GameManager para guardar automáticamente
        GameManager.Instance.OnGameOver += HandleGameOver;
    }
    void OnDestroy()
    {
        GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    async void HandleGameOver()
    {
        // Lee el nombre y stats directamente del GameManager, igual que UIManager
        string playerName = GameManager.Instance.PlayerName;
        int score         = GameManager.Instance.Score;
        int wave          = GameManager.Instance.Wave;
        int kills         = GameManager.Instance.Kills;

        await SaveSession(playerName, score, wave, kills);
    }

    public async Task SaveSession(string playerName, int score, int wave, int kills)
    {
        CollectionReference sessionsRef = db
            .Collection("players")
            .Document(playerName)
            .Collection("sessions");

        Dictionary<string, object> session = new Dictionary<string, object>
        {
            { "playerName",  playerName },
            { "startTime",   Timestamp.GetCurrentTimestamp() },
            { "finalScore",  score },
            { "waveReached", wave },
            { "totalKills",  kills },
            // tus métricas adicionales aquí
        };

        await sessionsRef.AddAsync(session);
        Debug.Log($"Sesión guardada para: {playerName}");
    }
}
