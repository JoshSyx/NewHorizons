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

    [Tooltip("The amount of damage that the enemy will inflict on a target during an attack.")]
    [SerializeField] private float damage;

    [Tooltip("The time interval, in seconds, that must pass before the enemy can attack again.")]
    [SerializeField] private float attackCooldown;

    private float _nextAttackTime;
    private NavMeshAgent _agent;
    private Transform _player;
    private Animator _animator;

    private static readonly int MovingParam = Animator.StringToHash("Moving");
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    /// <summary>
    /// Initializes essential components for the Enemy class: NavMeshAgent, Animator, Combat.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // Initializes Health base class (e.g. material setup)
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _combat = GetComponent<Combat>();
    }

    /// <summary>
    /// Attempts to locate the player GameObject using its tag "Player".
    /// </summary>
    private void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            _player = playerObj.transform;
        }
    }

    /// <summary>
    /// Handles movement towards the player and attacks when in range and cooldown is over.
    /// </summary>
    private void FixedUpdate()
    {
        if (!_player)
        {
            return;
        }

        _agent.destination = _player.position;

        if (IsAgentReachedDestination() && _nextAttackTime < Time.time)
        {
            _animator.SetTrigger(AttackParam);
        }

        _animator.SetBool(MovingParam, !IsAgentReachedDestination());
    }

    /// <summary>
    /// Determines whether the NavMeshAgent has reached its destination.
    /// </summary>
    private bool IsAgentReachedDestination()
    {
        return !_agent.pathPending &&
               _agent.remainingDistance <= _agent.stoppingDistance &&
               (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f);
    }

    /// <summary>
    /// Performs an attack on the player and sets the cooldown timer.
    /// </summary>
    public void Attack()
    {
        if (!_player)
        {
            return;
        }

        _combat.ManageHit(_player.gameObject, damage);
        _nextAttackTime = Time.time + attackCooldown;
    }

    /// <summary>
    /// Called when the enemy dies. Stops movement, notifies the controller, and triggers death visual logic.
    /// </summary>
    public override void Kill()
    {
        if (_agent)
        {
            _agent.isStopped = true;
        }

        EnemiesController.Instance.EnemyKilled();
        base.Kill(); // Handles visuals and object destruction
    }
}
