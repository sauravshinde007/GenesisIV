using System.Collections;
using UnityEngine;

public class StaticEnemy : MonoBehaviour, IDamageable

{
    [Header("References")]
    public Transform player; // Reference to the player
    public Transform gunTip; // Where bullets originate
    public Animator animator; // Enemy animation
    public TrailRenderer bulletTrail; // Bullet trail effect

    [Header("Enemy Settings")]
    public float detectionRange = 15f; // Enemy sight range
    public float rotationSpeed = 5f; // Enemy rotation speed
    public int shotDamage = 2; // Damage per shot
    public LayerMask obstacleMask; // Set this to include walls
    public GameObject breakEnemy;
    public GameObject CompleteEnemy;
    private int currenthealth;
    public int maxHealth = 10;
    public HealthBar healthBar;

    [Header("Shooting Settings")]
    public float attackRange = 50f; // Max shooting distance
    public float spread = 0.5f; // Bullet spread
    
    public float shootingInterval = 2f; // Time between shots
    private bool canShoot = true;
    void Awake()
    {
        currenthealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange && HasLineOfSight())
        {
            RotateTowardsPlayer();

            if (canShoot)
            {
                StartCoroutine(ShootRaycast());
            }
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    bool HasLineOfSight()
    {
        RaycastHit hit;
        Vector3 direction = player.position - transform.position;

        if (Physics.Raycast(transform.position, direction, out hit, detectionRange, obstacleMask))
        {
            return hit.transform == player;
        }

        return false;
    }

    private IEnumerator ShootRaycast()
    {
        canShoot = false;

        // Play shooting animation
        if (animator) animator.SetTrigger("shooting");

        Vector3 direction = (player.position - gunTip.position).normalized;

        // Apply random spread
        direction += new Vector3(
            Random.Range(-spread, spread) * 0.01f,
            Random.Range(-spread, spread) * 0.01f,
            Random.Range(-spread, spread) * 0.01f
        );
        direction.Normalize();

        RaycastHit hit;
        TrailRenderer trail = Instantiate(bulletTrail, gunTip.position, Quaternion.identity);

        if (Physics.Raycast(gunTip.position, direction, out hit, attackRange, obstacleMask))
        {
            if (hit.transform == player)
            {
                // Ensure the player has a damage script
                player.GetComponent<PlayerMovement>().TakeDamage(shotDamage); // Apply damage to the player
            }

            // Show bullet impact
            StartCoroutine(SpawnTrail(trail, hit.point));
        }
        else
        {
            StartCoroutine(SpawnTrail(trail, gunTip.position + direction * attackRange));
        }

        yield return new WaitForSeconds(shootingInterval);
        canShoot = true;
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, Vector3 hitPoint)
    {
        Vector3 startPosition = trail.transform.position;
        float distance = Vector3.Distance(startPosition, hitPoint);
        float traveledDistance = 0f;

        while (traveledDistance < distance)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hitPoint, traveledDistance / distance);
            traveledDistance += 50f * Time.deltaTime; // Bullet speed
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

   
    private void breakEnemyActive()
    {
        canShoot = false;
        CompleteEnemy.SetActive(false);
        breakEnemy.SetActive(true);
        Invoke("DestroyEnemy", 0.5f);
    }
    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

}
