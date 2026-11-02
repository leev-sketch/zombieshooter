using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;

    // NYTT: Variabel för att bestämma kulans riktning
    public Vector2 direction = Vector2.right;

    void Update()
    {
        // NYTT: Använd 'direction' istället för hårdkodat 'Vector2.right'
        transform.Translate(direction * speed * Time.deltaTime);

        // Förstör kulan när den åker utanför skärmen åt något håll
        if (Mathf.Abs(transform.position.x) > 15f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        BonnieBlue boss = hitInfo.GetComponent<BonnieBlue>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }

        // Lägg till detta precis under logiken för Bonnie Blue i Bullet.cs
        JohnnySins johnnyBoss = hitInfo.GetComponent<JohnnySins>();
        if (johnnyBoss != null)
        {
            johnnyBoss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}