using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button shopButton;
    public Button quitButton;

    [Header("Scenes (must be in Build Profiles / Build Settings)")]
    public string gameSceneName = "Game";
    public string shopSceneName = "Shop";

    void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (shopButton != null) shopButton.onClick.AddListener(Shop);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
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
