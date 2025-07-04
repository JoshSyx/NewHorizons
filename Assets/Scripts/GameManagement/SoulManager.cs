using System.Collections.Generic;
using UnityEngine;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance;

    [Header("Soul Spawn Settings")]
    public GameObject objectToSpawn;
    public int numberToSpawn = 3;
    public int killsPerSpawn = 5;

    private int currentKillCount = 0;
    private int spawnCount = 0;

    private List<GameObject> spawnPoints;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Get spawn points from ShrinePositioning
        spawnPoints = new List<GameObject>(ShrinePositioning.Instance.placementPoints);
    }

    public void RegisterKill()
    {
        if (spawnCount >= numberToSpawn || objectToSpawn == null || spawnPoints.Count == 0)
            return;

        currentKillCount++;

        if (currentKillCount >= killsPerSpawn)
        {
            SpawnObject();
            currentKillCount = 0;
        }
    }

    private void SpawnObject()
    {
        if (spawnPoints.Count == 0) return;

        int randomIndex = Random.Range(0, spawnPoints.Count);
        GameObject point = spawnPoints[randomIndex];

        Instantiate(objectToSpawn, point.transform.position, Quaternion.identity);
        spawnPoints.RemoveAt(randomIndex);

        spawnCount++;

        Debug.Log($"Spawned object at kill threshold. Total spawns: {spawnCount}/{numberToSpawn}");
    }
}
