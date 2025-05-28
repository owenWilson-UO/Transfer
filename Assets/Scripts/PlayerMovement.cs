using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] PlayerUpgradeData upgradeData;
    [SerializeField] UpgradeManagerUI upgradeManagerUI;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float moveMultiplier = 5f;
    public float airMultiplier = 0.4f;

    [Header("Sprinting")]
    [SerializeField] float crouchSpeed = 2f;
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float wallRunSpeed = 8f;
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
    [SerializeField] public InputActionReference jumpButton;
    [SerializeField] public InputActionReference sprintButton;
    [SerializeField] public InputActionReference crouchButton;
    [SerializeField] public InputActionReference slowMotionButton;

    [Header("Drag")]
    [SerializeField] float groundDrag = 6f;
    [SerializeField] float airDrag = 2f;

    //multiple properties here have private sets but public gets. This is because I need a reference to those properties in 
    //other scripts but I don't want those other scripts to able to modify those properties

    public float horizontalMovement {  get; private set; }
    public float verticalMovement { get; private set; }
    public float gravityMultiplier = 0f;

    [Header("Ground Detection")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] Transform groundChekPosition;
    public bool isGrounded { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isSprinting { get; private set; }
    public bool crouchButtonPressedIn { get; private set; }
    public bool isSliding { get; private set; }
    public float groundDistance = 0.01f;

    [Header("Slow Motion")]
    [SerializeField] float targetTimeScale = 0.2f;
    [SerializeField] float easeDuration = 0.25f;
    [SerializeField] Material slowMotionMaterial;
    [SerializeField] float vignettePowerStart;
    [SerializeField] float vignettePowerDuringSloMotion;
    [SerializeField] Image slowMoFillImage;
    [SerializeField] Image slowMoHighlightImage;
    public bool isInSlowMotion { get; private set; }
    public bool slowMotionCoolingDown { get; private set; }
    private float slowMotionDurationUsed;
    private Coroutine slowMoCoroutine;
    private Coroutine slowMoTimerCoroutine;
    private Coroutine slowMoCoolDownCoroutine;

    [Header("Fade In")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration;

    [Header("Audio")]
    [SerializeField] private AudioSource walkingAudioSource;
    [SerializeField] private AudioClip walkingClip;
    [SerializeField] private float footstepInterval = 0.5f; // time between footsteps
    private float footstepTimer;
    [SerializeField] private AudioSource slidingAudioSource; // For the slide sfx
    [SerializeField] private AudioClip slidingClip;


    Vector3 moveDir;
    Vector3 slopeMoveDir;

    Rigidbody rb;
    WallRun wr;
    CapsuleCollider capsule;

    RaycastHit slopeHit;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;


    private bool OnSlope()
    {
        //This function uses a raycast to determine if the
        //object the player is standing on is perpendicular to the global vertical vector.
        //This checks if the object is not horizontal which means it is a slope.
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, 1.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    private void OnEnable()
    {
        jumpButton.action.Enable();
        sprintButton.action.Enable();

        crouchButton.action.started += (ctx) => crouchButtonPressedIn = true; 
        crouchButton.action.canceled += (ctx) => crouchButtonPressedIn = false; 
        crouchButton.action.Enable();

        slowMotionButton.action.Enable();
    }

    private void OnDisable()
    {
        jumpButton.action.Disable();
        sprintButton.action.Disable();

        crouchButton.action.started -= (ctx) => crouchButtonPressedIn = true;
        crouchButton.action.canceled -= (ctx) => crouchButtonPressedIn = false;
        crouchButton.action.Disable();

        slowMotionButton.action.Disable();
    }

    private void Start()
    {
        footstepTimer = 0f;

        if (walkingAudioSource != null)
        {
            walkingAudioSource.clip = walkingClip;
            walkingAudioSource.loop = false;
            walkingAudioSource.playOnAwake = false;
        }

        rb = GetComponent<Rigidbody>();
        wr = GetComponent<WallRun>();
        capsule = rb.GetComponentInChildren<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.useGravity = true;
        isCrouching = false;

        currentHeight = standingheight;
        slideTimer = 0f;

        slowMotionDurationUsed = 0f;

        StartCoroutine(FadeFromBlack());
    }

    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundChekPosition.position, groundDistance, groundMask);
        //checksphere is similar to raycast but it creates a sphere centered at a given position with a given radius.
        //This returns true if any colliders overlap the sphere of a specific layer (ground in this case).

        MoveInput();
        ControlDrag();
        ControlSpeed();

        //Logic for slowing down time. Since we use unity's physics for everything,
        //we can simply change the global Time.timeScale to slow down time.
        //We do this here using a Coroutine to avoid a snappy change in and out of slow motion
        if (upgradeData.maxSlowMotionDuration > 0f && slowMotionButton.action.triggered && (!slowMotionCoolingDown || isInSlowMotion) && !upgradeManagerUI.isOpen)
        {
            SlowMotion();
        }

        if (jumpButton.action.triggered && isGrounded && !upgradeManagerUI.isOpen)
        {
            if (isSliding)
            {
                Jump();
                isCrouching = false;
                isSliding = false;
            }
            else if (isCrouching)
            {
                isCrouching = false;
                isSliding = false;
            }
            else
            {
                Jump();
            }
        }
        if (crouchButton.action.triggered && isGrounded && !isSprinting && !upgradeManagerUI.isOpen)
        {
            isCrouching = !isCrouching;
            isSliding = false;
        }
        else if (crouchButtonPressedIn && isGrounded && isSprinting && slideTimer == 0f && !upgradeManagerUI.isOpen)
        {
            //Sliding logic that locks the player into sliding that direction until the slide is up
            //and there is a 0.5s cooldown until you can slide again

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

            if (slidingAudioSource != null)
            {
                slidingAudioSource.Stop(); // Ensure clean restart
                slidingAudioSource.pitch = Random.Range(0.9f, 1.1f);
                slidingAudioSource.Play();
            }

        }

        if (slideTimer > 0) // timer to control how long the player is sliding and the cooldown
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                slideTimer = 0f;
            }
            else if (slideTimer-0.5f <= 0f)
            {
                isSliding = false;
                if (slidingAudioSource.isPlaying)
                {
                    slidingAudioSource.Stop();
                }
            }

        }

        ControlCrouch();
        
        slopeMoveDir = Vector3.ProjectOnPlane(moveDir, slopeHit.normal); 
        //using the RaycastHit from the OnSlope method, we can move on a slope (i.e. stairs smoothly with no wierd physics)

        HandleFootstepAudio();

    }

    
    //Controls basic player movement
    void MoveInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        if (!rb.useGravity && verticalMovement > 0)
        {
            Vector3 wallForward = Vector3.Cross(wr.wallNormal, Vector3.up).normalized;

            //flip to match player’s facing
            if (Vector3.Dot(wallForward, orientation.forward) < 0)
                wallForward = -wallForward;

            moveDir = wallForward;
        }
        else if (!isSliding)
        {
            moveDir = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3 (rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    public void SlowMotion()
    {
        if (!isInSlowMotion)
        {
            if (slowMoCoroutine != null) StopCoroutine(slowMoCoroutine);
            if (slowMoTimerCoroutine != null) StopCoroutine(slowMoTimerCoroutine);

            slowMoCoroutine = StartCoroutine(SmoothTimeScale(targetTimeScale, vignettePowerStart, vignettePowerDuringSloMotion));
            slowMoTimerCoroutine = StartCoroutine(SlowMoTimer());
            //This will automatically turn off the slow motion ability if the slowMotionButton is not pressed
            //before the players current maxDuration time has passsed.
            isInSlowMotion = true;
        }
        else
        {
            slowMotionCoolingDown = true;
            if (slowMoCoroutine != null) StopCoroutine(slowMoCoroutine);
            if (slowMoTimerCoroutine != null) StopCoroutine(slowMoTimerCoroutine);

            slowMoCoroutine = StartCoroutine(SmoothTimeScale(1f, vignettePowerDuringSloMotion, vignettePowerStart));
            isInSlowMotion = false;
        }
    }

    void ControlSpeed()
    {
        //Changes the movement speed of the player based on the player's state
        if (sprintButton.action.IsPressed() && (isGrounded || !rb.useGravity) && verticalMovement > 0f)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, rb.useGravity ? sprintSpeed : wallRunSpeed, acceleration * Time.deltaTime);
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
            moveSpeed = Mathf.Lerp(moveSpeed, rb.useGravity ? walkSpeed : wallRunSpeed, acceleration * Time.deltaTime);
            isSprinting = !rb.useGravity;
        }
    }

    // drag is what one can think of as an ice feeling.
    // We dont want the player to abruptly stop when the player stops pressing a direction, that doesn't feel right.
    // We also don't want the player to have the same amount of drag when in the air either. The last case is when sliding,
    // we want the player to slide further than they normaly would especially since when we slide, it's a simple impulse force that is added to the player,
    // so if we want that to carry we need less drag.
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
    }

    private void FixedUpdate()
    {
        MovePlayer();

        //move player with psylink platform if moving
        if (currentPlatform != null)
        {
            Vector3 platformMovement = currentPlatform.position - lastPlatformPosition;
            transform.position += platformMovement;
            lastPlatformPosition = currentPlatform.position;
        }
    }

    void MovePlayer()
    {
        //Physics for moving the player
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDir.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
            gravityMultiplier = 0f;
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeMoveDir.normalized * moveSpeed * moveMultiplier, ForceMode.Acceleration);
            gravityMultiplier = 0f;
        }
        else
        {
            rb.AddForce(moveDir.normalized * moveSpeed * moveMultiplier * airMultiplier, ForceMode.Acceleration);
            if (rb.useGravity)
            {
                rb.linearVelocity += Physics.gravity * Time.fixedDeltaTime + Physics.gravity * gravityMultiplier;
                gravityMultiplier += (float) 0.025 * Time.fixedDeltaTime;
                //with the build in gravity, its a constant 9.81f of downward force so im using this multiplier to increase
                //the downward force the longer the player is in the air, adjusting it back to 0 when needed like when wall running
                //or after transfering
            }
            else
            {
                gravityMultiplier = 0f;
            }
        }
    }

    IEnumerator SmoothTimeScale(float target, float startShaderValue, float targetShaderValue)
    {
        float start = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < easeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / easeDuration;

            Time.timeScale = Mathf.Lerp(start, target, t);
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            float shaderValue = Mathf.Lerp(startShaderValue, targetShaderValue, t);
            slowMotionMaterial.SetFloat("_VignettePower", shaderValue);

            yield return null;
        }

        Time.timeScale = target;
        Time.fixedDeltaTime = 0.02f * target;
        slowMotionMaterial.SetFloat("_VignettePower", targetShaderValue);
        if (target == 1f) // Going out of slow motion
        {
            //start slow motion cooldown timer
            if (slowMoCoolDownCoroutine != null) StopCoroutine(slowMoCoolDownCoroutine);
            slowMoCoolDownCoroutine = StartCoroutine(SlowMoCoolDown(Mathf.Clamp01(slowMotionDurationUsed/upgradeData.maxSlowMotionDuration) * 5f));
        }
    }

    IEnumerator SlowMoTimer()
    {
        float duration = upgradeData.maxSlowMotionDuration;
        slowMotionDurationUsed = 0f;

        //show progresss left on slow motion ability
        while (slowMotionDurationUsed < duration)
        {
            slowMotionDurationUsed += Time.unscaledDeltaTime;
            slowMoFillImage.fillAmount = 1f - Mathf.Clamp01(slowMotionDurationUsed / duration);
            slowMoHighlightImage.fillAmount = 1f - Mathf.Clamp01(slowMotionDurationUsed / duration);

            yield return null;
        }

        slowMotionCoolingDown = true;
        if (slowMoCoroutine != null) StopCoroutine(slowMoCoroutine);
        slowMoCoroutine = StartCoroutine(SmoothTimeScale(1f, vignettePowerDuringSloMotion, vignettePowerStart));
        isInSlowMotion = false;
        slowMoFillImage.fillAmount = 0f;
        slowMoHighlightImage.fillAmount = 0f;
    }

    IEnumerator SlowMoCoolDown(float elapsedCalc)
    {
        float duration = 5f; //5 second cooldown (maybe add upgrade later to decrease cooldown)
        float elapsed = duration - elapsedCalc; //this calculates the starting point at which the users cooldown should begin from
        //depending on if they exited slow motion before the maxSlowMotionDuration passed

        //visual for slow motion cooldown using in game time
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            slowMoFillImage.fillAmount = 0f + Mathf.Clamp01(elapsed / duration);
            slowMoHighlightImage.fillAmount = 0f + Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        slowMoFillImage.fillAmount = 1f;
        slowMoHighlightImage.fillAmount = 1f;
        slowMotionCoolingDown = false;

        slowMotionDurationUsed = 0f;
    }

    public IEnumerator FadeFromBlack()
    {
        Color color = fadeImage.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }

    private void OnCollisionStay(Collision collision)
    {
        if ((isGrounded || !rb.useGravity) && collision.gameObject.CompareTag("PsylinkInteractable"))
        {
            currentPlatform = collision.transform;
            lastPlatformPosition = currentPlatform.position;
            Debug.Log($"stuck  ground {isGrounded} wall run {!rb.useGravity}");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("PsylinkInteractable") && collision.transform == currentPlatform)
        {
            currentPlatform = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Physics.CheckSphere(groundChekPosition.position, groundDistance, groundMask)
            ? Color.green
            : Color.red;

        Gizmos.DrawWireSphere(groundChekPosition.position, groundDistance);
    } 

    private void HandleFootstepAudio()
    {
        bool isWalking = (Mathf.Abs(horizontalMovement) > 0 || Mathf.Abs(verticalMovement) > 0);
        bool isWallRunning = !rb.useGravity && isWalking;
        bool canPlayFootstep = (isGrounded || isWallRunning) && isWalking && !isSliding && !isCrouching;

        if (canPlayFootstep)
        {
            footstepTimer += Time.deltaTime;

            float interval = isSprinting ? footstepInterval * 0.6f : footstepInterval; // Faster when sprinting

            if (footstepTimer >= interval)
            {
                footstepTimer = 0f;

                if (walkingAudioSource != null)
                {
                    walkingAudioSource.pitch = Random.Range(0.95f, 1.05f);
                    walkingAudioSource.Play();
                }
            }
        }
        else
        {
            footstepTimer = footstepInterval; // reset so first step after pause is quick
        }
    }
}
