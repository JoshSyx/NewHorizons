using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Represents an enemy character within the game. Inherits from the <see cref="Health"/> class to include health-related behavior.
/// </summary>
/// <remarks>
/// This class utilizes Unity's <see cref="NavMeshAgent"/> and <see cref="Animator"/> components for movement and animation.
/// It is expected to be attached to enemy GameObjects.
/// </remarks>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : Health
{

    private Combat _combat;
    /// <summary>
    /// The amount of damage that the enemy can inflict on a target during an attack.
    /// </summary>
    [Tooltip("The amount of damage that the enemy will inflict on a target during an attack.")]
    [SerializeField] 
    private float damage;

    /// <summary>
    /// The time interval, in seconds, that must pass before the enemy can attack again.
    /// </summary>
    [Tooltip("The time interval, in seconds, that must pass before the enemy can attack again.")]
    [SerializeField]
    private float attackCooldown;

    /// <summary>
    /// Represents the next time the enemy can perform an attack in seconds.
    /// </summary>
    private float _nextAttackTime;

    /// <summary>
    /// Reference to the <see cref="NavMeshAgent"/> component attached to the enemy GameObject.
    /// </summary>
    private NavMeshAgent _agent;

    /// <summary>
    /// Reference to the player's <see cref="Transform"/> component.
    /// </summary>
    private Transform _player;

    /// <summary>
    /// Reference to the <see cref="Animator"/> component attached to the enemy GameObject.
    /// </summary>
    private Animator _animator;

    /// <summary>
    /// Indicates whether the enemy is currently moving.
    /// </summary>
    private bool _isMoving;

    /// <summary>
    /// Represents the hash identifier for the "Moving" bool animation parameter.
    /// </summary>
    private static readonly int MovingParam = Animator.StringToHash("Moving");

    /// <summary>
    /// Represents the hash identifier for the "Attack" animation trigger.
    /// </summary>
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    /// <summary>
    /// Initializes essential components for the Enemy class: <see cref="NavMeshAgent"/> and
    /// <see cref="Animator"/>.
    /// </summary>
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _combat = GetComponent<Combat>();
    }

    /// <summary>
    /// Initializes the enemy at the start of the game.
    /// Attempts to locate the player GameObject using its tag "Player".
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
    }

    /// <summary>
    /// Handles movement towards the player, updates animations,
    /// and set attack trigger when the enemy reaches the player and the attack cooldown has elapsed.
    /// </summary>
    private void FixedUpdate()
    {
        if (_player)
        {
            _agent.destination = _player.position;
            if (IsAgentReachedDestination() && _nextAttackTime < Time.time)
            {
                _animator.SetTrigger(AttackParam);
            }
            _animator.SetBool(MovingParam, !IsAgentReachedDestination());
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }

    /// <summary>
    /// Determines whether the NavMeshAgent has reached its destination.
    /// </summary>
    /// <returns>True if the agent has reached the destination or the path is complete; otherwise, false.</returns>
    private bool IsAgentReachedDestination()
    {
        return !_agent.pathPending &&
               _agent.remainingDistance <= _agent.stoppingDistance &&
               (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f);
    }

    /// <summary>
    /// Performs an attack on the current player target if available.
    /// Deals damage to the player and sets the cooldown for the next attack.
    /// </summary>
    /// <remarks>
    /// The method uses the <see cref="Combat.ManageHit"/> function to apply damage to the player.
    /// </remarks>
    /// <example>
    /// Use as the animation event to attack.
    /// </example>
    public void Attack()
    {
        if (!_player)
        {
            Debug.LogError("Player not found!");
            return;
        }
        Debug.Log("Attacking");
        _combat.ManageHit(_player.gameObject, damage);
        _nextAttackTime = Time.time + attackCooldown;
    }

    /// <summary>
    /// Decrements the count of active enemies through the <see cref="EnemiesController"/> and destroys
    /// the enemy GameObject when health reaches 0.
    /// </summary>
    public override void Kill()
    {
        EnemiesController.Instance.EnemyKilled();
        Destroy(gameObject);
    }

}

