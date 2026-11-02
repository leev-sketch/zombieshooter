using UnityEngine;

public class BalleProjectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 25;
    private Vector2 targetDirection;

    void Start()
    {
        // Hitta spelaren och räkna ut riktningen dit
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            targetDirection = (p.transform.position - transform.position).normalized;
        }
        else
        {
            targetDirection = Vector2.left; // Standard om spelaren är död
        }

        // Förstör projektilen efter 4 sekunder så den inte flyger för evigt
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        transform.Translate(targetDirection * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            PlayerController playerScript = hitInfo.GetComponent<PlayerController>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}