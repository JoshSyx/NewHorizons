using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
internal class EnemiesPerWaveSettings
{
    public Enemy enemyPrefab;
    [SerializeField]
    [ProgressBar(1, 10, 0, 0.4f, 0.5f, Segmented = true)]
    [InfoBox("@\"Probability: \" + _spawnProbability * 10 + \"%\"")]
    private float _spawnProbability;
    
    public float GetSpawnProbability() => _spawnProbability / 10;
}

[Serializable]
internal class WaveSettings
{
    [SerializeField]
    public int numberOfEnemies;
    [SerializeField]
    public float spawnInterval;
    [SerializeField]
    public EnemiesPerWaveSettings[] enemies;
}

public class EnemiesController : MonoBehaviour
{
    public static EnemiesController Instance;
    
    [SerializeField]
    [ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = false, ShowIndexLabels = true)]
    private WaveSettings[] waves;
    [SerializeField]
    private float spawnRadius = 10f;
    
    private int _currentWave;
    private List<Enemy> _activeEnemies = new();
    private int _activeEnemiesCount;
    private Transform _player;
    
    private void Awake()
    {
        if (!Instance || Instance != this)
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player object not found!");
        }
        StartCoroutine(SpawnWave());
        _activeEnemiesCount = waves[_currentWave].numberOfEnemies;
    }

    private void Update()
    {
        if (waves != null && waves.Length != 0)
        {
            if (_activeEnemiesCount <= waves[_currentWave].numberOfEnemies * 0.1f && _currentWave <= waves.Length - 2)
            {
                _currentWave++;
                StartCoroutine(SpawnWave());
                _activeEnemiesCount = waves[_currentWave].numberOfEnemies;
            }
        }
    }

    private IEnumerator SpawnWave()
    {
        if (waves == null || waves.Length == 0) yield break;
        
        for (int i = 0; i < waves[_currentWave].numberOfEnemies; i++)
        {
            var randomSpawnPosition = GetRandomSpawnPositionNearPlayer();
            var enemyPrefab = GetRandomPrefab(waves[_currentWave].enemies);
            if (randomSpawnPosition != Vector3.zero && enemyPrefab)
            {
                var instantiatedEnemy = Instantiate(enemyPrefab, randomSpawnPosition, Quaternion.identity);
                _activeEnemies.Add(instantiatedEnemy);
                yield return new WaitForSeconds(waves[_currentWave].spawnInterval);
            }
            else
            {
                i--;
                yield return null;
            }
        }
    }

    private Vector3 GetRandomSpawnPositionNearPlayer()
    {
        var randomDirection = _player.position + Random.insideUnitSphere * spawnRadius;
        if (NavMesh.SamplePosition(randomDirection, out var hit, 50f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return Vector3.zero;
    }


    private Enemy GetRandomPrefab(EnemiesPerWaveSettings[] waveEnemiesSettings)
    {
        if (waveEnemiesSettings == null || waveEnemiesSettings.Length == 0)
            return null;

        var totalWeight = waveEnemiesSettings.Sum(x => x.GetSpawnProbability());

        if (totalWeight <= 0f)
            return null;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        
        foreach (var item in waveEnemiesSettings)
        {
            cumulative += item.GetSpawnProbability();

            if (randomValue <= cumulative)
            {
                return item.enemyPrefab;
            }
        }

        return null;
    }

    public void EnemyKilled(Enemy enemy)
    {
        _activeEnemies.Remove(enemy);
        _activeEnemiesCount--;
    }
}
