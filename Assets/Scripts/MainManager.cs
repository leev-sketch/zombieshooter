//koden skriven av Alex
using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public int TotalCoins => PlayerProgress.Coins;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        PlayerProgress.AddCoins(amount);
        Debug.Log("Du har nu: " + TotalCoins + " mynt!");
    }
}