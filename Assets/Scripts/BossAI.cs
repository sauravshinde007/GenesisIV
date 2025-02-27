using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Boss Activation")]
    public bool isPlayerInBossArea = false;

    [Header("Health System")]
    public int maxHealth = 100;
    private int currentHealth;
    public HealthBar healthBar;

    [Header("Attack Settings")]
    public float attackCooldown = 3f; // Time between attacks
    private bool isAttacking = false;
    private float rotationSpeed = 3f;

    [Header("Machine Gun")]
    public Transform firePoint;
    public float bulletFireRate = 0.1f; // Time between each bullet
    public int bulletsPerBurst = 10; // Number of bullets per attack
    public float bulletSpread = 200f; // Spread factor
    public float bulletRange = 50f; // Maximum bullet range
    public int bulletDamage = 10; // Damage per bullet
    public LayerMask hitMask;

    public ParticleSystem muzzleFlash;
    public TrailRenderer BulletTrail;
    [Header("Missile Attack")]
    public GameObject missilePrefab;
    public Transform missileLaunchPoint;
    public GameObject target;

    public float delay = 0.5f;

    [Header("Laser Attack")]
    public LineRenderer laserLine;
    public Transform laserFirePoint;
    public float laserDuration = 1f;
    public int laserDamage = 20;
    public float laserRange = 50f;
    public LayerMask playerLayer;

    AudioManager audioManager;


    public float laserRotationSpeed = 2f;
    private void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(currentHealth);
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void Update()
    {
        if(!isPlayerInBossArea) return;

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

        animator.SetTrigger("MissileLaunch");
        Invoke("LaunchMissle", delay);

    }
    void LaunchMissle()
    {
        if (missilePrefab != null && missileLaunchPoint != null)
        {
            GameObject missileInstance = Instantiate(missilePrefab, missileLaunchPoint.position, missileLaunchPoint.rotation);
            Missile missileScript = missileInstance.GetComponent<Missile>();
            if (missileScript != null)
            {
                missileScript.SetTarget(target);
            }
        }

    }
    IEnumerator BulletFireRoutine()
    {
        animator.SetTrigger("BulletFire");

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootBullet();
            yield return new WaitForSeconds(bulletFireRate); // Machine gun firing delay
        }
    }

    void ShootBullet()
    {
        if (player == null) return;

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
            muzzleFlash.Play();
        }

        // Compute the fire direction
        Vector3 fireDirection = (player.position - firePoint.position).normalized;

        // Apply slight spread
        fireDirection += new Vector3(
            Random.Range(-bulletSpread, bulletSpread) * 0.01f,
            Random.Range(-bulletSpread, bulletSpread) * 0.01f,
            Random.Range(-bulletSpread, bulletSpread) * 0.01f
        );

        fireDirection.Normalize(); // Ensure the direction is properly normalized

        RaycastHit hit;
        Vector3 targetPoint = firePoint.position + fireDirection * bulletRange; // Default target if nothing is hit

        if (Physics.Raycast(firePoint.position, fireDirection, out hit, bulletRange, hitMask))
        {
            targetPoint = hit.point; // Update target point if a hit occurs

            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerMovement>().TakeDamage(bulletDamage);
            }
        }

        StartCoroutine(SpawnBulletTrail(targetPoint));
    }

    IEnumerator SpawnBulletTrail(Vector3 hitPoint)
    {
        TrailRenderer bulletTrail = Instantiate(BulletTrail, firePoint.position, Quaternion.identity);
        float elapsedTime = 0f;
        float bulletSpeed = 100f;
        Vector3 startPosition = firePoint.position;

        audioManager.PlaySFX(audioManager.machineGun);

        while (elapsedTime < 1f) // Smooth movement
        {
            bulletTrail.transform.position = Vector3.Lerp(startPosition, hitPoint, elapsedTime * bulletSpeed / bulletRange);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bulletTrail.transform.position = hitPoint;
        Destroy(bulletTrail.gameObject, bulletTrail.time);
    }




    void LaserFire()
    {
        Debug.Log("Boss Firing Laser!");
        animator.SetTrigger("LaserFire");
        Invoke("StartLaser", 0.5f);

    }
    void StartLaser()
    {
        StartCoroutine(FireLaser());
    }
    float GetRandomSweepAngle()
    {
        if (Random.Range(0, 2) == 0)
        {
            return Random.Range(-30f, -10f);
        }
        else
        {
            return Random.Range(10f, 30f);
        }
    }
    IEnumerator FireLaser()
    {
        laserLine.enabled = true;

        Vector3 directionToTarget = (player.position - laserFirePoint.position).normalized;
        Quaternion initialRotation = Quaternion.LookRotation(directionToTarget);

        float laserSweepAngle = GetRandomSweepAngle();

        float elapsedTime = 0f;



       
        Quaternion startRotation = Quaternion.Euler(initialRotation.eulerAngles.x, initialRotation.eulerAngles.y + laserSweepAngle, initialRotation.eulerAngles.z);


        laserFirePoint.rotation = startRotation; 

        Vector3 fixedHitPoint = Vector3.zero; 
        bool hitDetected = false; 

        while (elapsedTime < laserDuration)
        {
            laserLine.SetPosition(0, laserFirePoint.position);
            float yAngle = initialRotation.eulerAngles.y + 60 * (elapsedTime / laserDuration);
            Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles.x, yAngle, initialRotation.eulerAngles.z);

            
            laserFirePoint.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / laserDuration);

            Vector3 laserDirection = laserFirePoint.forward;

            RaycastHit hit;

            audioManager.PlaySFX(audioManager.laserBeam);

            if (Physics.Raycast(laserFirePoint.position, laserDirection, out hit, laserRange, playerLayer))
            {
                if (!hitDetected) 
                {
                    fixedHitPoint = hit.point;
                    hitDetected = true;
                }

                
                Vector3 adjustedHitPoint = new Vector3(hit.point.x, fixedHitPoint.y, hit.point.z);
                laserLine.SetPosition(1, adjustedHitPoint);

                if (hit.collider.CompareTag("Player"))
                {
                    hit.collider.GetComponent<PlayerMovement>().TakeDamage(laserDamage);
                }
            }
            else
            {
                
                Vector3 sweepEndPoint = laserFirePoint.position + laserDirection * laserRange;
                laserLine.SetPosition(1, sweepEndPoint);
            }

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        laserLine.enabled = false;
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

        if (GameObject.FindObjectOfType<BossArena>() != null)
        {
            GameObject.FindObjectOfType<BossArena>().BossDefeated();
        }

        Destroy(gameObject);
    }
}
