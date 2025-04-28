using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Knife Tilt")]
    [Tooltip("Drag the Knife GameObject (child of Hand_R) here")]
    [SerializeField] private Transform knifeTransform;
    [Tooltip("Local Euler tilt to apply when sprinting")]
    [SerializeField] private Vector3 sprintTiltEuler = new Vector3(10f, 0f, 0f);
    [Tooltip("How quickly the knife blends into the tilt")]
    [SerializeField] private float tiltSpeed = 8f;

    [Header("Sprint FOV")]
    [Tooltip("Your main camera here")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("FOV when walking / idle")]
    [SerializeField] private float baseFOV = 60f;
    [Tooltip("FOV when sprinting")]
    [SerializeField] private float sprintFOV = 75f;
    [Tooltip("How quickly FOV transitions")]
    [SerializeField] private float fovLerpSpeed = 6f;

    // store the knife’s rest (initial) local rotation
    private Quaternion _knifeRestRot;
    private float _currentTargetFOV;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (knifeTransform != null)
            _knifeRestRot = knifeTransform.localRotation;
        
        if (playerCamera != null)
        {
            // override baseFOV if you left it at 0
            baseFOV = playerCamera.fieldOfView;
            _currentTargetFOV = baseFOV;
        }
    }

    void Update()
    {
        // Walking
        if (playerMovement.verticalMovement == 1)      animator.SetBool("isWalking", true);
        else                                           animator.SetBool("isWalking", false);

        // Back Walk
        if (playerMovement.verticalMovement == -1)     animator.SetBool("isBackWalking", true);
        else                                           animator.SetBool("isBackWalking", false);

        // Right Walk
        if (playerMovement.horizontalMovement == 1)    animator.SetBool("isRightWalking", true);
        else                                           animator.SetBool("isRightWalking", false);

        // Left Walk
        if (playerMovement.horizontalMovement == -1)   animator.SetBool("isLeftWalking", true);
        else                                           animator.SetBool("isLeftWalking", false);

        // Sprint
        if (playerMovement.isSprinting)                animator.SetBool("isSprinting", true);
        else                                           animator.SetBool("isSprinting", false);

        // Sliding
        if (playerMovement.isSliding)                  animator.SetBool("isSliding", true);
        else                                           animator.SetBool("isSliding", false);

        // Crouching
        if (playerMovement.isCrouching)                animator.SetBool("isCrouching", true);
        else                                           animator.SetBool("isCrouching", false);

        // Knife tilt logic
        if (knifeTransform != null)
        {
            bool sprinting = playerMovement.isSprinting;
            // Determine target rotation: rest or rest + tilt
            Quaternion targetRot = sprinting
                ? _knifeRestRot * Quaternion.Euler(sprintTiltEuler)
                : _knifeRestRot;

            // Smoothly interpolate towards target
            knifeTransform.localRotation = Quaternion.Slerp(
                knifeTransform.localRotation,
                targetRot,
                Time.deltaTime * tiltSpeed
            );
        }

        // — FOV blend —
        if (playerCamera != null)
        {
            // decide where we’re heading
            _currentTargetFOV = playerMovement.isSprinting 
                ? sprintFOV 
                : baseFOV;

            // smooth lerp
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                _currentTargetFOV,
                Time.deltaTime * fovLerpSpeed
            );
        }
    }
}
