//koden skriven av Leevi
using System.Collections.Generic;
using UnityEngine;

public class EnemySwarm : MonoBehaviour
{
    [System.Serializable]
    public class EnemyRow
    {
        public EnemyController prefab;
        public int points = 10;
    }

    [Header("Formation (element 0 = översta raden)")]
    public EnemyRow[] rows;
    public int columns = 11;
    public float spacingX = 1.1f;
    public float spacingY = 0.8f;
    [Tooltip("Höjd för översta raden i våg 1")]
    public float startY = 3.5f;
    [Tooltip("Varje ny våg börjar så här mycket längre ner")]
    public float extraDropPerWave = 0.4f;
    public int maxWaveDrops = 5;

    [Header("Rörelse")]
    public float stepX = 0.15f;
    public float stepDown = 0.4f;
    [Tooltip("Tid mellan att två fiender flyttas. Klassiskt: en per bildruta (1/60).")]
    public float secondsPerEnemyMove = 1f / 60f;
    [Tooltip("Hur mycket snabbare varje ny våg är (0.1 = 10 %)")]
    public float speedUpPerWave = 0.1f;
    public float edgeMargin = 0.5f;
    [Tooltip("Når en fiende hit är spelet slut. Sätt strax ovanför spelaren.")]
    public float invasionY = -3.5f;

    [Header("Skjutande")]
    public float baseShootInterval = 1.2f;
    public float minShootInterval = 0.35f;
    public float shootIntervalDecreasePerWave = 0.1f;
    public int maxEnemyShotsOnScreen = 3;
    [Tooltip("Fienderna skjuter tillsammans: ett skott per vinkel. 0 = rakt ner.")]
    public float[] shotAngles = { 0f, -25f, 25f };
    [Tooltip("Chans att skottet kommer från kolumnen närmast spelaren")]
    [Range(0f, 1f)] public float aimedShotChance = 0.4f;

    [Header("Ljud (valfritt)")]
    public AudioSource audioSource;
    [Tooltip("Klassiskt: 4 låga toner som spelas i tur och ordning")]
    public AudioClip[] marchNotes;

    public int AliveCount => enemies.Count;
    public bool IsPaused { get; private set; }

    readonly List<EnemyController> enemies = new List<EnemyController>();
    int moveIndex;
    float moveTimer;
    float shootTimer;
    int direction = 1;
    bool dropThisPass;
    int noteIndex;
    int wave = 1;
    float leftEdge, rightEdge, centerX;

    void Awake()
    {
        Camera cam = Camera.main;
        leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;
        centerX = cam.transform.position.x;
    }

    void Update()
    {
        if (IsPaused || enemies.Count == 0) return;
        HandleMovement();
        HandleShooting();
    }

    public void SetPaused(bool value)
    {
        IsPaused = value;
    }

    public void SpawnWave(int waveNumber)
    {
        ClearEnemies();
        wave = waveNumber;

        if (rows == null || rows.Length == 0)
        {
            Debug.LogWarning("EnemySwarm: inga rader är inställda i Inspector!");
            return;
        }

        float drop = Mathf.Min(wave - 1, maxWaveDrops) * extraDropPerWave;
        Vector3 origin = new Vector3(centerX, startY - drop, 0f);

        for (int r = rows.Length - 1; r >= 0; r--)
        {
            if (rows[r].prefab == null) continue;

            for (int c = 0; c < columns; c++)
            {
                Vector3 pos = origin + new Vector3((c - (columns - 1) * 0.5f) * spacingX, -r * spacingY, 0f);
                EnemyController enemy = Instantiate(rows[r].prefab, pos, Quaternion.identity, transform);
                enemy.Init(this, c, rows[r].points);
                enemies.Add(enemy);
            }
        }

        moveIndex = 0;
        moveTimer = 0f;
        direction = 1;
        dropThisPass = false;
        noteIndex = 0;
        shootTimer = CurrentShootInterval() + 1f;
        IsPaused = false;
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        int i = enemies.IndexOf(enemy);
        if (i < 0) return;

        enemies.RemoveAt(i);
        if (i < moveIndex) moveIndex--;

        if (enemies.Count == 0 && GameManager.Instance != null)
            GameManager.Instance.OnWaveCleared();
    }

