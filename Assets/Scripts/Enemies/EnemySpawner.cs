using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// <summary>
/// Defines the settings for enemy type that can spawn within a wave.
/// </summary>
/// <remarks>
/// Contains information about the enemy prefab and its spawn probability,
/// which determines the likelihood of each enemy type being spawned during a wave.
/// </remarks>
[Serializable]
internal class EnemiesPerWaveSettings
{
    /// <summary>
    /// Represents the prefab of an enemy used in spawning during a wave.
    /// </summary>
    public EnemyController enemyPrefab;

    /// <summary>
    /// Represents the probability of spawning a specific enemy in a wave configuration.
    /// </summary>
    [SerializeField]
    [ProgressBar(1, 10, 0, 0.4f, 0.5f, Segmented = true)]
    [InfoBox("@\"Probability: \" + _spawnProbability * 10 + \"%\"")]
    private float _spawnProbability;

    /// <summary>
    /// Retrieves the spawn probability for the associated enemy.
    /// </summary>
    /// <remarks>
    /// Due to the <see cref="ProgressBarAttribute"/> behaviour, the <see cref="_spawnProbability"/> takes values from 1 to 10 for the probability.
    /// </remarks>
    /// <returns>
    /// A float value representing the spawn probability of the enemy, scaled down to a range of 0.0 to 1.0.
    /// </returns>
    public float GetSpawnProbability() => _spawnProbability / 10;
}

/// <summary>
/// Represents the settings for a wave of enemy spawns in the game.
/// </summary>
/// <remarks>
/// This class contains configuration for the number of enemies, their spawn interval, and specific details
/// about the enemies to be spawned within the wave.
/// Each wave can contain multiple types of enemies, each with a spawn probability.
/// </remarks>
[Serializable]
internal class WaveSettings
{
    /// <summary>
    /// Represents the total number of enemies to be spawned in a single wave.
    /// This value determines how many enemy instances will be generated during
    /// the course of the wave.
    /// </summary>
    [SerializeField]
    public int numberOfEnemies;

    /// <summary>
    /// Specifies the interval, in seconds, between consecutive enemy spawns within a wave.
    /// </summary>
    [SerializeField]
    public float spawnInterval;

    /// <summary>
    /// Represents the settings for enemies in a specific wave, containing various enemy configurations.
    /// </summary>
    /// <remarks>
    /// This variable is part of the <see cref="WaveSettings"/> class and holds an array of <see cref="EnemiesPerWaveSettings"/> for that wave.
    /// Each entry defines the type of enemy and its spawn probability within the specified wave.
    /// </remarks>
    [SerializeField]
    public EnemiesPerWaveSettings[] enemies;
}

