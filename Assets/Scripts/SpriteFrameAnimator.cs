using UnityEngine;

// Spelar upp en lista med sprites i en slinga (vattnet, spelarens båt).
// Följer Time.deltaTime, så animationen stannar när spelet pausas.
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

    // Spelar en annan animation en gång och går sedan tillbaka till slingan
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
