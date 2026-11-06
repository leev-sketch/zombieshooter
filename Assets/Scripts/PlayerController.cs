using UnityEngine;

// Spelarens kanon. Liv räknas i GameManager (inte här),
// spelaren försvinner när den träffas och GameManager återupplivar den.
public class PlayerController : MonoBehaviour
{
    [Header("Rörelse")]
    public float moveSpeed = 6f;
    [Tooltip("Avstånd från skärmkanten som spelaren inte kan gå förbi")]
    public float edgePadding = 0.6f;

    [Header("Skjuta")]
    public Projectile laserPrefab;
    public Transform firePoint;
    public float fireCooldown = 0.2f;
    [Tooltip("Klassiskt Space Invaders: bara 1 skott åt gången")]
    public int maxShotsOnScreen = 1;
    [Tooltip("Spelas en gang varje gang spelaren skjuter")]
    public Sprite[] attackFrames;

    [Header("Uppgraderingar (per köpt nivå)")]
    public float moveSpeedPerUpgrade = 0.15f;
    public float shotSpeedPerUpgrade = 0.2f;

    [Header("Efter att ha blivit träffad")]
    public float invincibleTime = 2f;
    public float blinkInterval = 0.1f;

    [Header("Powerup fran tunnor: tre skott i solfjader")]
    public float powerUpDuration = 8f;
    public float powerUpSpreadAngle = 12f;
    public Color powerUpColor = new Color(1f, 0.85f, 0.3f);

    [Header("Effekter (valfritt)")]
    public GameObject explosionPrefab;

    public bool IsAlive { get; private set; } = true;

    float cooldownTimer;
    float invincibleTimer;
    float powerUpTimer;
    Color normalColor = Color.white;
    float minX, maxX;
    Vector3 spawnPosition;
    SpriteRenderer sr;
    SpriteFrameAnimator animator;
    Collider2D col;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<SpriteFrameAnimator>();
        if (sr != null) normalColor = sr.color;
        col = GetComponent<Collider2D>();
        spawnPosition = transform.position;
    }

    void Start()
    {
        Camera cam = Camera.main;
        minX = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x + edgePadding;
        maxX = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x - edgePadding;

        moveSpeed *= 1f + moveSpeedPerUpgrade * PlayerProgress.GetUpgrade(UpgradeType.ShipSpeed);
        maxShotsOnScreen += PlayerProgress.GetUpgrade(UpgradeType.ExtraCannon);
    }

    void Update()
    {
        if (!IsAlive) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (GameScreens.IsPaused) return;

        Move();
        Shoot();
        UpdateInvincibility();
        UpdatePowerUp();
    }

    void Move()
    {
        // GetAxisRaw = ingen "glidning", känns mer som ett arkadspel
        float input = Input.GetAxisRaw("Horizontal");
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + input * moveSpeed * Time.deltaTime, minX, maxX);
        transform.position = pos;
    }

    void Shoot()
    {
        cooldownTimer -= Time.deltaTime;

        if (!Input.GetKey(KeyCode.Space) || cooldownTimer > 0f) return;
        if (laserPrefab == null) return;
        bool powered = powerUpTimer > 0f;
        if (Projectile.CountShots(true) >= (powered ? maxShotsOnScreen * 3 : maxShotsOnScreen)) return;

        cooldownTimer = fireCooldown;
        Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;
        FireShot(pos, 0f);
        if (powered)
        {
            FireShot(pos, powerUpSpreadAngle);
            FireShot(pos, -powerUpSpreadAngle);
        }

        if (animator != null) animator.PlayOnce(attackFrames);
        GameAudio.PlayCannon();
    }

    void FireShot(Vector3 pos, float angle)
    {
        Projectile shot = Instantiate(laserPrefab, pos, Quaternion.identity);
        shot.damage += PlayerProgress.GetUpgrade(UpgradeType.CannonPower);
        shot.speed *= 1f + shotSpeedPerUpgrade * PlayerProgress.GetUpgrade(UpgradeType.ShotSpeed);
        shot.SetAngle(angle);
    }

    public void GivePowerUp()
    {
        if (!IsAlive) return;
        powerUpTimer = powerUpDuration;
        if (sr != null) sr.color = powerUpColor;
    }

    void UpdatePowerUp()
    {
        if (powerUpTimer <= 0f) return;

        powerUpTimer -= Time.deltaTime;
        if (powerUpTimer <= 0f) EndPowerUp();
    }

    void EndPowerUp()
    {
        powerUpTimer = 0f;
        if (sr != null) sr.color = normalColor;
    }

    void UpdateInvincibility()
    {
        if (invincibleTimer <= 0f) return;

        invincibleTimer -= Time.deltaTime;
        if (sr != null)
            sr.enabled = invincibleTimer <= 0f || Mathf.Repeat(invincibleTimer, blinkInterval * 2f) > blinkInterval;
    }

    // Returnerar true om spelaren faktiskt tog skada
    public bool TakeHit()
    {
        if (!IsAlive || invincibleTimer > 0f) return false;
        Die();
        return true;
    }

    void Die()
    {
        IsAlive = false;
        EndPowerUp();
        if (sr != null) sr.enabled = false;
        if (col != null) col.enabled = false;

        if (explosionPrefab != null)
            Destroy(Instantiate(explosionPrefab, transform.position, Quaternion.identity), 1f);
        GameAudio.PlayPlayerDeath();

        if (GameManager.Instance != null) GameManager.Instance.OnPlayerDied();
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        IsAlive = true;
        if (col != null) col.enabled = true;
        if (sr != null) sr.enabled = true;
        invincibleTimer = invincibleTime;
    }
}