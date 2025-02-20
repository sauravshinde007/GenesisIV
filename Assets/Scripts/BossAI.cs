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
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float bulletFireRate = 0.1f; // Time between shots
    private bool isFiring = false;
    private float playerSpeedThreshold = 5f;

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

        float fireRate = bulletFireRate;
        if (player.GetComponent<Rigidbody>().velocity.magnitude > playerSpeedThreshold)
        {
            fireRate *= 1.5f; // Add delay if player is moving fast
        }

        for (int i = 0; i < 10; i++) // Fire 10 bullets
        {
            ShootBullet();
            yield return new WaitForSeconds(fireRate);
        }

        isFiring = false;
    }

    void ShootBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = firePoint.forward * bulletSpeed;
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
