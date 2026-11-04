using System.Collections;
using UnityEngine;

// Bossen i slutet av en nivå. Glider in uppifrån, åker fram och tillbaka och skjuter salvor.
// GameManager skapar den och visar hälsomätaren.
public class BossController : MonoBehaviour
{
    [Header("Liv & belöning")]
    public int points = 500;
    public int coinReward = 150;

    [Header("Rörelse")]
    public float moveSpeed = 3f;
    public float enterSpeed = 2f;
    public float fightY = 2.6f;
    public float edgeMargin = 0.5f;

    [Header("Skjuta")]
    public Projectile laserPrefab;
    [Tooltip("Kanonernas placering i sidled, en kula per kanon i varje salva")]
    public float[] cannonOffsets = { -1f, 0f, 1f };
    public float cannonY = -0.9f;
    public float shootInterval = 1.6f;
    [Tooltip("Under så här stor del av livet blir bossen arg och skjuter snabbare")]
    [Range(0f, 1f)] public float enrageBelow = 0.5f;
    public float enragedShootInterval = 0.9f;

    [Header("Grafik & ljud (valfritt)")]
    public GameObject explosionPrefab;
    public AudioClip deathSound;
    public Color hitFlashColor = Color.red;

    int hp;
    int maxHp;
    int direction = 1;
    bool entering = true;
    bool dead;
    float shootTimer;
    float minX, maxX;
    SpriteRenderer sr;
    Color normalColor = Color.white;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) normalColor = sr.color;
    }

    public void Init(int health)
    {
        maxHp = health;
        hp = health;
        shootTimer = shootInterval;

        Camera cam = Camera.main;
        float halfWidth = sr != null ? sr.bounds.extents.x : 1f;
        minX = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x + halfWidth + edgeMargin;
        maxX = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x - halfWidth - edgeMargin;
        float topY = cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y;
        transform.position = new Vector3(cam.transform.position.x, topY + 2f, 0f);

        if (GameManager.Instance != null) GameManager.Instance.OnBossHealthChanged(hp, maxHp);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || gm.IsGameOver || gm.swarm.IsPaused) return;

        if (entering)
        {
            Enter();
            return;
        }

        Move();
        HandleShooting();
    }

    void Enter()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, fightY, enterSpeed * Time.deltaTime);
        transform.position = pos;
        if (Mathf.Approximately(pos.y, fightY)) entering = false;
    }

    void Move()
    {
        Vector3 pos = transform.position;
        pos.x += direction * moveSpeed * Time.deltaTime;
        if (pos.x >= maxX) direction = -1;
        else if (pos.x <= minX) direction = 1;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        transform.position = pos;
    }

    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        bool enraged = hp <= maxHp * enrageBelow;
        shootTimer = enraged ? enragedShootInterval : shootInterval;

        if (laserPrefab == null) return;
        foreach (float offset in cannonOffsets)
            Instantiate(laserPrefab, transform.position + new Vector3(offset, cannonY, 0f), Quaternion.identity);
    }

    // Returnerar true om skottet räknas som en träff
    public bool TakeDamage(int damage)
    {
        if (dead || entering) return false;

        hp = Mathf.Max(0, hp - damage);
        if (GameManager.Instance != null) GameManager.Instance.OnBossHealthChanged(hp, maxHp);

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

        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position);

        PlayerProgress.AddCoins(coinReward);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.OnBossDefeated();
        }
        Destroy(gameObject);
    }
}
