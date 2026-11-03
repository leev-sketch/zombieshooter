using UnityEngine;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance; // Gör att alla script kan nå denna
    public int totalCoins = 0;

    void Awake()
    {
        // Se till att det bara finns en MainManager
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddCoins(int amount)
    {
        totalCoins += amount;
        Debug.Log("Du har nu: " + totalCoins + " mynt!");
    }
}