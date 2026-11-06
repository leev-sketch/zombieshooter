//koden skriven av Umar
using UnityEngine;

public class BunkerBlock : MonoBehaviour
{
    bool destroyed;

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