using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround, whatIsPlayer;
    public LayerMask hitmask;

    [Header("Status")]
    private int currenthealth;
    public int maxHealth = 10;
    public HealthBar healthBar;
    public Animator animator;

    // Patroling
    public Vector3 walkPoint;
    bool walkPointSet;
    public float walkPointRange;

    // Attacking
    [Header("Attacking")]

    public float timeBetweenAttacks;
    public float spread=20f;
    bool alreadyAttacked;
    public TrailRenderer bulletTrail;
    public Transform gunTip;
    
    public int attackDamage;
    public GameObject breakEnemy;
    public GameObject CompleteEnemy;

    // States
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    AudioManager audioManager;

    private void Start()
    {
        currenthealth = maxHealth;
        healthBar.SetMaxHealth(currenthealth);
        agent.updateRotation = false; // Prevent NavMeshAgent from auto-rotating

    }

    private void Awake()
    {
        player = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
        breakEnemy.SetActive(false);
        CompleteEnemy.SetActive(true);

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
        {
            animator.SetTrigger("isWalking");
            agent.SetDestination(walkPoint);
            Vector3 direction = (walkPoint - transform.position).normalized;
            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            }
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
            walkPointSet = true;
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        animator.SetTrigger("isWalking");
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        animator.SetTrigger("isShooting");

        if (!alreadyAttacked)
        {
            Vector3 direction = (player.position - gunTip.position).normalized;
            direction +=new Vector3(
           Random.Range(-spread, spread) * 0.01f,
           Random.Range(-spread, spread) * 0.01f,
           Random.Range(-spread, spread) * 0.01f
       );
            direction.Normalize();
            RaycastHit hit;
            TrailRenderer trail = Instantiate(bulletTrail, gunTip.position, Quaternion.identity);
            audioManager.PlaySFX(audioManager.enemyFire);

            if (Physics.Raycast(gunTip.position, direction, out hit, attackRange, hitmask))
            {
                if (hit.transform == player)
                {
                    player.GetComponent<PlayerMovement>().TakeDamage(attackDamage);
                }
                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal));
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, gunTip.position + direction * attackRange, Vector3.zero));
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint, Vector3 hitNormal)
    {
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(startPosition, hitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= 50f * Time.deltaTime;
            yield return null;
        }
        trail.transform.position = hitPoint;
        Destroy(trail.gameObject, trail.time);
    }

    public void TakeDamage(int damage)
    {
        currenthealth -= damage;
        healthBar.SetHealth(currenthealth);
        Debug.Log("Enemy took damage: " + damage);

        if (currenthealth <= 0)
        {
            breakEnemyActive();
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    private void breakEnemyActive()
    {
        alreadyAttacked = true;
        CompleteEnemy.SetActive(false);
        breakEnemy.SetActive(true);
        audioManager.PlaySFX(audioManager.enemyDeath);
        Invoke("DestroyEnemy", 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
