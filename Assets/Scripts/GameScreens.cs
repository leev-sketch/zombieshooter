//koden skriven av Umar
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameScreens : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Paneler")]
    public GameObject pausePanel;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject levelCompletePanel;

    [Header("Knappar")]
    public Button resumeButton;
    [Tooltip("Alla knappar som startar om spelet (paus, förlust, vinst)")]
    public Button[] restartButtons;
    [Tooltip("Alla knappar som går till huvudmenyn")]
    public Button[] mainMenuButtons;
    [Tooltip("Alla knappar som går till butiken")]
    public Button[] shopButtons;

    [Header("Poäng på slutskärmarna (valfritt)")]
    public TMP_Text loseScoreText;
    public TMP_Text winScoreText;
    public TMP_Text levelCompleteScoreText;

    [Header("Inställningar")]
    public string mainMenuSceneName = "MainMenu";
    public string shopSceneName = "Shop";
    public KeyCode[] pauseKeys = { KeyCode.Escape, KeyCode.P };

    void Awake()
    {
        SetPaused(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        foreach (Button b in restartButtons) if (b != null) b.onClick.AddListener(Restart);
        foreach (Button b in mainMenuButtons) if (b != null) b.onClick.AddListener(GoToMainMenu);
        foreach (Button b in shopButtons) if (b != null) b.onClick.AddListener(GoToShop);
    }

    void OnDestroy()
    {
        SetPaused(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null && gm.IsGameOver)
        {
            UpdateScoreTexts(gm);
            return;
        }

        foreach (KeyCode key in pauseKeys)
        {
            if (Input.GetKeyDown(key))
            {
                if (IsPaused) Resume();
                else Pause();
                break;
            }
        }
    }

    public void Pause()
    {
        SetPaused(true);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Resume()
    {
        SetPaused(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void Restart()
    {
        SetPaused(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void GoToShop()
    {
        SetPaused(false);
        SceneManager.LoadScene(shopSceneName);
    }

    void SetPaused(bool value)
    {
        IsPaused = value;
        Time.timeScale = value ? 0f : 1f;
        AudioListener.pause = value;
    }

    void UpdateScoreTexts(GameManager gm)
    {
        string text = "SCORE " + gm.Score.ToString("D5");
        if (loseScoreText != null) loseScoreText.text = text;
        if (winScoreText != null) winScoreText.text = text;
        if (levelCompleteScoreText != null) levelCompleteScoreText.text = text;
    }
}
