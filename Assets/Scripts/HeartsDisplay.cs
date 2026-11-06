//koden skriven av Leevi
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsDisplay : MonoBehaviour
{
    public Sprite fullHeart;
    public Sprite emptyHeart;
    public float heartSize = 60f;

    readonly List<Image> hearts = new List<Image>();
    int shownLives = -1;
    int shownMax = -1;

    void OnEnable() { Refresh(); }
    void Update() { Refresh(); }

    void Refresh()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || (gm.Lives == shownLives && gm.MaxLives == shownMax)) return;

        shownLives = gm.Lives;
        shownMax = gm.MaxLives;

        while (hearts.Count < shownMax) hearts.Add(CreateHeart());

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].sprite = i < shownLives ? fullHeart : emptyHeart;
    }

    Image CreateHeart()
    {
        GameObject go = new GameObject("Heart", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        ((RectTransform)go.transform).sizeDelta = new Vector2(heartSize, heartSize);

        Image image = go.GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }
}
