using UnityEngine;

// Det mystiska skeppet som flyger över toppen ibland och ger bonuspoäng.
// GameManager skapar det.
public class Ufo : MonoBehaviour
{
    public float speed = 3f;
    public int[] possiblePoints = { 50, 100, 150, 300 };
    public int coinReward = 50;
    public GameObject explosionPrefab;
    public AudioClip deathSound;

    int direction = 1;
    float leftLimit, rightLimit;
    bool dead;

    public void Launch(int dir, float height)
    {
        Camera cam = Camera.main;
        leftLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x - 1f;
        rightLimit = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x + 1f;

        direction = dir;
        transform.position = new Vector3(dir > 0 ? leftLimit : rightLimit, height, 0f);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        float x = transform.position.x;
        if (x < leftLimit - 0.5f || x > rightLimit + 0.5f) Destroy(gameObject);
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
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);

        Destroy(gameObject);
        return true;
    }
}