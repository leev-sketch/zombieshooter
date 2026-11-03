using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Håller koll på poäng, liv, vågor, UFO och game over.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referenser")]
    public EnemySwarm swarm;
    public PlayerController player;
    public Bunker[] bunkers;

    [Header("Regler")]
    public int startLives = 3;
    [Tooltip("Extraliv vid denna poäng (0 = av). Originalet: 1500.")]
    public int extraLifeAtScore = 1500;
    public float respawnDelay = 1.5f;
    public float nextWaveDelay = 2f;
    public bool rebuildBunkersEachWave = true;

    [Header("UFO")]
    public Ufo ufoPrefab;
    public float ufoMinDelay = 15f;
    public float ufoMaxDelay = 30f;
    public float ufoHeight = 4.3f;
    [Tooltip("UFO dyker bara upp när minst så här många fiender lever")]
    public int ufoMinEnemiesLeft = 8;

    [Header("UI (valfritt, TextMeshPro)")]
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text livesText;
    public TMP_Text waveText;
    public GameObject gameOverPanel;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Wave { get; private set; }
    public bool IsGameOver { get; private set; }

    const string HighScoreKey = "HighScore";
    int highScore;
    bool extraLifeGiven;
    float ufoTimer;
    Ufo currentUfo;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Lives = startLives;
        ufoTimer = Random.Range(ufoMinDelay, ufoMaxDelay);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        StartNextWave();
    }

    void Update()
    {
        if (IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        HandleUfo();
    }

    // ---------- Vågor ----------

    void StartNextWave()
    {
        Wave++;
        swarm.SpawnWave(Wave);

        // Bunkrarna bygger sig själva i början, sedan byggs de om varje ny våg
        if (Wave > 1 && rebuildBunkersEachWave && bunkers != null)
            foreach (Bunker b in bunkers) if (b != null) b.Build();

        UpdateUI();
    }

    public void OnWaveCleared()
    {
        if (IsGameOver) return;
        StartCoroutine(NextWaveRoutine());
    }

    IEnumerator NextWaveRoutine()
    {
        Projectile.ClearAll();
        yield return new WaitForSeconds(nextWaveDelay);
        if (!IsGameOver) StartNextWave();
    }

    // ---------- Spelaren ----------

    public void OnPlayerDied()
    {
        if (IsGameOver) return;

        Lives--;
        UpdateUI();

        if (Lives <= 0) GameOver();
        else StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        // Som i originalet: allt fryser medan spelaren exploderar
        swarm.SetPaused(true);
        Projectile.ClearAll();

        yield return new WaitForSeconds(respawnDelay);
        if (IsGameOver) yield break;

        player.Respawn();
        swarm.SetPaused(false);
    }

    // Fienderna nådde marken = game over direkt, oavsett liv
    public void OnInvasion()
    {
        if (IsGameOver) return;
        Lives = 0;
        GameOver();
    }

    // ---------- Poäng ----------

    public void AddScore(int points)
    {
        if (IsGameOver) return;

        Score += points;
        if (Score > highScore) highScore = Score;

        if (!extraLifeGiven && extraLifeAtScore > 0 && Score >= extraLifeAtScore)
        {
            extraLifeGiven = true;
            Lives++;
        }

        UpdateUI();
    }

    void GameOver()
    {
        IsGameOver = true;
        swarm.SetPaused(true);

        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        UpdateUI();
        Debug.Log("Game Over! Poäng: " + Score + ". Tryck R för att börja om.");
    }

    // ---------- UFO ----------

    void HandleUfo()
    {
        if (ufoPrefab == null || currentUfo != null) return;
        if (swarm.IsPaused || swarm.AliveCount < ufoMinEnemiesLeft) return;

        ufoTimer -= Time.deltaTime;
        if (ufoTimer > 0f) return;

        ufoTimer = Random.Range(ufoMinDelay, ufoMaxDelay);
        int dir = Random.value < 0.5f ? 1 : -1;
        currentUfo = Instantiate(ufoPrefab);
        currentUfo.Launch(dir, ufoHeight);
    }

    // ---------- UI ----------

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE " + Score.ToString("D5");
        if (highScoreText != null) highScoreText.text = "HI " + highScore.ToString("D5");
        if (livesText != null) livesText.text = "LIVES " + Mathf.Max(0, Lives);
        if (waveText != null) waveText.text = "WAVE " + Wave;
    }
}