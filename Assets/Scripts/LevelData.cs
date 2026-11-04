using UnityEngine;

// En nivå: vilka fiender, hur många vågor och vilken boss.
// Skapa fler med Assets > Create > Game > Level och lägg dem i GameManager.
[CreateAssetMenu(menuName = "Game/Level", fileName = "Level")]
public class LevelData : ScriptableObject
{
    [Header("Fiender (element 0 = översta raden)")]
    public EnemySwarm.EnemyRow[] rows;
    public int columns = 8;

    [Header("Vågor")]
    public int waves = 2;
    [Tooltip("Läggs på vågnumret: högre = snabbare fiender som skjuter oftare")]
    public int difficulty = 0;

    [Header("Boss (tom = ingen boss)")]
    public BossController bossPrefab;
    public int bossHp = 20;

    [Header("Belöning")]
    [Tooltip("Mynt för att klara nivån")]
    public int coinBonus = 100;
}
