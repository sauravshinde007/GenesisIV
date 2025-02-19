using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public Camera fpsCamera;
    public Transform gunTip, player;
    public GameObject crosshair;
    public PlayerMovement playerMovement;
    public Rigidbody playerRb;
    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;
    public LayerMask whatIsGrappleable;
    private Vector3 grapplePoint;
    private bool grappling;
    [Header("Grappling Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Swing Variables")]
    public LayerMask swingable;
    public float maxSwingDistance = 50f;
    private SpringJoint joint;
    // private Vector3 grapplePoint;
    private Vector3 currentGrapplePosition;
    [Header("OdmGear")]

    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Trampoline")]
    public LayerMask trampoline;
    public float maxCheckDistance;
    public float trampolineForce;
    private Vector3 trampolinePosition;
    
    void Update()
    {
        HandleInput();
        
        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1)) StartSwingorGrapple();
        if (Input.GetMouseButtonUp(1) && isSwinging()) StopSwing();
        if (joint != null) OdmGearMovement();
        if (Input.GetMouseButtonDown(0))
        {
            CheckForTrampoline();
            
        }
    }

    
    bool isLookingDown(){
        float currentAngle = fpsCamera.transform.eulerAngles.x;
        if(currentAngle>180) currentAngle -= 360;
        Debug.Log(currentAngle);
        return currentAngle>=60;
    }


    // Trampoline SYSTEM
    private bool CheckForTrampoline()
    {
        RaycastHit hit;

        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, maxCheckDistance, trampoline))
        {
            trampolinePosition = hit.point;
            Vector3 direction = trampolinePosition - fpsCamera.transform.position;
            // Vector3 direction = hit.normal;
            playerRb.velocity = Vector3.zero;
            Debug.Log("Direction" + direction.normalized);
            playerRb.AddForce(-direction.normalized * trampolineForce, ForceMode.VelocityChange);
            return true;
        }
        return false;

    }
    //Swing and Grapple SYSTEM
    void StartSwingorGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, maxSwingDistance, swingable))
        {
            StartSwing(hit);
        }
        else
        {
            StartGrapple();

        }
    }

    // Swinging SYSTEM
    void StartSwing(RaycastHit hit)
    {
        
        playerMovement.swinging = true;
        grapplePoint = hit.point;

        if (joint == null)
        {
            joint = player.gameObject.AddComponent<SpringJoint>();
        }

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        // customize values as you like
        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;
        currentGrapplePosition = gunTip.position;

        if (crosshair != null) crosshair.SetActive(false);

    }

    void StopSwing()
    {

        playerMovement.swinging = false;
        if (joint != null) Destroy(joint);

        if (crosshair != null) crosshair.SetActive(true);
    }
    private void OdmGearMovement()
    {
        // right
        if (Input.GetKey(KeyCode.D)) playerRb.AddForce(player.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) playerRb.AddForce(-player.right * horizontalThrustForce * Time.deltaTime);

        // forward
        if (Input.GetKey(KeyCode.W)) playerRb.AddForce(player.forward * horizontalThrustForce * Time.deltaTime);

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = grapplePoint - transform.position;
            playerRb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }
        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }


    public bool isSwinging()
    {
        return joint != null;
    }

    public Vector3 GetSwingPoint()
    {
        return grapplePoint;
    }

    // Grapple SYSTEM
    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        grappling = true;
        playerMovement.freeze = true;
        RaycastHit hit;
        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = fpsCamera.transform.position + fpsCamera.transform.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }


    }
    private void ExecuteGrapple()
    {
        playerMovement.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc;

        if (grapplePoint.y >= transform.position.y)
        {
            // Normal arc when grappling upwards
            highestPointOnArc = grapplePoint.y + overshootYAxis;
        }
        else if(!isLookingDown())
        {
            
            highestPointOnArc = transform.position.y+overshootYAxis;
        }else{
            StopGrapple();
            return;
        }

        playerMovement.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }
    public void StopGrapple()
    {
        playerMovement.freeze = false;

        grappling = false;
        playerMovement.fallMultiplier = 2.5f;

        grapplingCdTimer = grapplingCd;

        //lr.enabled = false;
    }
    public bool IsGrappling()
    {
        return grappling;
    }

}
