using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float wallrunSpeed;
    public float swingSpeed;

   
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Health")]
    public int maxHealth = 50;
    public int currentHealth;
    public HealthBar healthBar;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Jump Tweaks")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    public Transform orientation;
    public PlayerCam cam;
    public float grappleFov = 95f;
    public Gun gun;

    float horizontalInput;
    float verticalInput;

    

    Rigidbody rb;
    private Vector3 gravity = Physics.gravity;

    public enum MovementState
    {
        freeze,
        Walking,
        grappling,
        swinging,
        Sprinting,
        //Crouching,
        WallRunning,
        Air,
    }
    public MovementState state;
    [HideInInspector]
    public bool wallrunning;
    public bool activeGrapple;
    public bool swinging;
    public bool freeze;
    public bool walking;
    public bool isWalking;
    public Vector3 moveDirection;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;

        //Health settings
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded && !activeGrapple)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

    }

    private void FixedUpdate()
    {
        MovePlayer();

        // if (!grounded) // Apply extra gravity when not grounded
        // {
        //     rb.AddForce(Vector3.down * (fallMultiplier * 10f), ForceMode.Acceleration);
        // }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //// start crouch
        //if (Input.GetKeyDown(crouchKey))
        //{
        //    transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
        //    rb.AddForce(Vector3.down*5f, ForceMode.Impulse);
        //}
        //// stop crouch
        //if (Input.GetKeyUp(crouchKey))
        //{
        //    transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        //}
    }

    private void StateHandler()
    {
        //Mode- Wall Running
        isWalking = false;
        if (wallrunning)
        {
            state = MovementState.WallRunning;
            moveSpeed = wallrunSpeed;
        }
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }
        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }

        // Mode - Swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }
        //Mode - Crouching
        //if (Input.GetKey(crouchKey))
        //{
        //    state = MovementState.Crouching;
        //    moveSpeed = crouchSpeed;
        //}
        //Mode - Sprinting
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.Sprinting;
            moveSpeed = sprintSpeed;
            isWalking = true;
            
        }
        //Mode - Walking
        else if (grounded &&(horizontalInput != 0 || verticalInput != 0))
        {
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
            isWalking = true;
        }
        //Mode - Air
        else
        {
            state = MovementState.Air;
        }
    }

    private void MovePlayer()
    {
        if (activeGrapple) return;
        if (swinging) return;
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // slope handling
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 5f * airMultiplier, ForceMode.Force);

        //turn of gravity on slope
        rb.useGravity = !OnSlope();
        if (!wallrunning) rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;
        //limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight*0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }
    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
        cam.DoFov(grappleFov);
    }
    public void ResetRestrictions()
    {
        activeGrapple = false;
        swinging = false;
        Physics.gravity = gravity;
    
        cam.DoFov(85f);
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

    private void Die()
    {
        Debug.Log("Player Died!");
        RestartScene();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            //gun.StopGrapple();
        }

        // Check if the player is hit by a bullet
        if (collision.gameObject.CompareTag("Bullet"))
        {

            TakeDamage(10);
            Destroy(collision.gameObject); // Destroy bullet on impact
        }

        // Check if the player collides with a death object
        if (collision.gameObject.CompareTag("Death"))
        {
            RestartScene();
        }
    }
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Mathf.Abs(Physics.gravity.y); // Ensure positive gravity
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        float timeToReach;
        float velocityY;
        Vector3 velocityXZ;

        if (displacementY > 0) // Grappling upwards
        {
            velocityY = Mathf.Sqrt(2 * gravity * trajectoryHeight); // Needed to reach peak height
            timeToReach = (velocityY + Mathf.Sqrt(velocityY * velocityY + 2 * gravity * displacementY)) / gravity;
        }
        else // Grappling downward
        {
            velocityY = Mathf.Sqrt(2*gravity * (trajectoryHeight - transform.position.y));
            Debug.Log("h:"+transform.position.y);
            Debug.Log("TrajectoryHeight:" + trajectoryHeight);
            Debug.Log("Hmax:"+(trajectoryHeight-transform.position.y));
            timeToReach = (velocityY +Mathf.Sqrt(2*trajectoryHeight*gravity))/gravity; // Free-fall time

        }

        velocityXZ = displacementXZ / timeToReach; // Compute horizontal velocity

        Debug.Log("Gravity: " + gravity);
        Debug.Log("Velocity XZ: " + velocityXZ + " Velocity Y: " + velocityY);

        return new Vector3(velocityXZ.x, velocityY, velocityXZ.z);
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
