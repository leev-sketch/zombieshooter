//koden skriven av Leevi
using UnityEngine;

public class Ufo : MonoBehaviour
{
    public float speed = 3f;
    public int[] possiblePoints = { 50, 100, 150, 300 };
    public int coinReward = 50;
    public GameObject explosionPrefab;
    public Projectile shotPrefab;
    public float shootInterval = 1.2f;
    public float shotOffsetY = -0.6f;

    int direction = 1;
    float leftLimit, rightLimit;
    bool dead;
    float shootTimer;

    public void Launch(int dir, float height)
    {
        Camera cam = Camera.main;
        leftLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x - 1f;
        rightLimit = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x + 1f;

        direction = dir;
        shootTimer = shootInterval * 0.5f;
        GetComponent<SpriteRenderer>().flipX = dir < 0;
        transform.position = new Vector3(dir > 0 ? leftLimit : rightLimit, height, 0f);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        float x = transform.position.x;
        if (x < leftLimit - 0.5f || x > rightLimit + 0.5f) Destroy(gameObject);

        HandleShooting(x);
    }

    void HandleShooting(float x)
    {
        if (shotPrefab == null || x < leftLimit + 1f || x > rightLimit - 1f) return;
        if (GameManager.Instance != null && GameManager.Instance.player != null && !GameManager.Instance.player.IsAlive) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        shootTimer = shootInterval;
        Instantiate(shotPrefab, transform.position + Vector3.up * shotOffsetY, Quaternion.identity);
        GameAudio.PlayCannon();
    }

    public bool Hit()
    {
        if (dead) return false;
        dead = true;

        int points = possiblePoints[Random.Range(0, possiblePoints.Length)];
        if (GameManager.Instance != null) GameManager.Instance.AddScore(points);
        if (MainManager.Instance != null) MainManager.Instance.AddCoins(coinReward);
        Debug.Log("UFO träffat! +" + points);

        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);
        GameAudio.PlayExplosion();

        Destroy(gameObject);
        return true;
    }
}