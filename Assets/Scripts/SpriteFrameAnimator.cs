//koden skriven av Umar
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFrameAnimator : MonoBehaviour
{
    public Sprite[] frames;
    public float framesPerSecond = 8f;

    SpriteRenderer sr;
    float timer;
    int frame;
    Sprite[] oneShot;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void PlayOnce(Sprite[] once)
    {
        if (once == null || once.Length == 0) return;

        oneShot = once;
        frame = 0;
        timer = 0f;
        sr.sprite = once[0];
    }

    void Update()
    {
        Sprite[] current = oneShot != null ? oneShot : frames;
        if (current == null || current.Length < 2 || framesPerSecond <= 0f) return;

        float frameTime = 1f / framesPerSecond;
        timer += Time.deltaTime;
        if (timer < frameTime) return;

        timer %= frameTime;
        frame++;

        if (oneShot != null && frame >= oneShot.Length)
        {
            oneShot = null;
            frame = 0;
            current = frames;
            if (current == null || current.Length == 0) return;
        }

        frame %= current.Length;
        sr.sprite = current[frame];
    }
}
