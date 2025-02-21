using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public HealthBar healthBar;

    [Header("Attack Settings")]
    public float attackCooldown = 3f; // Time between attacks
    private bool isAttacking = false;
    private float rotationSpeed = 5f;

    [Header("Machine Gun")]
    public Transform firePoint;
    public float bulletFireRate = 0.1f; // Time between each bullet
    public int bulletsPerBurst = 10; // Number of bullets per attack
    public float bulletSpread = 2f; // Spread factor
    public float bulletRange = 50f; // Maximum bullet range
    public int bulletDamage = 10; // Damage per bullet
    public LayerMask hitMask;

    public ParticleSystem muzzleFlash;
    public TrailRenderer BulletTrail;
    private bool isFiring = false;

    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        RotateTowardsPlayer();

        if (!isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;

        // Move towards the player
        agent.SetDestination(player.position);
        animator.SetBool("Walk", true);

        yield return new WaitForSeconds(2f); // Wait before attacking

        // Stop moving and attack
        agent.SetDestination(transform.position);
        animator.SetBool("Walk", false);

        ChooseAttack();

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Keep rotation only in the horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, 90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void ChooseAttack()
    {
        if (currentHealth > maxHealth / 2) // Above 50% health
        {
            int attackType = Random.Range(0, 2); // 0 = Missile, 1 = Bullet
            if (attackType == 0)
                MissileLaunch();
            else
                StartCoroutine(BulletFireRoutine());
        }
        else // Below 50% health
        {
            int attackType = Random.Range(0, 3); // 0 = Missile, 1 = Bullet, 2 = Laser
            if (attackType == 0)
                MissileLaunch();
            else if (attackType == 1)
                StartCoroutine(BulletFireRoutine());
            else
                LaserFire();
        }
    }

    void MissileLaunch()
    {
        Debug.Log("Boss Launching Missile!");
        animator.SetTrigger("MissileLaunch");
        // Implement missile attack logic here
    }

    IEnumerator BulletFireRoutine()
    {
        isFiring = true;
        animator.SetTrigger("BulletFire");

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(bulletFireRate); // Machine gun firing delay
        }

        isFiring = false;
    }

    void ShootBullet()
    {
        if (player == null) return;

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(); // Stop any existing particles
            muzzleFlash.Play(); // Play muzzle flash effect
        }

        // Add slight spread to machine gun fire
        Vector3 fireDirection = (player.position - firePoint.position).normalized;

        fireDirection += new Vector3(
            Random.Range(-bulletSpread, bulletSpread) * 0.01f,
            Random.Range(-bulletSpread, bulletSpread) * 0.01f,
            Random.Range(-bulletSpread, bulletSpread) * 0.01f
        );

        RaycastHit hit;
    
        if (Physics.Raycast(firePoint.position, fireDirection, out hit, bulletRange, hitMask))
        {
            // Apply damage if it hits the player
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerMovement>().TakeDamage(bulletDamage);
            }
        }
        StartCoroutine(SpawnBulletTrail(hit.point));
    }

    IEnumerator SpawnBulletTrail(Vector3 hitPoint)
    {
        TrailRenderer bulletTrail = Instantiate(BulletTrail, firePoint.position, Quaternion.identity);
        float elapsedTime = 0f;
        float bulletSpeed = 100f;
        Vector3 startPosition = firePoint.position;

        while (elapsedTime < 1f) // Make trail move smoothly
        {
            bulletTrail.transform.position = Vector3.Lerp(startPosition, hitPoint, elapsedTime * bulletSpeed / bulletRange);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bulletTrail.transform.position = hitPoint; // Ensure it reaches final position
        Destroy(bulletTrail.gameObject, bulletTrail.time); // Destroy after trail effect ends
    }

    void LaserFire()    
    {
        Debug.Log("Boss Firing Laser!");
        animator.SetTrigger("LaserFire");
        // Implement laser attack logic here
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");
        Destroy(gameObject);
    }
}
