using UnityEngine;

public enum UpgradeType { ShotSpeed, ShipSpeed, ExtraLife, ExtraCannon, CannonPower }

// Sparar mynt, uppgraderingar och nivå mellan scener och spelomgångar.
public static class PlayerProgress
{
    const string CoinsKey = "Coins";
    const string LevelKey = "Level";
    const string UpgradePrefix = "Upgrade_";

    public static int Coins => PlayerPrefs.GetInt(CoinsKey, 0);

    // Nästa nivå att spela (1 = första)
    public static int Level
    {
        get => Mathf.Max(1, PlayerPrefs.GetInt(LevelKey, 1));
        set => PlayerPrefs.SetInt(LevelKey, Mathf.Max(1, value));
    }

    public static void AddCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, Coins + amount);
    }

    public static int GetUpgrade(UpgradeType type)
    {
        return PlayerPrefs.GetInt(UpgradePrefix + type, 0);
    }

    // Returnerar false om mynten inte räcker
    public static bool BuyUpgrade(UpgradeType type, int cost)
    {
        if (Coins < cost) return false;

        PlayerPrefs.SetInt(CoinsKey, Coins - cost);
        PlayerPrefs.SetInt(UpgradePrefix + type, GetUpgrade(type) + 1);
        Save();
        return true;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }

    // Börja om från noll (rekordet ligger kvar)
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(CoinsKey);
        PlayerPrefs.DeleteKey(LevelKey);
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
            PlayerPrefs.DeleteKey(UpgradePrefix + type);
        Save();
    }
}
