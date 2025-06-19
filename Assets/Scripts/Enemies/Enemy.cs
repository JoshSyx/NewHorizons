using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : Health
{
    [SerializeField] private float damage;
    [SerializeField] private float attackCooldown;

    private float _nextAttackTime;
    private NavMeshAgent _agent;
    private Transform _player;
    private Animator _animator;
    private bool _isMoving;
    
    private static readonly int MovingParam = Animator.StringToHash("Moving");
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    private void Awake()
    { 
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
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
    }

    private void Update()
    {
        if (_player)
        {
            _agent.destination = _player.position;
            _animator.SetBool(MovingParam, !IsAgentReachedDestination());
            if (IsAgentReachedDestination() && _nextAttackTime < Time.time)
            {
                _animator.SetTrigger(AttackParam);
            }
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }
    
    private bool IsAgentReachedDestination()
    {
        return !_agent.pathPending &&
               _agent.remainingDistance <= _agent.stoppingDistance &&
               (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f);
    }

    public void Attack()
    {
        if (!_player)
        {
            Debug.LogError("Player not found!");
            return;
        }
        Debug.Log("Attacking");
        Combat.ManageHit(_player.gameObject, damage);
        _nextAttackTime = Time.time + attackCooldown;
    }
    
    public override void Kill()
    {
        EnemiesController.Instance.EnemyKilled(this);
        Destroy(gameObject);
    }

}

