//koden skriven av Leevi
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button shopButton;
    public Button quitButton;
    public Button resetButton;

    [Header("Reset progress")]
    [Tooltip("Seconds the player has to press again to confirm")]
    public float resetConfirmTime = 3f;

    [Header("Scenes (must be in Build Profiles / Build Settings)")]
    public string gameSceneName = "Game";
    public string shopSceneName = "Shop";

    TMP_Text resetLabel;
    string resetLabelText;
    bool confirmingReset;
    Coroutine resetRoutine;

    void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (shopButton != null) shopButton.onClick.AddListener(Shop);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetProgress);
            resetLabel = resetButton.GetComponentInChildren<TMP_Text>();
            if (resetLabel != null) resetLabelText = resetLabel.text;
        }
    }

    public void ResetProgress()
    {
        if (resetRoutine != null) StopCoroutine(resetRoutine);

        if (confirmingReset)
        {
            confirmingReset = false;
            PlayerProgress.ResetAll();
            resetRoutine = StartCoroutine(ShowResetMessage("PROGRESS RESET"));
        }
        else
        {
            confirmingReset = true;
            resetRoutine = StartCoroutine(ShowResetMessage("ARE YOU SURE?"));
        }
    }

    IEnumerator ShowResetMessage(string message)
    {
        if (resetLabel != null) resetLabel.text = message;
        yield return new WaitForSecondsRealtime(resetConfirmTime);

        if (resetLabel != null) resetLabel.text = resetLabelText;
        confirmingReset = false;
        resetRoutine = null;
    }

    public void Play()
    {
        LoadScene(gameSceneName);
    }

    public void Shop()
    {
        LoadScene(shopSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void LoadScene(string sceneName)
    {
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning("MainMenu: scene '" + sceneName + "' is not in the build scene list.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
