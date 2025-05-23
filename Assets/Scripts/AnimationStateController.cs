using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private TransferThrowable tt;

    [Header("Knife Tilt")]
    [SerializeField] private Transform knifeTransform;
    [SerializeField] private Vector3 sprintTiltEuler = new Vector3(10f, 0f, 0f);
    [SerializeField] private float tiltSpeed = 8f;

    [Header("Sprint FOV")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float fovLerpSpeed = 6f;

    [Header("Slide‑Shrink Knife")]
    [SerializeField] private float scaleLerpSpeed = 8f;
    [SerializeField] private ParticleSystem lightning;   // assign your lightning VFX here
    private AudioSource lightningAudio;

    [Header("Throwing")]
    [Tooltip("Must match your Animator Trigger parameter")]
    [SerializeField] private string windupTrigger = "FP_Windup";
    [SerializeField] private string throwTrigger  = "FP_Throw";

    [Header("Throwing")]
    [Tooltip("Must match your Animator Trigger parameter")]
    [SerializeField] private string windupLeftTrigger = "FP_WindupLeft";
    [SerializeField] private string throwLeftTrigger  = "FP_ThrowLeft";
    

    private Quaternion _knifeRestRot;
    private Vector3    _knifeRestScale;
    private float      _currentTargetFOV;

    // track previous slide state
    private bool _wasSliding;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (knifeTransform != null)
        {
            _knifeRestRot   = knifeTransform.localRotation;
            _knifeRestScale = knifeTransform.localScale;
        }

        if (playerCamera != null)
        {
            baseFOV = playerCamera.fieldOfView;
            _currentTargetFOV = baseFOV;
        }

        _wasSliding = false;
        if (lightning != null){
            lightning.Stop();
        }
        if (lightning != null){
            lightningAudio = lightning.GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // — ANIMATOR STATES —  
        animator.SetBool("hasTransfer",    tt.transferAmount > 0);
        animator.SetBool("isWalking",      playerMovement.verticalMovement ==  1);
        animator.SetBool("isBackWalking",  playerMovement.verticalMovement == -1);
        animator.SetBool("isRightWalking", playerMovement.horizontalMovement ==  1);
        animator.SetBool("isLeftWalking",  playerMovement.horizontalMovement == -1);
        animator.SetBool("isSprinting",    playerMovement.isSprinting);
        animator.SetBool("isCrouching",    playerMovement.isCrouching);

        // — SLIDE STATE & LIGHTNING VFX (only if knife is active) —
    bool hasKnife   = knifeTransform != null && knifeTransform.gameObject.activeSelf;
    bool vfxSliding = playerMovement.isSliding && hasKnife;

    // still drive the raw “isSliding” animator param
    animator.SetBool("isSliding", playerMovement.isSliding);

    // play/stop lightning based on vfxSliding (not raw sliding)
    if (vfxSliding && !_wasSliding && !tt.isPreparingThrow)
    {
        lightning?.Play();
        if (lightningAudio != null){
            lightningAudio.pitch = Random.Range(0.9f, 1.1f);
            lightningAudio.Play();
        }
    }
    else if (!vfxSliding && _wasSliding)
        lightning?.Stop();

    // remember for next frame
    _wasSliding = vfxSliding;

    // — KNIFE TILT WHEN SPRINTING —
    if (knifeTransform != null)
    {
        Quaternion targetRot = playerMovement.isSprinting
            ? _knifeRestRot * Quaternion.Euler(sprintTiltEuler)
            : _knifeRestRot;

        knifeTransform.localRotation = Quaternion.Slerp(
            knifeTransform.localRotation,
            targetRot,
            Time.deltaTime * tiltSpeed
        );
    }

    // — SLIDE‑SHRINK SCALE (unaffected) —
    if (knifeTransform != null)
    {
        Vector3 targetScale = playerMovement.isSliding && !tt.isPreparingThrow
            ? Vector3.zero
            : _knifeRestScale;

        knifeTransform.localScale = Vector3.Lerp(
            knifeTransform.localScale,
            targetScale,
            Time.deltaTime * scaleLerpSpeed
        );
    }

    // — FOV BLEND ON SPRINT —
    if (playerCamera != null)
    {
        _currentTargetFOV = playerMovement.isSprinting
            ? sprintFOV
            : baseFOV;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            _currentTargetFOV,
            Time.deltaTime * fovLerpSpeed
        );
    }
}
    
    public void PlayThrowAnim()
    {
        animator.SetTrigger(throwTrigger);
    }

    public void PlayWindup() => animator.SetTrigger(windupTrigger);
    public void PlayLeftWindup() => animator.SetTrigger(windupLeftTrigger);
    public void PlayThrow() 
    { 
        animator.SetTrigger(throwTrigger);
        animator.ResetTrigger(windupTrigger);
    }  
    public void PlayThrowLeft() 
    { 
        animator.SetTrigger(throwLeftTrigger);
        animator.ResetTrigger(windupLeftTrigger);
    }  
}