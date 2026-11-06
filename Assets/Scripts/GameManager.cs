//koden skriven av Leevi
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

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

    [Header("Nivåer")]
    [Tooltip("Spelas i ordning. Tom lista = oändliga vågor.")]
    public LevelData[] levels;
    public float bossDelay = 2f;

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
    public TMP_Text coinsText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject levelCompletePanel;
    public Slider bossHealthBar;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int MaxLives { get; private set; }
    public int Wave { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsWon { get; private set; }
    public int LevelNumber => levelIndex + 1;

    const string HighScoreKey = "HighScore";
    int highScore;
    bool extraLifeGiven;
    float ufoTimer;
    Ufo currentUfo;
    float barrelTimer;
    LevelData level;
    int levelIndex;
    int shownCoins = -1;

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
        SetupLevel();
        Lives = startLives + PlayerProgress.GetUpgrade(UpgradeType.ExtraLife);
        MaxLives = Lives;
        ufoTimer = Random.Range(ufoMinDelay, ufoMaxDelay);
        Time.timeScale = 1f;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);
        StartNextWave();
    }

    void SetupLevel()
    {
        if (levels == null || levels.Length == 0) return;

        levelIndex = Mathf.Clamp(PlayerProgress.Level - 1, 0, levels.Length - 1);
        level = levels[levelIndex];
        swarm.rows = level.rows;
        swarm.columns = level.columns;
        barrelTimer = Random.Range(level.barrelMinDelay, level.barrelMaxDelay);
    }

    void Update()
    {
        UpdateCoinsText();

        if (IsGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        HandleUfo();
        HandleBarrels();
    }

    void StartNextWave()
    {
        Wave++;
        swarm.SpawnWave(Wave + (level != null ? level.difficulty : 0));

        if (Wave > 1 && rebuildBunkersEachWave && bunkers != null)
            foreach (Bunker b in bunkers) if (b != null) b.Build();

        UpdateUI();
    }

    public void OnWaveCleared()
    {
        if (IsGameOver) return;
        if (level != null && Wave >= level.waves)
        {
            if (level.bossPrefab != null) StartCoroutine(BossRoutine());
            else CompleteLevel();
            return;
        }
        StartCoroutine(NextWaveRoutine());
    }

    IEnumerator NextWaveRoutine()
    {
        Projectile.ClearAll();
        yield return new WaitForSeconds(nextWaveDelay);
        if (!IsGameOver) StartNextWave();
    }

    IEnumerator BossRoutine()
    {
        Projectile.ClearAll();
        yield return new WaitForSeconds(bossDelay);
        if (IsGameOver) yield break;

        BossController boss = Instantiate(level.bossPrefab);
        boss.Init(level.bossHp);
    }

    public void OnBossHealthChanged(int hp, int maxHp)
    {
        if (bossHealthBar == null) return;
        bossHealthBar.gameObject.SetActive(true);
        bossHealthBar.value = (float)hp / maxHp;
    }

    public void OnBossDefeated()
    {
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);
        CompleteLevel();
    }

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
        swarm.SetPaused(true);
        Projectile.ClearAll();

        yield return new WaitForSeconds(respawnDelay);
        if (IsGameOver) yield break;

        player.Respawn();
        swarm.SetPaused(false);
    }

    public void OnInvasion()
    {
        if (IsGameOver) return;
        Lives = 0;
        GameOver();
    }

    public void AddScore(int points)
    {
        if (IsGameOver) return;

        Score += points;
        if (Score > highScore) highScore = Score;

        if (!extraLifeGiven && extraLifeAtScore > 0 && Score >= extraLifeAtScore)
        {
            extraLifeGiven = true;
            Lives++;
            MaxLives = Mathf.Max(MaxLives, Lives);
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

    void CompleteLevel()
    {
        IsGameOver = true;
        IsWon = levels == null || levelIndex >= levels.Length - 1;
        swarm.SetPaused(true);
        Projectile.ClearAll();

        if (level != null) PlayerProgress.AddCoins(level.coinBonus);
        PlayerProgress.Level = IsWon ? 1 : LevelNumber + 1;

        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();

        GameObject panel = IsWon ? winPanel : levelCompletePanel;
        if (panel != null) panel.SetActive(true);
        UpdateUI();
    }

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

    void HandleBarrels()
    {
        if (level == null || level.barrelPrefab == null) return;
        if (swarm.IsPaused || !player.IsAlive) return;

        barrelTimer -= Time.deltaTime;
        if (barrelTimer > 0f) return;

        barrelTimer = Random.Range(level.barrelMinDelay, level.barrelMaxDelay);
        Instantiate(level.barrelPrefab).DropFromTop();
    }

    void UpdateCoinsText()
    {
        if (coinsText == null || shownCoins == PlayerProgress.Coins) return;
        shownCoins = PlayerProgress.Coins;
        coinsText.text = "COINS " + shownCoins;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "SCORE " + Score.ToString("D5");
        if (highScoreText != null) highScoreText.text = "HI " + highScore.ToString("D5");
        if (livesText != null) livesText.text = "LIVES " + Mathf.Max(0, Lives);
        if (waveText != null)
            waveText.text = level != null ? "LEVEL " + LevelNumber + "  WAVE " + Wave + "/" + level.waves : "WAVE " + Wave;
    }
}