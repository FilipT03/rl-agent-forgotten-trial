using UnityEngine;
using UnityEngine.AI;

public class EnemyAI: MonoBehaviour
{
    NavMeshAgent agent;

    Transform player;
    //public GameObject spawner;

    [SerializeField]
    LayerMask whatIsGround;//, whatIsPlayer;
    AudioSource audioSource;

    [SerializeField] float health;
    [SerializeField] float strenght;
    bool isDead = false;
    //int harvestHits = 5;
    //int knifeHits = 0;

    // Patroling
    Vector3 walkPoint, yOffset;
    bool walkPointSet;
    [SerializeField] Vector2 walkPointRange;

    // Attacking
    [SerializeField] float timeBetweenAttacks, angrySpeed;
    float normalSpeed;
    bool alreadyAttacked;
    bool enteredAttack = false;

    // States
    [SerializeField] float sightRange, attackRange;
    [SerializeField] float keepAggroTime;
    bool playerInSightRange, playerInAttackRange;

    // Animator
    [SerializeField] Animator animator;

    float sinceAggro;
    bool postAggro;
    bool wasPlayerInSightRange;

    // Editor
    [SerializeField] bool drawSight, drawAttack, drawMovement;

    enum State
    {
        patroling, chasing, attacking
    }
    State state;

    private void Awake()
    {
        //spawner = GameObject.Find("Spawner");
        audioSource = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;
        if(sightRange <= attackRange)
            Debug.LogWarning("Sight range must be greater than attack range!");
        yOffset = new Vector3(0, walkPointRange.y, 0);
    }

    private void Start()
    {
        player = Player.instance.transform;
    }

    private void Update()
    {
        if (isDead) 
            return;

        if (postAggro)
        {
            //Debug.Log("I am still angry");
            sinceAggro += Time.deltaTime;
            if (sinceAggro >= keepAggroTime)
            {
                sinceAggro = 0;
                postAggro = false;
                state = State.patroling;
            }
        }

        agent.speed = state == State.patroling ? normalSpeed : angrySpeed;

        // Check for sight and attack range
        wasPlayerInSightRange = playerInSightRange;
        //playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        //playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distanceToPlayer <= sightRange;
        playerInAttackRange = distanceToPlayer <= attackRange;
        
        if (wasPlayerInSightRange ^ playerInSightRange) // If the state changes
        {
            postAggro = wasPlayerInSightRange; // we set the postAggro to true, if the player just left the sight range.
            sinceAggro = 0;
        }

        if (playerInAttackRange)
        {
            //animator.SetBool("Walking", false);
            //animator.SetBool("Running", false);
            //if(!enteredAttack)
            //{
            //    enteredAttack = true;
            //    Invoke(nameof(ResetAttack), timeBetweenAttacks / 3);
            //}
            state = State.attacking;
            AttackPlayer();
        }
        else if (playerInSightRange || postAggro) // If the enemy sees the player and isn't too close, it chases the player. Same happens if they are still angry.
        {
            state = State.chasing;
            ChasePlayer();
            //animator.SetBool("Walking", false);
            //animator.SetBool("Running", true);
            //animator.SetBool("Idle", false);
        }
        else // If the enemy doesn't see the player, it wanders.
        {
            state = State.patroling;
            Patroling();
            //animator.SetBool("Walking", true);
            //animator.SetBool("Running", false);
            //animator.SetBool("Idle", false);
        }
        //else
        //{
        //    alreadyAttacked = true;
        //    enteredAttack = false;
        //}
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 2f)
            walkPointSet = false;
    }
    private void SearchWalkPoint()
    {
        //Calculate random point in range
        float randomZ = Random.Range(-walkPointRange.x, walkPointRange.x);
        float randomX = Random.Range(-walkPointRange.x, walkPointRange.x);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint + yOffset, -transform.up, out RaycastHit hit, 2 * walkPointRange.y, whatIsGround))
        {
            walkPointSet = true;
            walkPoint = hit.point + Vector3.up * 0.1f;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        //transform.LookAt(player, Vector3.up);

        Vector3 targetPostition = new Vector3(player.position.x,
                                       this.transform.position.y,
                                       player.position.z);
        transform.LookAt(targetPostition);

        if (!alreadyAttacked)
        {
            ///Attack code here
            //player.GetComponent<healthAndHunger>().TakeDamage(strenght);
            SoundManager.PlaySound(audioSource, SoundManager.Sound.enemyAttack);
            //animator.SetTrigger("Attack");
            Debug.Log("Attack!");
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            //animator.SetBool("Idle", true);
        }
    }

    /*private void AttackPlayer()
    {
        //Make sure enemy doesn't move
        agent.SetDestination(transform.position);

        //transform.LookAt(player, Vector3.up);

        Vector3 targetPostition = new Vector3(player.position.x,
                                       this.transform.position.y,
                                       player.position.z);
        transform.LookAt(targetPostition);

        if (!alreadyAttacked)
        {
            ///Attack code here
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            rb.AddForce(transform.up * 8f, ForceMode.Impulse);
            ///End of attack code

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }*/

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(int damage, bool knife = false, bool arrow = false)
    {
        health -= damage;

        //if(isDead && !arrow)
        //{
        //    harvestHits--;
        //    if (knife) knifeHits++;
        //    if (harvestHits <= 0)
        //    {
        //        float chance = Random.Range(0, 101);
        //        if(chance <= 10 * knifeHits)
        //            player.GetComponent<Inventory>().AddItem(new Item("hide"), 4);
        //        else if(chance <= 5 * knifeHits)
        //            player.GetComponent<Inventory>().AddItem(new Item("hide"), 5);
        //        else
        //            player.GetComponent<Inventory>().AddItem(new Item("hide"), 3);

        //        chance = Random.Range(0, 101);
        //        if (chance <= 5 * knifeHits)
        //            player.GetComponent<Inventory>().AddItem(new Item("infected_meat"), 2);
        //        else
        //            player.GetComponent<Inventory>().AddItem(new Item("infected_meat"), 1);
        //        CancelInvoke("DestroyObject");
        //        Destroy(gameObject);
        //    }
        //}
        if (health <= 0 && !isDead)
        {
            Die();
            //player.GetComponent<playerExperience>().AddXp(40);
        }
    }
    private void Die()
    {
        isDead = true;
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        //CapsuleCollider[] capsules = GetComponentsInChildren<CapsuleCollider>();
        //SphereCollider[] spheres = GetComponentsInChildren<SphereCollider>();
        //BoxCollider[] boxes = GetComponentsInChildren<BoxCollider>();
        //GetComponent<CapsuleCollider>().isTrigger = true;
        animator.enabled = false;
        agent.enabled = false;
        Debug.Log("I died.");
        //foreach (CapsuleCollider cc in capsules)
        //{
        //    if(cc.name == "Belly")
        //        cc.isTrigger = true;
        //    else
        //        cc.isTrigger = false;
        //}
        //foreach(SphereCollider sc in spheres)
        //    sc.isTrigger = false;
        //foreach (BoxCollider bc in boxes)
        //    bc.isTrigger = false;
        //foreach (Rigidbody rb in rigidbodies)
        //    rb.isKinematic = false;
        Invoke(nameof(DestroyObject), 180);
        //spawner.GetComponent<pigAndRabbitSpawning>().Invoke("SpawnPig", 60);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (drawSight)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
        if (drawAttack)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }
        if (drawMovement)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(2 * walkPointRange.x, 2 * walkPointRange.y, 2 * walkPointRange.x));
        }
    }
}
