using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirestoreManager : MonoBehaviour
{
    FirebaseFirestore db;

    // ── Flag para evitar guardar dos veces ──
    bool sessionSaved = false;

    // ── Métricas que se acumulan durante la partida ──
    float sessionStartTime;
    Dictionary<string, int> killsPerWave = new Dictionary<string, int>();
    int killsThisWave = 0;
    int currentWaveTracked = 1;

    // Para avgReactionTime
    float lastEnemySpawnTime = 0f;
    List<float> reactionTimes = new List<float>();

async void Start()
{
    db = FirebaseFirestore.DefaultInstance;
    await Task.Yield();

    sessionStartTime = Time.time;

    GameManager.Instance.OnGameOver     += HandleGameOver;
    GameManager.Instance.OnWaveChanged  += HandleWaveChanged;
    GameManager.Instance.OnKillsChanged += HandleKillsChanged;
}

void OnDestroy()
{
    if (GameManager.Instance == null) return;
    GameManager.Instance.OnGameOver     -= HandleGameOver;
    GameManager.Instance.OnWaveChanged  -= HandleWaveChanged;
    GameManager.Instance.OnKillsChanged -= HandleKillsChanged;
}

    void OnApplicationQuit()
    {
        if (GameManager.Instance == null) return;
        if (string.IsNullOrEmpty(GameManager.Instance.PlayerName)) return;
        if (sessionSaved) return; 
        _ = SaveSession();
    }

    void HandleWaveChanged(int newWave)
    {
        killsPerWave[currentWaveTracked.ToString()] = killsThisWave;
        killsThisWave = 0;
        currentWaveTracked = newWave;
        lastEnemySpawnTime = Time.time;
    }

    void HandleKillsChanged(int totalKills)
    {
        killsThisWave++;

        if (lastEnemySpawnTime > 0f)
        {
            float reaction = Time.time - lastEnemySpawnTime;
            reactionTimes.Add(reaction);
            lastEnemySpawnTime = Time.time;
        }
    }

    async void HandleGameOver()
    {
    if (sessionSaved) return;
    killsPerWave[currentWaveTracked.ToString()] = killsThisWave;
    await SaveSession();
    await SaveHighscore(GameManager.Instance.PlayerName, GameManager.Instance.Score); // ← agregar
    }

    // Resetea todo para una nueva partida
    public void ResetSession()
    {
        sessionSaved       = false;
        sessionStartTime   = Time.time;
        killsPerWave       = new Dictionary<string, int>();
        killsThisWave      = 0;
        currentWaveTracked = 1;
        lastEnemySpawnTime = 0f;
        reactionTimes      = new List<float>();
        Debug.Log("FirestoreManager: sesión reseteada");
    }

    public async Task SaveSession()
    {
        if (sessionSaved) return; // ← bloquea cualquier llamada extra
        sessionSaved = true;

        string playerName = GameManager.Instance.PlayerName;
        int score         = GameManager.Instance.Score;
        int wave          = GameManager.Instance.Wave;
        int kills         = GameManager.Instance.Kills;
        float duration    = Time.time - sessionStartTime;

        float avgReaction = 0f;
        if (reactionTimes.Count > 0)
        {
            float total = 0f;
            foreach (float t in reactionTimes) total += t;
            avgReaction = total / reactionTimes.Count;
        }

        DocumentReference playerRef = db
            .Collection("players")
            .Document(playerName);

        await playerRef.SetAsync(new Dictionary<string, object>
        {
            { "playerName", playerName },
            { "lastSeen",   Timestamp.GetCurrentTimestamp() }
        }, SetOptions.MergeAll);

        CollectionReference sessionsRef = playerRef.Collection("sessions");

        Dictionary<string, object> session = new Dictionary<string, object>
        {
            { "playerName",      playerName },
            { "startTime",       Timestamp.GetCurrentTimestamp() },
            { "duration",        Mathf.Round(duration) },
            { "finalScore",      score },
            { "waveReached",     wave },
            { "totalKills",      kills },
            { "killsPerWave",    killsPerWave },
            { "killsPerMinute",  kills / (duration / 60f) },
            { "avgReactionTime", avgReaction },
        };

        await sessionsRef.AddAsync(session);
        Debug.Log($"Sesión guardada para: {playerName}");
    }

    public async Task SaveHighscore(string playerName, int score)
{
    CollectionReference highscoresRef = db.Collection("highscores");

    Dictionary<string, object> highscore = new Dictionary<string, object>
    {
        { "playerName", playerName },
        { "score",      score },
        { "date",       Timestamp.GetCurrentTimestamp() }
    };

    await highscoresRef.AddAsync(highscore);
    Debug.Log($"Highscore guardado: {playerName} - {score}");
}

public async Task<List<Dictionary<string, object>>> GetHighscores(int limit = 3)
{
    CollectionReference highscoresRef = db.Collection("highscores");
    Query query = highscoresRef.OrderByDescending("score").Limit(limit);
    QuerySnapshot snapshot = await query.GetSnapshotAsync();

    List<Dictionary<string, object>> results = new List<Dictionary<string, object>>();
    foreach (DocumentSnapshot doc in snapshot.Documents)
        results.Add(doc.ToDictionary());

    return results;
}

}