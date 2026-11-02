using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject normalZombiePrefab;
    public GameObject heavyZombiePrefab;
    public GameObject bossPrefab; // Bonnie Blue
    public GameObject johnnySinsPrefab; // Ny Boss

    public float spawnRate = 2f;
    public int wavesBeforeBonnie = 10;
    public int wavesBeforeJohnny = 20;

    private float nextSpawn = 0f;
    private int spawnedCount = 0;
    private bool bonnieSpawned = false;
    private bool johnnySpawned = false;

    void Update()
    {
        if (Time.time > nextSpawn)
        {
            nextSpawn = Time.time + spawnRate;
            spawnedCount++;

            if (spawnedCount == wavesBeforeJohnny && !johnnySpawned)
            {
                SpawnJohnnySins();
            }
            else if (spawnedCount == wavesBeforeBonnie && !bonnieSpawned)
            {
                SpawnBonnieBlue();
            }
            else if (!bonnieSpawned || (bonnieSpawned && !johnnySpawned && spawnedCount < wavesBeforeJohnny))
            {
                // Fortsätt spawna zombies mellan bossarna
                SpawnRandomEnemy();
            }
        }
    }

    void SpawnRandomEnemy()
    {
        float spawnX = Random.Range(0, 2) == 0 ? -11f : 11f;
        float spawnY = Random.Range(-4.5f, 4.5f);
        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        GameObject prefabToSpawn = Random.value > 0.3f ? normalZombiePrefab : heavyZombiePrefab;
        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    void SpawnBonnieBlue()
    {
        bonnieSpawned = true;
        Debug.Log("BOSSINVASION! Bonnie Blue har dykt upp!");
        Instantiate(bossPrefab, new Vector2(11f, 0f), Quaternion.identity);
    }

    void SpawnJohnnySins()
    {
        johnnySpawned = true;
        Debug.Log("FINAL BOSS! Johnny Sins anländer!");
        Instantiate(johnnySinsPrefab, new Vector2(-11f, 0f), Quaternion.identity); // Kommer in från vänster
    }
}