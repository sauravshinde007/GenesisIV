using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class GunShoot : MonoBehaviour
{
    [Header("Shooting")]
    public Transform gunTip;
    public float bulletForce = 50f;
    public float spread;
    public float timeBetweenShooting;
    public int magazineSize;
    public float reloadTime;
    public TrailRenderer BulletTrail;
    public ParticleSystem ShootingSystem, ImpactParticleSystem;
    public LayerMask Mask;
    public Animator gunAnimator;
    public TextMeshProUGUI ammunitionDisplay;
    public PlayerMovement playerMovement;

    private Queue<TrailRenderer> trailPool = new Queue<TrailRenderer>();
    public int poolSize = 10;
    private float LastShootTime;
    private int bulletsLeft;
    private bool reloading;
    private bool readyToShoot = true;

    AudioManager audioManager;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        InitializeTrailPool();
    }

    private void InitializeTrailPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            TrailRenderer trail = Instantiate(BulletTrail);
            trail.gameObject.SetActive(false);
            trailPool.Enqueue(trail);
        }
    }

    private TrailRenderer GetTrail()
    {
        if (trailPool.Count > 0)
        {
            TrailRenderer trail = trailPool.Dequeue();
            trail.gameObject.SetActive(true);
            return trail;
        }
        return Instantiate(BulletTrail);
    }

    private void ReturnTrail(TrailRenderer trail)
    {
        trail.gameObject.SetActive(false);
        trailPool.Enqueue(trail);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && readyToShoot && !reloading && bulletsLeft > 0)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        if (readyToShoot && Input.GetMouseButton(0) && !reloading && bulletsLeft <= 0)
        {
            Reload();
        }
        UpdateUI();
        HandleWalkingAnimation();
    }
    private void HandleWalkingAnimation(){
        gunAnimator.SetBool("isWalking", playerMovement.isWalking);     
    }
    private void UpdateUI()
    {
        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft.ToString());

    }

    public void Shoot()
    {

        if (LastShootTime + timeBetweenShooting < Time.time)
        {
            audioManager.PlaySFX(audioManager.gunShot);

            gunAnimator.ResetTrigger("isShooting");
            gunAnimator.SetTrigger("isShooting");
            ShootingSystem.Play();
            Vector3 direction = GetDirection();
            RaycastHit hit;
            TrailRenderer trail = GetTrail();
            trail.transform.position = gunTip.position;

            
            if (Physics.Raycast(gunTip.position, direction, out hit, float.MaxValue, Mask))
            {
                //Check if we hit an enemy
                IDamageable damagable = hit.collider.GetComponent<IDamageable>();
                if(damagable != null)
                {
                    damagable.TakeDamage(3);
                }

                StartCoroutine(SpawnTrail(trail, hit.point, hit.normal, true));
            }
            else
            {
                StartCoroutine(SpawnTrail(trail, gunTip.position + direction * 100, Vector3.zero, false));
            }

            bulletsLeft--;
            LastShootTime = Time.time;
            
        }
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = gunTip.forward;
        direction += new Vector3(Random.Range(-spread, spread), Random.Range(-spread, spread), 0);
        direction.Normalize();
        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(startPosition, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));
            remainingDistance -= bulletForce * Time.deltaTime;
            yield return null;
        }
        Trail.transform.position = HitPoint;

        if (MadeImpact)
        {
            //Instantiate(ImpactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }
        
        ReturnTrail(Trail);
    }

    private void Reload()
    {
        if (reloading) return;

        reloading = true;
        readyToShoot = false;
        

        if (gunAnimator != null)
        {
            gunAnimator.SetBool("isReloading",true);
            audioManager.PlaySFX(audioManager.reload);
        }

        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
        readyToShoot = true;
        gunAnimator.SetBool("isReloading",false);
    }
}
