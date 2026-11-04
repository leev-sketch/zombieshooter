using UnityEngine;

// Ett litet block i en bunker. Bunker.cs skapar dessa automatiskt.
public class BunkerBlock : MonoBehaviour
{
    bool destroyed;

    // Returnerar true om blocket fanns kvar och nu förstördes
    public bool Hit(bool erodeNeighbours = true)
    {
        if (destroyed) return false;
        destroyed = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (erodeNeighbours)
        {
            Bunker bunker = GetComponentInParent<Bunker>();
            if (bunker != null) bunker.Erode(transform.position);
        }

        Destroy(gameObject);
        return true;
    }
}