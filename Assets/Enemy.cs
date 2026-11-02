using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 3;
    public int damageToPlayer = 15;
    public bool isBoss = false;

    private Transform player;
    private Transform healthBarFill;
    private int maxHealth;

    void Start()
    {
        maxHealth = health;
        GenerateHealthBar();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime);
        }
    }

    // Skapar healthbaren helt från noll via kod
    void GenerateHealthBar()
    {
        // 1. Skapa texturen
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        Sprite healthSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f));

        // Hämta standardmaterialet för 2D-sprites så att den inte blir osynlig
        Material spriteMaterial = new Material(Shader.Find("Sprites/Default"));

        // 2. Skapa bakgrunden (Röd)
        GameObject bgObj = new GameObject("HealthBar_BG");
        bgObj.transform.SetParent(transform);

        // Z-värde -1 flyttar den framför zombien i kameran
        bgObj.transform.localPosition = new Vector3(-0.5f, 0.8f, -1f);

        SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = healthSprite;
        bgRenderer.material = spriteMaterial; // Tvinga fram rätt material
        bgRenderer.color = Color.red;
        bgRenderer.sortingOrder = 100; // Sätt en jättehög siffra så den ritas överst
        bgObj.transform.localScale = new Vector3(1f, 0.15f, 1f);

        // 3. Skapa fyllningen (Grön)
        GameObject fillObj = new GameObject("HealthBar_Fill");
        fillObj.transform.SetParent(bgObj.transform);

        // Z-värde -0.1 flyttar den gröna mätaren snäppet framför den röda
        fillObj.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        fillObj.transform.localScale = Vector3.one;

        SpriteRenderer fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = healthSprite;
        fillRenderer.material = spriteMaterial; // Tvinga fram rätt material
        fillRenderer.color = Color.green;
        fillRenderer.sortingOrder = 101; // Ännu högre så den är framför det röda

        healthBarFill = fillObj.transform;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        // Räkna ut procenten HP som är kvar och ändra längden på den gröna mätaren
        if (healthBarFill != null)
        {
            float healthPercent = (float)health / maxHealth;
            healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
        }

        if (health <= 0)
        {
            if (isBoss) Debug.Log("Boss besegrad!");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            PlayerController playerScript = hitInfo.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageToPlayer);
            }
        }
    }
}