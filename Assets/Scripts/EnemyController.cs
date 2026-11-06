using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

// En enskild fiende. Rörelse och "vem skjuter" sköts av EnemySwarm.
public class EnemyController : MonoBehaviour
{
    [Header("Liv & belöning")]
    [FormerlySerializedAs("hp")] // behåller ditt gamla värde från Inspector
    public int maxHp = 1;
    public int coinReward = 10;

    [Header("Skjuta")]
    [FormerlySerializedAs("enemyLaserPrefab")]
    public Projectile laserPrefab;
    public Transform firePoint;
    [Tooltip("Spela sjojungfruns ljud nar den skjuter")]
    public bool mermaidSound;

    [Header("Grafik & ljud (valfritt)")]
    [Tooltip("2 sprites som växlar för varje steg (klassisk 'gå'-animation)")]
    public Sprite[] animationFrames;
    public GameObject explosionPrefab;
    public Color hitFlashColor = Color.red;

    public int Column { get; private set; }

    int hp;
    int points = 10;
    int frame;
    bool dead;
    EnemySwarm swarm;
    SpriteRenderer sr;
    Collider2D col;
    Color normalColor = Color.white;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (sr != null) normalColor = sr.color;
        hp = maxHp;
    }

    public void Init(EnemySwarm owner, int column, int scoreValue)
    {
        swarm = owner;
        Column = column;
        points = scoreValue;
        hp = maxHp;
    }

    // Anropas av EnemySwarm varje gång just denna fiende tar ett steg
    public void OnStep()
    {
        if (sr != null && animationFrames != null && animationFrames.Length > 1)
        {
            frame = (frame + 1) % animationFrames.Length;
            sr.sprite = animationFrames[frame];
        }
        EatBunkers();
    }

    // Fiender som går in i en bunker förstör den, som i originalet
    void EatBunkers()
    {
        if (col == null) return;
        Vector2 center = (Vector2)transform.position + col.offset;
        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, col.bounds.size, 0f))
        {
            if (hit.TryGetComponent(out BunkerBlock block)) block.Hit(false);
        }
    }

    public void Shoot(float angle = 0f)
    {
        if (dead || laserPrefab == null) return;
        Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.down * 0.4f;
        Projectile shot = Instantiate(laserPrefab, pos, Quaternion.identity);
        shot.SetAngle(angle);
        if (mermaidSound) GameAudio.PlayMermaidShoot();
    }

    // Returnerar true om skottet räknas som en träff
    public bool TakeDamage(int damage)
    {
        if (dead) return false;

        hp -= damage;
        if (hp > 0)
        {
            if (sr != null) StartCoroutine(HitFlash());
        }
        else
        {
            Die();
        }
        return true;
    }

    IEnumerator HitFlash()
    {
        sr.color = hitFlashColor;
        yield return new WaitForSeconds(0.08f);
        sr.color = normalColor;
    }

    void Die()
    {
        dead = true;
        if (col != null) col.enabled = false;

        if (GameManager.Instance != null) GameManager.Instance.AddScore(points);
        if (MainManager.Instance != null) MainManager.Instance.AddCoins(coinReward);

        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);
        GameAudio.PlayExplosion();

        if (swarm != null) swarm.RemoveEnemy(this);
        Destroy(gameObject);
    }
}