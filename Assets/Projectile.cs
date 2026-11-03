using System.Collections.Generic;
using UnityEngine;

// Ett skott för BÅDE spelaren och fienderna (ersätter EnemyProjectile).
// Skottet kollar med en raycast vad som finns mellan förra och nya positionen,
// så snabba skott "hoppar" aldrig över små saker som bunkerblock.
// Det sköter själv all skada, så inga taggar eller Rigidbody2D behövs.
public class Projectile : MonoBehaviour
{
    public static readonly List<Projectile> Active = new List<Projectile>();

    [Tooltip("I = spelarens skott (åker upp). Ur = fiendens skott (åker ner).")]
    public bool fromPlayer = true;
    public float speed = 10f;
    public int damage = 1;

    Collider2D myCollider;
    float topLimit, bottomLimit;
    bool spent;

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        Camera cam = Camera.main;
        topLimit = cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y + 1f;
        bottomLimit = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y - 1f;
    }

    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }

    void Update()
    {
        if (spent) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        Vector2 dir = fromPlayer ? Vector2.up : Vector2.down;
        float distance = speed * Time.deltaTime;
        Vector2 start = transform.position;

        foreach (RaycastHit2D hit in Physics2D.RaycastAll(start, dir, distance))
        {
            if (hit.collider == myCollider) continue;
            if (TryHit(hit.collider))
            {
                Consume();
                return;
            }
        }

        transform.position += (Vector3)(dir * distance);

        float y = transform.position.y;
        if (y > topLimit || y < bottomLimit) Destroy(gameObject);
    }

    bool TryHit(Collider2D other)
    {
        if (fromPlayer)
        {
            EnemyController enemy = other.GetComponentInParent<EnemyController>();
            if (enemy != null) return enemy.TakeDamage(damage);

            Ufo ufo = other.GetComponentInParent<Ufo>();
            if (ufo != null) return ufo.Hit();
        }
        else
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null) return player.TakeHit();
        }

        BunkerBlock block = other.GetComponent<BunkerBlock>();
        if (block != null) return block.Hit();

        // Spelarens och fiendens skott kan ta ut varandra
        Projectile shot = other.GetComponent<Projectile>();
        if (shot != null && !shot.spent && shot.fromPlayer != fromPlayer)
        {
            shot.Consume();
            return true;
        }

        return false;
    }

    void Consume()
    {
        spent = true;
        Destroy(gameObject);
    }

    // ---------- Hjälpfunktioner ----------

    public static int CountShots(bool playerShots)
    {
        int count = 0;
        foreach (Projectile p in Active)
            if (p != null && !p.spent && p.fromPlayer == playerShots) count++;
        return count;
    }

    public static void ClearAll()
    {
        foreach (Projectile p in Active.ToArray())
            if (p != null) Destroy(p.gameObject);
    }
}