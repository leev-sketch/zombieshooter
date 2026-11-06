using UnityEngine;

// Rullar en tilad bakgrund nedåt och hoppar tillbaka efter en hel tile, så det ser ändlöst ut.
// Följer Time.deltaTime, så rörelsen stannar när spelet pausas.
[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("Enheter per sekund")]
    public float scrollSpeed = 0.75f;

    Vector3 startPosition;
    float tileHeight;
    float pixelsPerUnit;
    float offset;

    void Awake()
    {
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        tileHeight = sprite.bounds.size.y;
        pixelsPerUnit = sprite.pixelsPerUnit;
        startPosition = transform.position;
    }

    void Update()
    {
        offset = Mathf.Repeat(offset + scrollSpeed * Time.deltaTime, tileHeight);

        // Flytta i hela pixlar så pixelgrafiken håller sig skarp
        float snapped = Mathf.Round(offset * pixelsPerUnit) / pixelsPerUnit;
        transform.position = startPosition + Vector3.down * snapped;
    }
}
