using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

internal class Chunk
{
    public Chunk(float[,] matrix, Vector2Int position)
    {
        this.matrix = matrix;
        this.position = position;
    }
    public float[,] matrix;
    public Vector2Int position;
}

public class MapGenerator : MonoBehaviour
{
    [TabGroup("General")]
    [SerializeField]
    private Vector2Int chunkSize;
    
    [TabGroup("General")]
    [SerializeField]
    private Transform player;
    
    [TabGroup("General")]
    [SerializeField]
    private int drawDistance;
    
    
    [TabGroup("Randomization")]
    [SerializeField]
    private Vector2 perlinScale;
    
    [TabGroup("Randomization")]
    [SerializeField]
    private float perlinThreshold;
    
    [TabGroup("Randomization")]
    [SerializeField]
    private float perlinSaturation;
    
    [SerializeField]
    [TabGroup("Randomization")]
    [InlineButton("RandomizeSeed", SdfIconType.Dice4)]
    private int seed;
    
    private List<Chunk> _loadedChunks = new();
    private Dictionary<Vector2Int, Vector2Int> _shrinesPositions = new();
    
    private readonly int[,] _shrineShape =
    {
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }
    };

    private void Update()
    {
        CalculateChunks(player.position);
    }

    public void CalculateChunks(Vector3 position)
    {
        var currentChunk = new Vector2Int(Mathf.FloorToInt(position.x / chunkSize.x), Mathf.FloorToInt(position.z / chunkSize.y));
        var chunksToLoad = new List<Vector2Int>();
        var center = new Vector2Int(drawDistance, drawDistance);

        for (int x = 0; x < drawDistance * 2 + 1; x++)
        {
            for (int y = 0; y < drawDistance * 2 + 1; y++)
            {
                chunksToLoad.Add(currentChunk + new Vector2Int(center.x - x, center.y - y));
            }
        }
        GenerateShrinesMap(chunksToLoad);
        UpdateChunks(chunksToLoad);
    }

    private void GenerateShrinesMap(List<Vector2Int> surroundingChunks)
    {
        foreach (var chunk in surroundingChunks)
        {
            if (_shrinesPositions.ContainsKey(chunk)) continue;

            var chunkMatrix = GenerateChunk(chunk).matrix;
            if (TryToFindShrinePosition(chunkMatrix, out var position))
            {
                _shrinesPositions.Add(chunk, position);
            }
            else
            {
                _shrinesPositions.Add(chunk, new Vector2Int(-1, -1));
            }
        }
    }

    private bool TryToFindShrinePosition(float[,] chunkMatrix, out Vector2Int position)
    {
        for (int x = 0; x <= chunkMatrix.GetLength(0) - _shrineShape.GetLength(0); x++)
        {
            for (int y = 0; y <= chunkMatrix.GetLength(1) - _shrineShape.GetLength(1); y++)
            {
                bool match = true;

                for (int sx = 0; sx < _shrineShape.GetLength(0); sx++)
                {
                    for (int sy = 0; sy < _shrineShape.GetLength(1); sy++)
                    {
                        if (_shrineShape[sx, sy] == 1 && chunkMatrix[x + sx, y + sy] != 0)
                        {
                            match = false;
                            break;
                        }
                    }
                    if (!match) break;
                }

                if (match)
                {
                    position = new Vector2Int(x, y);
                    return true;
                }
            }
        }

        position = Vector2Int.zero;
        return false;
    }

    private void UpdateChunks(List<Vector2Int> rawChunksToLoad)
    {
        foreach (var chunkPos in rawChunksToLoad)
        {
            if (_loadedChunks.Exists(c => c.position == chunkPos)) continue;
            _loadedChunks.Add(GenerateChunk(chunkPos));
        }
        
        var chunksToRemove = _loadedChunks.Where(chunk => !rawChunksToLoad.Contains(chunk.position)).ToList();
        foreach (var chunk in chunksToRemove)
        {
            _loadedChunks.Remove(chunk);
        }
    }

    private void RandomizeSeed()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
    }
    
    private Chunk GenerateChunk(Vector2Int chunk = default)
    {
        var rnd = new System.Random(seed);
        var matrix = new float[chunkSize.x, chunkSize.y];
        var offset = new Vector2(rnd.Next(-100000, 100000), rnd.Next(-100000, 100000));
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                var perlin = Mathf.PerlinNoise((x + offset.x + chunkSize.x * chunk.x) / perlinScale.x, (y + offset.y + chunkSize.y * chunk.y) / perlinScale.y);
                if (perlin > perlinThreshold)
                {
                    matrix[x, y] = perlinSaturation * perlin;
                    Debug.Log(matrix[x, y]);
                }
                else
                {
                    matrix[x, y] = 0;
                }
            }
        }
        var newChuck = new Chunk(matrix, chunk);
        return newChuck;
    }

    private void OnDrawGizmos()
    {
        if (_loadedChunks == null || _loadedChunks.Count == 0) return;
        foreach (var chunk in _loadedChunks)
        {
            Gizmos.color = Color.green;
            if (_shrinesPositions[chunk.position] != new Vector2Int(-1, -1))
            {
                Gizmos.color = Color.magenta;
            }
            Gizmos.DrawWireCube(
                new Vector3(
                    chunkSize.x * chunk.position.x + chunkSize.x / 2, 
                    -0.5f, 
                    chunkSize.y * chunk.position.y + chunkSize.y / 2
                ), 
                new Vector3(
                    chunkSize.x, 
                    1, 
                    chunkSize.y
                )
            );
            if (_shrinesPositions[chunk.position] != new Vector2Int(-1, -1))
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(
                    new Vector3(
                        chunkSize.x * chunk.position.x + _shrinesPositions[chunk.position].x + Mathf.FloorToInt(_shrineShape.GetLength(0) / 2), 
                        0,
                        chunkSize.y * chunk.position.y + _shrinesPositions[chunk.position].y + Mathf.FloorToInt(_shrineShape.GetLength(1) / 2)
                    ), 
                    new Vector3(_shrineShape.GetLength(0), 3, _shrineShape.GetLength(1))
                );
            }
            for (var x = 0; x < chunkSize.x; x++)
            {
                for (var y = 0; y < chunkSize.y; y++)
                {
                    Gizmos.color = Color.Lerp(Color.green, Color.red, chunk.matrix[x, y]);
                    if (chunk.matrix[x, y] != 0)
                    {
                        Gizmos.DrawCube(
                            new Vector3(
                                x + chunkSize.x * chunk.position.x, 
                                Mathf.Lerp(-0.5f, 2.5f, chunk.matrix[x, y]), 
                                y + chunkSize.y * chunk.position.y
                            ), 
                            new Vector3(
                                1,
                                Mathf.Lerp(0, 5, chunk.matrix[x, y]),
                                1
                            )
                        );
                    }
                }
            }
        }
    }
}