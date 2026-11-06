//koden skriven av Alex
using UnityEngine;

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

        float snapped = Mathf.Round(offset * pixelsPerUnit) / pixelsPerUnit;
        transform.position = startPosition + Vector3.down * snapped;
    }
}
