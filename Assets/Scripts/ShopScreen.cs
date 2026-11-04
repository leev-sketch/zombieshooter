using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Butiken: köp uppgraderingar för mynten. Effekterna läses av PlayerController och GameManager.
public class ShopScreen : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public UpgradeType type;
        public string title;
        public string description;
        public int maxLevel = 5;
        public int baseCost = 150;
        [Tooltip("Priset ökar så här mycket för varje köpt nivå")]
        public int costPerLevel = 150;

        [Header("UI")]
        public Button buyButton;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public TMP_Text buyButtonText;

        public int Level => PlayerProgress.GetUpgrade(type);
        public bool IsMaxed => Level >= maxLevel;
        public int Cost => baseCost + costPerLevel * Level;
    }

    public ShopItem[] items;
    public TMP_Text coinsText;

    [Header("Knappar")]
    public Button playButton;
    public Button mainMenuButton;

    [Header("Scener")]
    public string gameSceneName = "Game";
    public string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        foreach (ShopItem item in items)
        {
            ShopItem current = item;
            if (current.buyButton != null) current.buyButton.onClick.AddListener(() => Buy(current));
        }
        if (playButton != null) playButton.onClick.AddListener(() => SceneManager.LoadScene(gameSceneName));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene(mainMenuSceneName));
    }

    void Start()
    {
        Refresh();
    }

    void Buy(ShopItem item)
    {
        if (item.IsMaxed) return;
        if (PlayerProgress.BuyUpgrade(item.type, item.Cost)) Refresh();
    }

    void Refresh()
    {
        if (coinsText != null) coinsText.text = "COINS " + PlayerProgress.Coins;

        foreach (ShopItem item in items)
        {
            if (item.titleText != null) item.titleText.text = item.title + "  " + item.Level + "/" + item.maxLevel;
            if (item.descriptionText != null) item.descriptionText.text = item.description;
            if (item.buyButtonText != null) item.buyButtonText.text = item.IsMaxed ? "MAX" : item.Cost + " COINS";
            if (item.buyButton != null) item.buyButton.interactable = !item.IsMaxed && PlayerProgress.Coins >= item.Cost;
        }
    }
}
