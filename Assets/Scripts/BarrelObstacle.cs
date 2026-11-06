using System.Collections.Generic;
using UnityEngine;

// En tunna som faller ner från toppen. Den kostar ett liv om den träffar spelaren,
// spräcker bunkerblock och ger spelaren en powerup om den skjuts sönder.
// GameManager skapar tunnorna.
public class BarrelObstacle : MonoBehaviour
{
    static readonly List<BarrelObstacle> Active = new List<BarrelObstacle>();

    public float fallSpeed = 2.5f;
    public int points = 25;
    [Tooltip("Avstånd från skärmkanten där tunnor inte dyker upp")]
    public float edgePadding = 0.6f;
    public GameObject explosionPrefab;

    BoxCollider2D box;
    float bottomLimit;
    bool spent;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    public void DropFromTop()
    {
        Camera cam = Camera.main;
        Vector3 min = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        bottomLimit = min.y - 1f;
        float x = Random.Range(min.x + edgePadding, max.x - edgePadding);
        transform.position = new Vector3(x, max.y + 1f, 0f);
    }

    void Update()
    {
        if (spent) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (transform.position.y < bottomLimit)
        {
            Destroy(gameObject);
            return;
        }

        CheckCollisions();
    }

    void CheckCollisions()
    {
        Vector2 center = (Vector2)transform.position + box.offset;
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, box.size, 0f))
        {
            PlayerController player = hit.GetComponentInParent<PlayerController>();
            if (player != null && player.TakeHit())
            {
                // Spelarens egen dödseffekt räcker här
                spent = true;
                Destroy(gameObject);
                return;
            }

            if (hit.TryGetComponent(out BunkerBlock block) && block.Hit())
            {
                Smash();
                return;
            }
        }
    }

    // Anropas av spelarens skott. Returnerar true om skottet räknas som en träff.
    public bool Hit()
    {
        if (spent) return false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            if (GameManager.Instance.player != null) GameManager.Instance.player.GivePowerUp();
        }
        Smash();
        return true;
    }

    void Smash()
    {
        spent = true;
        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);
        GameAudio.PlayExplosion();
        Destroy(gameObject);
    }

    public static void ClearAll()
    {
        foreach (BarrelObstacle barrel in Active.ToArray())
            if (barrel != null) Destroy(barrel.gameObject);
    }
}
