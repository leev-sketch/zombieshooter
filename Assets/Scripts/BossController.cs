//koden skriven av Alex
using System.Collections;
using UnityEngine;

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
    [Tooltip("Används i stället för cannonOffsets när bossen är arg")]
    public float[] enragedCannonOffsets = { -2f, -1f, 0f, 1f, 2f };
    [Tooltip("En extra tentakel kommer upp rakt ovanför spelaren i varje salva")]
    public bool aimAtPlayer = true;

    [Header("Grafik & ljud (valfritt)")]
    [Tooltip("Spelas en gång vid varje salva, ovanpå SpriteFrameAnimator")]
    public Sprite[] attackFrames;
    [Tooltip("Tid från att attacken börjar tills tentaklerna slår i vattnet och skotten går")]
    public float slamDelay = 0.32f;
    public GameObject explosionPrefab;
    public Color hitFlashColor = Color.red;

    int hp;
    int maxHp;
    int direction = 1;
    bool entering = true;
    bool dead;
    float shootTimer;
    bool attacking;
    float slamTimer;
    float minX, maxX;
    SpriteRenderer sr;
    SpriteFrameAnimator animator;
    Color normalColor = Color.white;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<SpriteFrameAnimator>();
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
        GameAudio.PlayBossIntro();
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
        if (attacking)
        {
            slamTimer -= Time.deltaTime;
            if (slamTimer <= 0f)
            {
                attacking = false;
                FireVolley();
            }
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        shootTimer = IsEnraged ? enragedShootInterval : shootInterval;

        if (animator != null) animator.PlayOnce(attackFrames);
        attacking = true;
        slamTimer = slamDelay;
    }

    bool IsEnraged => hp <= maxHp * enrageBelow;

    void FireVolley()
    {
        if (laserPrefab == null) return;

        foreach (float offset in IsEnraged ? enragedCannonOffsets : cannonOffsets)
            SpawnShot(transform.position.x + offset);

        PlayerController player = GameManager.Instance != null ? GameManager.Instance.player : null;
        if (aimAtPlayer && player != null && player.IsAlive) SpawnShot(player.transform.position.x);

        GameAudio.PlayBossAttack();
    }

    void SpawnShot(float x)
    {
        Instantiate(laserPrefab, new Vector3(x, transform.position.y + cannonY, 0f), Quaternion.identity);
    }

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
        GameAudio.PlayExplosion();

        PlayerProgress.AddCoins(coinReward);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.OnBossDefeated();
        }
        Destroy(gameObject);
    }
}
