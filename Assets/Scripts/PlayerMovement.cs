using System.Text.RegularExpressions;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float moveMultiplier = 5f;
    public float airMultiplier = 0.4f;

    [Header("Sprinting")]
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float acceleration = 12f;

    [Header("Crouching")]
    [SerializeField] float crouchTransitionSpeed = 3f;
    [SerializeField] float standingheight = 2f;
    [SerializeField] float crouchHeight = 1f;
    float currentHeight;

    [Header("Sliding")]
    [SerializeField] private float slideForce = 8f;  
    [SerializeField] private float slideTime = 0.75f;
    [SerializeField] private float slideDrag = 1f;
    private float slideTimer;

    [Header("Jumping")]
    public float jumpForce = 12f;

    [Header("Keybinds")]
    [SerializeField] public KeyCode jumpKey = KeyCode.Space;
    [SerializeField] public KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] public KeyCode crouchKey = KeyCode.C;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;



    float horizontalMovement;
    float verticalMovement;

    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundChekPosition;
    public bool isGrounded { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isSprinting { get; private set; }
    public bool isSliding { get; private set; }

    public float groundDistance = 0.01f;

    Vector3 moveDir;
    Vector3 slopeMoveDir;

    Rigidbody rb;
    CapsuleCollider capsule;

    RaycastHit slopeHit;


    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 3 / 2 + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = rb.GetComponentInChildren<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;
        isCrouching = false;

        currentHeight = standingheight;
        slideTimer = 0f;
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundChekPosition.position, groundDistance, groundMask);

        MoveInput();
        ControlDrag();
        ControlSpeed();

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            if (isCrouching)
            {
                isCrouching = false;
                isSliding = false;
            }
            else
            {
                Jump();
            }
        }
        if (Input.GetKeyDown(crouchKey) && isGrounded && !isSprinting)
        {
            isCrouching = !isCrouching;
            isSliding = false;
        }
        else if (Input.GetKey(crouchKey) && isGrounded && isSprinting && slideTimer == 0f)
        {
            isSliding = true;
            isCrouching = true;

            slideTimer = slideTime + 0.5f; // + cooldown
            Vector3 slideDir = orientation.forward * verticalMovement;
            if (slideDir.magnitude < 0.1f)
            {
                slideDir = rb.linearVelocity.normalized;
            }
            rb.AddForce(slideDir.normalized * slideForce, ForceMode.Impulse);
            rb.AddForce(Vector3.down * 1f, ForceMode.Impulse);

        }

        if (slideTimer > 0)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                slideTimer = 0f;
            }
            else if (slideTimer-0.5f <= 0f)
            {
                isSliding = false;
            }

        }

        ControlCrouch();
        
        slopeMoveDir = Vector3.ProjectOnPlane(moveDir, slopeHit.normal);
    }

    void MoveInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        if (!isSliding)
        {
            moveDir = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3 (rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ControlSpeed()
    {
        if (Input.GetKey(sprintKey) && (isGrounded || !rb.useGravity) && verticalMovement == 1)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, acceleration * Time.deltaTime);
            isSprinting = true;
            if (!isSliding)
            {
                isCrouching = false;
            }
        }
        else if (isCrouching)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, crouchSpeed, acceleration * Time.deltaTime);
            isSprinting = false;
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, walkSpeed, acceleration * Time.deltaTime);
            isSprinting = false;
        }
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            if (isSliding)
            {
                rb.linearDamping = slideDrag;
            }
            else
            {
                rb.linearDamping = groundDrag;
            }
        }
        else
        {
            rb.linearDamping = airDrag;
        }
    }

    void ControlCrouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standingheight;

        currentHeight = Mathf.Lerp(
            currentHeight,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );
        capsule.height = currentHeight;

        //if (isCrouching && !OnSlope())
        //{
        //    rb.AddForce(Vector3.down * 0.5f, ForceMode.Impulse);
        //}
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDir.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDir.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDir.normalized * moveSpeed * moveMultiplier * airMultiplier, ForceMode.Acceleration);
            if (rb.useGravity)
            {
                rb.AddForce(Vector3.down * 9.81f, ForceMode.Force);
            }
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = true;
    //    }
    //}
    //private void OnCollisionExit(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Ground"))
    //    {
    //        isGrounded = false;
    //    }
    //}
}
