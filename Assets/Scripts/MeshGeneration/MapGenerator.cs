using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    public int width = 128;
    public int height = 128;
    public RawImage display; // Assign in Inspector for UI

    private Texture2D mapTexture;
    private Color[,] mapColors;

    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        mapTexture = new Texture2D(width, height);
        mapColors = new Color[width, height];

        GenerateBiomes();
        GenerateRiver();
        PlaceLandmarks();
        DrawRoads();
        ApplyTexture();
    }

    void GenerateBiomes()
    {
        float scale = 20f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Mathf.PerlinNoise(x / scale, y / scale);
                if (value < 0.3f)
                    mapColors[x, y] = Color.green;
                else if (value < 0.5f)
                    mapColors[x, y] = new Color(0.8f, 0.5f, 0.2f); // orange
                else if (value < 0.7f)
                    mapColors[x, y] = new Color(1f, 0.7f, 0.9f); // pink
                else
                    mapColors[x, y] = new Color(0.5f, 0.7f, 1f); // blue
            }
        }
    }

    void GenerateRiver()
    {
        Vector2Int pos = new Vector2Int(width / 2, height - 1);
        Vector2Int end = new Vector2Int(Random.Range(0, width), 0);

        while (pos.y > 0)
        {
            mapColors[pos.x, pos.y] = Color.cyan;

            // Random walk down
            int dx = Random.Range(-1, 2);
            pos.x = Mathf.Clamp(pos.x + dx, 0, width - 1);
            pos.y--;
        }
    }

    List<Vector2Int> landmarks = new List<Vector2Int>();

    void PlaceLandmarks()
    {
        // Yellow (center)
        PlaceDot(Color.yellow, width / 2, height / 2);

        // Yellow (top-left)
        PlaceDot(Color.yellow, Random.Range(5, width / 4), height - Random.Range(5, 20));

        // Yellow (bottom-left)
        PlaceDot(Color.yellow, Random.Range(5, width / 4), Random.Range(5, height / 4));

        // Red (bottom-right)
        PlaceDot(Color.red, width - Random.Range(5, 20), Random.Range(5, height / 4));
    }

    void PlaceDot(Color color, int x, int y)
    {
        mapColors[x, y] = color;
        landmarks.Add(new Vector2Int(x, y));
    }

    void DrawRoads()
    {
        // Draw black lines between landmarks
        for (int i = 0; i < landmarks.Count - 1; i++)
        {
            DrawLine(landmarks[i], landmarks[i + 1], Color.black);
        }
    }

    void DrawLine(Vector2Int a, Vector2Int b, Color color)
    {
        int x0 = a.x, y0 = a.y;
        int x1 = b.x, y1 = b.y;

        int dx = Mathf.Abs(x1 - x0), dy = -Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                mapColors[x0, y0] = color;

            if (x0 == x1 && y0 == y1) break;

            e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    void ApplyTexture()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mapTexture.SetPixel(x, y, mapColors[x, y]);

        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();

        if (display != null)
        {
            display.texture = mapTexture;
        }
        else
        {
            // Optional: create a sprite in world space if RawImage is not used
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(mapTexture, new Rect(0, 0, width, height), Vector2.one * 0.5f, 1);
        }
    }
}