/// <summary>
/// Manages and tracks the state of all enemies in the game.
/// </summary>
/// <remarks>
/// This class utilizes the Singleton pattern.
/// It provides functionality to manage the count of active enemies and update the state when an enemy is killed.
/// </remarks>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// Singleton Instance of the <see cref="EnemiesController"/> class.
    /// </summary>
    public static EnemySpawner Instance;

    /// <summary>
    /// Represents the configuration for multiple waves of enemies in the game.
    /// Each wave contains a specific number of enemies, spawn intervals,
    /// and settings for enemy types and their spawn probabilities.
    /// </summary>
    /// <remarks>
    /// Configured via the Inspector with support for list editing and
    /// detailed settings for each wave.
    /// </remarks>
    [SerializeField]
    [ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = false, ShowIndexLabels = true)]
    private WaveSettings[] waves;

    /// <summary>
    /// Defines the radius within which enemies are spawned around the player.
    /// This value is used to calculate a random spawn location near the player,
    /// ensuring enemies are generated within a certain distance of the player's position.
    /// </summary>
    [SerializeField]
    private float spawnRadius = 10f;

    /// <summary>
    /// Specifies the percentage of enemies remaining at which a new wave of enemies is initiated.
    /// </summary>
    [SerializeField]
    [Unit(Units.Percent)]
    private int thresholdToStartNewWave = 10;

    /// <summary>
    /// Represents the index of the current wave in the enemy spawning system.
    /// </summary>
    private int _currentWave;

    /// <summary>
    /// Represents the count of currently active enemies in the game at any given time.
    /// </summary>
    private int _activeEnemiesCount;

    /// <summary>
    /// Reference to the player's <see cref="Transform"/> component.
    /// </summary>
    private Transform _player;

    /// <summary>
    /// Initializes Singleton pattern by assigning Instance variable.
    /// </summary>
    private void Awake()
    {
        if (!Instance || Instance != this)
        {
            Instance = this;
        }
    }

    /// <summary>
    /// Starts the initialization process for the enemies' controller.
    /// This includes retrieving the player reference, initiating wave spawning through a coroutine, and storing the active enemies count for the current wave.
    /// </summary>
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

        // Validate waves array and _currentWave index before accessing
        if (waves != null && waves.Length > 0 && _currentWave >= 0 && _currentWave < waves.Length)
        {
            _activeEnemiesCount = waves[_currentWave].numberOfEnemies;
            StartCoroutine(SpawnWave());
        }
        else
        {
            Debug.LogError("Waves array is not set up correctly or current wave index is invalid.");
        }
    }


    /// <summary>
    /// Updates the logic for progressing through waves of enemies in the game.
    /// Checks the active enemy count against a threshold and advances to the next wave
    /// if conditions are met. Begins spawning a new wave of enemies when progression occurs.
    /// </summary>
    private void Update()
    {
        if (waves != null && waves.Length != 0)
        {
            if (_activeEnemiesCount <= waves[_currentWave].numberOfEnemies * thresholdToStartNewWave/100 && _currentWave <= waves.Length - 2)
            {
                _currentWave++;
                StartCoroutine(SpawnWave());
                _activeEnemiesCount = waves[_currentWave].numberOfEnemies;
            }
        }
    }

    /// <summary>
    /// Spawns a wave of enemies, based on the current wave settings.
    /// Each enemy is instantiated at a random position within a specified spawn radius
    /// around the player, and spawning is spaced out by the defined interval.
    /// </summary>
    private IEnumerator SpawnWave()
    {
        if (waves == null || waves.Length == 0) yield break;
        
        for (int i = 0; i < waves[_currentWave].numberOfEnemies; i++)
        {
            var randomSpawnPosition = GetRandomSpawnPositionNearPlayer();
            var enemyPrefab = GetRandomPrefab(waves[_currentWave].enemies);
            if (randomSpawnPosition != Vector3.zero && enemyPrefab)
            {
                Instantiate(enemyPrefab, randomSpawnPosition, Quaternion.identity);
                yield return new WaitForSeconds(waves[_currentWave].spawnInterval);
            }
            else
            {
                i--;
                yield return null;
            }
        }
    }

    /// <summary>
    /// Generates a random spawn position near the player within the defined spawn radius.
    /// Ensures the position is valid using Unity's navigation mesh.
    /// </summary>
    /// <returns>
    /// A <c>Vector3</c> representing a valid spawn position near the player.
    /// If no valid position is found, returns <c>Vector3.zero</c>.
    /// </returns>
    private Vector3 GetRandomSpawnPositionNearPlayer()
    {
        if (_player != null)
        {
            var randomDirection = _player.position + Random.insideUnitSphere * spawnRadius;

            if (NavMesh.SamplePosition(randomDirection, out var hit, 50f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }
        return Vector3.zero;
    }


    /// <summary>
    /// Selects a random enemy prefab from the provided array of enemy wave settings
    /// based on their spawn probabilities.
    /// </summary>
    /// <param name="waveEnemiesSettings">An array of settings defining possible enemy prefabs
    /// and their associated spawn probabilities for the current wave.</param>
    /// <returns>The selected <see cref="EnemyController"/> prefab if available, or null if no valid selection can be made.</returns>
    private EnemyController GetRandomPrefab(EnemiesPerWaveSettings[] waveEnemiesSettings)
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

    /// <summary>
    /// Decreases the count of currently active enemies in the game.
    /// </summary>
    /// <remarks>
    /// This method is called when an enemy is killed to update the internal count of active enemies.
    /// It assumes that the enemy object is already destroyed or will be destroyed upon its invocation.
    /// </remarks>
    public void EnemyKilled()
    {
        _activeEnemiesCount--;
    }
}
