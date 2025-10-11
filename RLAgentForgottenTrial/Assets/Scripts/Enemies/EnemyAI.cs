using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI: MonoBehaviour
{
    [Header("References")]
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public Transform player;
    public LayerMask groundMask;
    
    [SerializeField] private AudioSource audioSource;

    [Header("Stats")]
    public float health = 100f;
    public float strength = 10f;
    public float sightRange = 10f;
    public float attackRange = 2f;
    public float keepAggroTime = 5f;
    public float timeBetweenAttacks = 1.5f;
    public float angrySpeed = 6f;
    public float normalSpeed = 3.5f;

    private float startHealth;

    [Header("Debug")]
    public bool drawGizmos = false;

    public EnemyStateMachine StateMachine { get; private set; }

    [Header("Patrol")]
    public Vector2 walkPointRange = new Vector2(5f, 5f);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        startHealth = health;
    }

    private void Start()
    {
        player = GetComponentInParent<TrainingReferences>().player.transform;
        StateMachine = new EnemyStateMachine(this);
        StateMachine.ChangeState(new EnemyPatrolState());
    }

    private void Update()
    {
        StateMachine?.UpdateState();
    }

    public void ResetValues()
    {
        health = startHealth;
        StateMachine?.ChangeState(new EnemyPatrolState());
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health < 0)
            StateMachine?.ChangeState(new EnemyDeadState());
    }

    public bool IsDead()
    {
        return health < 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }

    public void StartAttackCooldown(float delay, Action onCooldownComplete)
    {
        StartCoroutine(AttackCooldownCoroutine(delay, onCooldownComplete));
    }

    private IEnumerator AttackCooldownCoroutine(float delay, Action onCooldownComplete)
    {
        yield return new WaitForSeconds(delay);
        onCooldownComplete?.Invoke();
    }

    public bool IsPlayerInSight()
    {
        return Vector3.Distance(transform.position, player.position) <= sightRange;
    }

    public bool IsPlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }
}