    void ClearEnemies()
    {
        foreach (EnemyController e in enemies)
            if (e != null) Destroy(e.gameObject);
        enemies.Clear();
    }

    void HandleMovement()
    {
        float interval = Mathf.Max(0.0001f, secondsPerEnemyMove / (1f + (wave - 1) * speedUpPerWave));

        moveTimer += Time.deltaTime;
        moveTimer = Mathf.Min(moveTimer, interval * enemies.Count);

        while (moveTimer >= interval && enemies.Count > 0 && !IsPaused)
        {
            moveTimer -= interval;
            MoveNextEnemy();
        }
    }

    void MoveNextEnemy()
    {
        if (moveIndex >= enemies.Count) StartNewPass();

        EnemyController enemy = enemies[moveIndex];
        moveIndex++;

        Vector3 step = dropThisPass ? Vector3.down * stepDown : Vector3.right * (direction * stepX);
        enemy.transform.position += step;
        enemy.OnStep();

        if (enemy.transform.position.y <= invasionY && GameManager.Instance != null)
            GameManager.Instance.OnInvasion();
    }

    void StartNewPass()
    {
        moveIndex = 0;

        if (dropThisPass)
        {
            dropThisPass = false;
        }
        else if (AnyEnemyAtEdge())
        {
            dropThisPass = true;
            direction = -direction;
        }

        PlayMarchNote();
    }

    bool AnyEnemyAtEdge()
    {
        foreach (EnemyController e in enemies)
        {
            float x = e.transform.position.x;
            if (direction > 0 && x >= rightEdge - edgeMargin) return true;
            if (direction < 0 && x <= leftEdge + edgeMargin) return true;
        }
        return false;
    }

    void PlayMarchNote()
    {
        if (audioSource == null || marchNotes == null || marchNotes.Length == 0) return;
        audioSource.PlayOneShot(marchNotes[noteIndex]);
        noteIndex = (noteIndex + 1) % marchNotes.Length;
    }

    void HandleShooting()
    {
        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f) return;

        shootTimer = CurrentShootInterval() * Random.Range(0.7f, 1.3f);

        if (Projectile.CountShots(false) >= maxEnemyShotsOnScreen) return;

        List<EnemyController> shooters = PickShooters();
        if (shooters.Count == 0) return;

        for (int i = 0; i < shotAngles.Length; i++)
            shooters[i % shooters.Count].Shoot(shotAngles[i]);
    }

    float CurrentShootInterval()
    {
        return Mathf.Max(minShootInterval, baseShootInterval - (wave - 1) * shootIntervalDecreasePerWave);
    }

    List<EnemyController> PickShooters()
    {
        Dictionary<int, EnemyController> bottom = new Dictionary<int, EnemyController>();
        foreach (EnemyController e in enemies)
        {
            if (!bottom.TryGetValue(e.Column, out EnemyController current) ||
                e.transform.position.y < current.transform.position.y)
            {
                bottom[e.Column] = e;
            }
        }

        List<EnemyController> candidates = new List<EnemyController>(bottom.Values);
        List<EnemyController> shooters = new List<EnemyController>();

        PlayerController player = GameManager.Instance != null ? GameManager.Instance.player : null;
        if (candidates.Count > 0 && player != null && player.IsAlive && Random.value < aimedShotChance)
        {
            EnemyController best = null;
            float bestDistance = float.MaxValue;
            foreach (EnemyController e in candidates)
            {
                float d = Mathf.Abs(e.transform.position.x - player.transform.position.x);
                if (d < bestDistance) { bestDistance = d; best = e; }
            }
            shooters.Add(best);
            candidates.Remove(best);
        }

        while (shooters.Count < shotAngles.Length && candidates.Count > 0)
        {
            int i = Random.Range(0, candidates.Count);
            shooters.Add(candidates[i]);
            candidates.RemoveAt(i);
        }
        return shooters;
    }
}
