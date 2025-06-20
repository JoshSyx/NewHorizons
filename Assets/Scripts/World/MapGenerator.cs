using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private Vector2Int startPoint;
    [SerializeField]
    private Vector2Int chunkSize;
    [SerializeField]
    [InlineButton("RandomizeSeed")]
    private int seed;
    [SerializeField]
    private Vector2 perlinScale;
    [SerializeField]
    private float perlinThreshold;
    [SerializeField]
    private float perlinSaturation;
    [SerializeField] 
    private Vector2Int currentChunk;
    
    private float[,] _map;
    private System.Random _rnd;

    private void OnEnable()
    {
        _map = new float[chunkSize.x, chunkSize.y];
        _rnd = new System.Random(seed);
    }

    private void OnValidate()
    {
        GenerateMap();
    }

    private void RandomizeSeed()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        _rnd = new System.Random(seed);
    }
    
    [Button]
    private void GenerateMap()
    {
        _rnd = new System.Random(seed);
        var offset = new Vector2(_rnd.Next(-100000, 100000), _rnd.Next(-100000, 100000));
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                if (x == _map.GetLength(0) - 1 || x == 0 || y == _map.GetLength(1) - 1 || y == 0)
                {
                    _map[x, y] = 1;
                }
                else
                {
                    var perlin = Mathf.PerlinNoise((x + offset.x + chunkSize.x * currentChunk.x) / perlinScale.x, (y + offset.y + chunkSize.y * currentChunk.y) / perlinScale.y);
                    _map[x, y] = perlin > perlinThreshold ? perlin : 0;
                    _map[x, y] = Mathf.Clamp(_map[x, y] * perlinSaturation, 0, 1);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_map == null || _map.GetLength(0) == 0 || _map.GetLength(1) == 0) return;

        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                Gizmos.color = Color.Lerp(Color.green, Color.red, _map[x, y]);
                Gizmos.DrawCube(new Vector3(x + chunkSize.x * currentChunk.x, 0, y + chunkSize.y * currentChunk.y), Vector3.one);
            }
        }
    }
}