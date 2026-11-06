//koden skriven av Leevi
using UnityEngine;

public class Bunker : MonoBehaviour
{
    public BunkerBlock blockPrefab;
    [Tooltip("Storlek på varje block (blockets sprite bör vara 1x1 enhet)")]
    public float blockSize = 0.12f;

    [Tooltip("# = block, . = tomt. Översta raden först.")]
    [TextArea(4, 12)]
    public string shape =
        "...########...\n" +
        "..##########..\n" +
        ".############.\n" +
        "##############\n" +
        "##############\n" +
        "####......####\n" +
        "###........###";

    [Header("Skador")]
    public float erosionRadius = 0.18f;
    [Range(0f, 1f)] public float erosionChance = 0.45f;

    void Start()
    {
        Build();
    }

    public void Build()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        if (blockPrefab == null)
        {
            Debug.LogWarning("Bunker: blockPrefab saknas på " + name);
            return;
        }

        string[] lines = shape.Replace("\r", "").Split('\n');
        int height = lines.Length;
        int width = 0;
        foreach (string line in lines) width = Mathf.Max(width, line.Length);

        for (int row = 0; row < height; row++)
        {
            for (int c = 0; c < lines[row].Length; c++)
            {
                if (lines[row][c] != '#') continue;

                BunkerBlock block = Instantiate(blockPrefab, transform);
                block.transform.localPosition = new Vector3(
                    (c - (width - 1) * 0.5f) * blockSize,
                    ((height - 1) * 0.5f - row) * blockSize,
                    0f);
                block.transform.localScale = Vector3.one * blockSize;
            }
        }
    }

    public void Erode(Vector2 point)
    {
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(point, erosionRadius))
        {
            if (hit.TryGetComponent(out BunkerBlock block) &&
                block.transform.parent == transform &&
                Random.value < erosionChance)
            {
                block.Hit(false);
            }
        }
    }
}