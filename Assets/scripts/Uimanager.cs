using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject startScreen;
    public GameObject gameScreen;
    public GameObject gameOverScreen;
    public GameObject waveBanner;

    [Header("Start Screen")]
    public TMP_InputField nameInput;
    public Button startButton;
    public TextMeshProUGUI errorText;

    [Header("Game HUD")]
    public TextMeshProUGUI hudPlayerName;
    public TextMeshProUGUI hudScore;
    public TextMeshProUGUI hudWave;
    public TextMeshProUGUI hudLives;
    public TextMeshProUGUI hudKills;
    public TextMeshProUGUI waveBannerText;

    [Header("Game Over Screen")]
    public TextMeshProUGUI goPlayerName;
    public TextMeshProUGUI goFinalScore;
    public TextMeshProUGUI goWaveReached;
    public TextMeshProUGUI goTotalKills;
    public TextMeshProUGUI goRanking;  
    public Button restartButton;
    public Button mainMenuButton;
    public Button dashboardButton; 

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ShowScreen(startScreen);

        startButton.onClick.AddListener(OnStartPressed);
        restartButton.onClick.AddListener(OnRestartPressed);
        mainMenuButton.onClick.AddListener(OnMainMenuPressed);
        dashboardButton.onClick.AddListener(() =>{Application.OpenURL("https://laboratoriofirebaseunity.web.app");});

        GameManager.Instance.OnPlayerNameSet += UpdatePlayerName;
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnWaveChanged += UpdateWave;
        GameManager.Instance.OnKillsChanged += UpdateKills;
        GameManager.Instance.OnGameOver += ShowGameOver;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnPlayerNameSet -= UpdatePlayerName;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnWaveChanged -= UpdateWave;
        GameManager.Instance.OnKillsChanged -= UpdateKills;
        GameManager.Instance.OnGameOver -= ShowGameOver;
    }

void OnStartPressed()
{
    string playerName = nameInput.text.Trim();
    if (string.IsNullOrEmpty(playerName))
    {
        errorText.text = "Ingresa un nombre para continuar";
        return;
    }
    errorText.text = "";
    FindFirstObjectByType<FirestoreManager>().ResetSession(); // ← cambio
    ShowScreen(gameScreen);
    GameManager.Instance.StartGame(playerName);
}

void OnRestartPressed()
{
    FindFirstObjectByType<FirestoreManager>().ResetSession(); // ← cambio
    ShowScreen(gameScreen);
    GameManager.Instance.RestartGame();
}

void OnMainMenuPressed()
{
    FindFirstObjectByType<FirestoreManager>().ResetSession(); // ← cambio
    EnemySpawner.Instance.ClearEnemies();
    GameManager.Instance.StopAllCoroutines();
    waveBanner.SetActive(false);
    nameInput.text = "";
    errorText.text = "";
    ShowScreen(startScreen);
}

    void UpdatePlayerName(string playerName) => hudPlayerName.text = playerName;
    void UpdateScore(int score) => hudScore.text = score.ToString("N0");
    void UpdateLives(int lives) => hudLives.text = GetLivesString(lives);
    void UpdateKills(int kills) => hudKills.text = kills.ToString();

    void UpdateWave(int wave)
    {
        hudWave.text = "Oleada " + wave;
        StartCoroutine(ShowWaveBanner(wave));
    }

    string GetLivesString(int lives)
    {
        string result = "";
        for (int i = 0; i < GameManager.Instance.maxLives; i++)
            result += i < lives ? "♥ " : "♡ ";
        return result.Trim();
    }

    IEnumerator ShowWaveBanner(int wave)
    {
        waveBannerText.text = "OLEADA " + wave;
        waveBanner.SetActive(true);
        yield return new WaitForSeconds(2f);
        waveBanner.SetActive(false);
    }

async void ShowGameOver()
{
    goPlayerName.text  = GameManager.Instance.PlayerName;
    goFinalScore.text  = GameManager.Instance.Score.ToString("N0");
    goWaveReached.text = "Oleada " + GameManager.Instance.Wave;
    goTotalKills.text  = GameManager.Instance.Kills + " enemigos eliminados";

    ShowScreen(gameOverScreen);

    // Cargar ranking desde Firebase
    goRanking.text = "Cargando ranking...";
    var firestoreManager = FindFirstObjectByType<FirestoreManager>();
    var highscores = await firestoreManager.GetHighscores(3);

    string rankingText = "--- TOP 3 ---\n";
    string[] medals = { "1.", "2.", "3."};

    for (int i = 0; i < highscores.Count; i++)
    {
        var entry = highscores[i];
        string name  = entry.TryGetValue("playerName", out var n) ? n.ToString() : "?";
        string score = entry.TryGetValue("score", out var s) ? ((long)s).ToString("N0") : "0";
        rankingText += $"{medals[i]} {name} — {score}\n";
    }

    goRanking.text = rankingText;
}

    void ShowScreen(GameObject screen)
    {
        startScreen.SetActive(false);
        gameScreen.SetActive(false);
        gameOverScreen.SetActive(false);
        screen.SetActive(true);
    }
}