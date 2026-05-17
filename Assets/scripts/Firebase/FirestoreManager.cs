using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    FirebaseFirestore db;

    async void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("FirestoreManager: Firestore inicializado");

        // Espera a que GameManager esté listo
        await Task.Yield();

        GameManager.Instance.OnGameOver += HandleGameOver;
        Debug.Log("FirestoreManager: suscrito a OnGameOver");
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= HandleGameOver;
    }

    async void HandleGameOver()
    {
        string playerName = GameManager.Instance.PlayerName;
        int score         = GameManager.Instance.Score;
        int wave          = GameManager.Instance.Wave;
        int kills         = GameManager.Instance.Kills;

        await SaveSession(playerName, score, wave, kills);
    }

    public async Task SaveSession(string playerName, int score, int wave, int kills)
    {
        // Igual que el ejemplo: db.Collection().Document()
        // El nombre del jugador es el Document, sessions es la subcolección
        DocumentReference playerRef = db
            .Collection("players")
            .Document(playerName);

        // Crea o actualiza el documento del jugador
        Dictionary<string, object> playerData = new Dictionary<string, object>
        {
            { "playerName", playerName },
            { "lastSeen",   Timestamp.GetCurrentTimestamp() }
        };

        await playerRef.SetAsync(playerData, SetOptions.MergeAll);
        Debug.Log($"Jugador creado/actualizado: {playerName}");

        // Igual que AddDataToCollection del ejemplo pero en la subcolección sessions
        CollectionReference sessionsRef = playerRef.Collection("sessions");

        Dictionary<string, object> session = new Dictionary<string, object>
        {
            { "playerName",  playerName },
            { "startTime",   Timestamp.GetCurrentTimestamp() },
            { "finalScore",  score },
            { "waveReached", wave },
            { "totalKills",  kills },
        };

        await sessionsRef.AddAsync(session);
        Debug.Log($"Sesión guardada para: {playerName}");
    }
}