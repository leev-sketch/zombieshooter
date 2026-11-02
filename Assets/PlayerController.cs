using UnityEngine;
using UnityEngine.UI; // Viktigt för att UI ska fungera
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int maxHealth = 100;
    public int currentHealth;

    public Slider healthBar; // Länka din PlayerHealthBar här i Inspector

    private bool hasAids = false;
    private bool hasStd = false;
    private bool hasFungus = false;
    private Vector2 facingDirection = Vector2.right;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Update()
    {
        float currentSpeed = hasStd ? speed * 0.5f : speed;
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector2 moveDirection = new Vector2(x, y).normalized;
        transform.Translate(moveDirection * currentSpeed * Time.deltaTime);

        if (x != 0)
        {
            facingDirection = x > 0 ? Vector2.right : Vector2.left;
            transform.localScale = new Vector3(x > 0 ? 1 : -1, 1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject newBullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        newBullet.GetComponent<Bullet>().direction = facingDirection;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = hasAids ? damage * 2 : damage;
        currentHealth -= finalDamage;

        if (healthBar != null)
        {
            healthBar.value = currentHealth; // Uppdatera UI
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void InfectWithBonnieBlueDiseases()
    {
        if (!hasAids || !hasStd || !hasFungus)
        {
            hasAids = true;
            hasStd = true;
            if (!hasFungus)
            {
                hasFungus = true;
                StartCoroutine(FungusDamageRoutine());
            }
        }
    }

    IEnumerator FungusDamageRoutine()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(2f);
            TakeDamage(5);
        }
    }
}