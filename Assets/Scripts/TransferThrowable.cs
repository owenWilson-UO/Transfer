using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using LensDistortion = UnityEngine.Rendering.Universal.LensDistortion;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

public class TransferThrowable : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UpgradeManagerUI upgradeManagerUI;
    public ParticleSystem teleport;                   // your VFX
    public ParticleSystem lightning;

    [Tooltip("The knife model in the player's hand")]
    [SerializeField] private GameObject handKnife;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse1;
    [SerializeField] InputActionReference throwButton;
    private bool holding;
    public float throwForce;
    public float throwUpwardForce;

    [Header("Player")]
    [SerializeField] private PlayerUpgradeData upgradeData;
    public int transferAmount { get; set; }

    [Header("Print-In Animation")]
    [Tooltip("How long the print-in takes")]
    [SerializeField] private float spawnDuration = 0.3f;

    [Header("Animation")]
    [SerializeField] private AnimationStateController rightAnimController;
    [SerializeField] private AnimationStateController transferAnimController;
    [SerializeField] private AnimationStateController leftAnimController;

    [SerializeField] private Animator transferAnimator;

    [Header("Teleport Lens Warp")]
    [SerializeField] private Volume volume;
    private LensDistortion warp;
    [SerializeField] private float warpDuration;

    [Header("Crosshair")]
    [SerializeField] private RawImage outerCrossHair;
    [SerializeField] private RawImage transferCrosshair;

    private bool readyToThrow;
    public bool isPreparingThrow { get; private set; }
    private Rigidbody rb;
    private PlayerMovement playerMovement;
    
    // Instant Transmission sfx
    private AudioSource teleportAudio;
    // Knife throw sfx
    [SerializeField] private AudioSource throwAudioSource;
    [SerializeField] private AudioClip throwClip;


    // cache the knife’s “ready” scale
    private Vector3 _knifeRestScale;
    private Coroutine _printCoroutine;
    private Coroutine _warpCoroutine;

    private bool manualTeleport;

    public bool TransferLockout { get; private set; } = false;
    private void OnEnable()
    {
        throwButton.action.started += OnThrowStarted;
        throwButton.action.canceled += OnThrowEnded;
        throwButton.action.Enable();
    }

    private void OnDisable()
    {
        throwButton.action.started -= OnThrowStarted;
        throwButton.action.canceled -= OnThrowEnded;
        throwButton.action.Disable();
    }

    private void OnThrowStarted(InputAction.CallbackContext ctx)
    {

        if (manualTeleport)
        {
            manualTeleport = false;
            return;
        }

        var td = FindFirstObjectByType<ThrowableDetection>();
        if (!readyToThrow && td != null)
        {
            manualTeleport = true;
            TeleportToTransfer(td);
            return;
        }
        
        if (TransferLockout) { return; }

        if (readyToThrow && transferAmount > 0)
        {
            isPreparingThrow = true;
            rightAnimController.PlayWindup();
            transferAnimController.PlayWindup();
            leftAnimController.PlayWindup();
        }
    }

    private void OnThrowEnded(InputAction.CallbackContext ctx)
    {
        if (TransferLockout) { return; }

        if (manualTeleport)
        {
            manualTeleport = false;
            return;
        }
        if (isPreparingThrow)
        {
            isPreparingThrow = false;
            rightAnimController.PlayThrow();
            transferAnimController.PlayWindup();
            leftAnimController.PlayThrow();
            Throw();
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        readyToThrow = true;
        isPreparingThrow = false;
        transferAmount = upgradeData.maxTransferAmount;
        if (teleport != null)
        {
            teleportAudio = teleport.GetComponent<AudioSource>();
        }
        if (handKnife != null)
        {
            _knifeRestScale = handKnife.transform.localScale;
            handKnife.SetActive(transferAmount > 0);
        }

        if (rightAnimController == null)
            rightAnimController = GetComponent<AnimationStateController>();
        if (leftAnimController == null)
            leftAnimController = GetComponent<AnimationStateController>();

        if (volume != null && volume.profile.TryGet(out warp))
        {
            warp.intensity.overrideState = true;
            warp.center.overrideState = true;
            warp.scale.overrideState = true;
        }
    }

    void Update()
    {
        if (upgradeManagerUI.isOpen)
            return;

        ChangeCrosshair();
        
        var td = FindFirstObjectByType<ThrowableDetection>();

        // 3) After you’ve thrown (readyToThrow==false), a click can teleport if a knife exists
        //    (or if td.targetHit is true—you keep your existing logic)
        //if (!readyToThrow && throwButton.action.triggered && td != null)
        //{
        //    TeleportToTransfer(td);
        //}

        // 4) Also handle the “auto‐teleport on hit” you already had:
        if (td != null && td.targetHit)
        {
            TeleportToTransfer(td, true);
        }
    }

    private void Throw()
    {
        readyToThrow = false;
        handKnife?.SetActive(false);

        // 1) Build a ray from the CENTER of the screen
        Camera camComp = cam.GetComponent<Camera>();
        Ray centerRay = camComp.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 2) Figure out direction
        Vector3 forceDir = cam.forward;              // fallback
        if (Physics.Raycast(centerRay, out RaycastHit hit, 500f))
        {
            forceDir = (hit.point - centerRay.origin).normalized;
        }

        // 3) Choose spawn point = the ray origin + slight forward offset
        float spawnOffset = 0.5f;  // half a meter in front of camera
        Vector3 spawnPos = centerRay.origin + forceDir * spawnOffset;

        // 4) Compute rotation
        Quaternion rot = Quaternion.LookRotation(forceDir, Vector3.up);

        // 5) Instantiate and launch
        var proj = Instantiate(objectToThrow, spawnPos, rot);
        var projRb = proj.GetComponent<Rigidbody>();
        projRb.AddForce(forceDir * throwForce + transform.up * throwUpwardForce,
                        ForceMode.Impulse);
        if (throwAudioSource != null && throwClip != null){
            throwAudioSource.pitch = Random.Range(0.95f, 1.05f);
            throwAudioSource.PlayOneShot(throwClip);
        }
        // —— Trail setup ——
        var trail = proj.GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.Clear();     // wipe any old segments
            trail.emitting = true;  // start drawing
        }                
    }

    private void ChangeCrosshair()
    {
        if (isPreparingThrow)
        {
            outerCrossHair.color = new(outerCrossHair.color.r, outerCrossHair.color.g, outerCrossHair.color.b, 0f);

            transferCrosshair.color = new(transferCrosshair.color.r, transferCrosshair.color.g, transferCrosshair.color.b, 1f);
        }
        else
        {
            outerCrossHair.color = new(outerCrossHair.color.r, outerCrossHair.color.g, outerCrossHair.color.b, 1f);

            transferCrosshair.color = new(transferCrosshair.color.r, transferCrosshair.color.g, transferCrosshair.color.b, 0f);
        }
    }

    private void TeleportToTransfer(ThrowableDetection td, bool keepPlayerMomentum = false)
    {
        if (td == null)
        {
            return;
        }

        Vector3 playerLinearVelocity = rb.linearVelocity;
        Vector3 playerAngularVelocity = rb.angularVelocity;

        rb.isKinematic = true;
        
        // reposition
        if (keepPlayerMomentum)
        {
            rb.position = td.contactPoint.point + td.contactPoint.normal * 0.5f; //vertical offset to not have the player camera see out of bound when colliding with the roof
        }
        else
        {
            rb.position = td.transform.position;
        }

        // play VFX
        teleport.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        teleport.Play();
        
        // SFX for the teleport VFX
        if (teleportAudio != null)
        {
            teleportAudio.Play();
        }

        if (_warpCoroutine != null) { StopCoroutine(_warpCoroutine); }
        if (warp)
        {
            _warpCoroutine = StartCoroutine(Warp());
        }

        // restore physics & momentum
        playerMovement.gravityMultiplier = 0f;
        rb.isKinematic = false;
        rb.linearVelocity = keepPlayerMomentum ? playerLinearVelocity : td.rb.linearVelocity * 1.25f;
        rb.angularVelocity = keepPlayerMomentum ? playerAngularVelocity : td.rb.angularVelocity * 1.25f;

        Destroy(td.gameObject);
        ResetThrow();
    }

    private void ResetThrow()
    {
        transferAmount--;
        readyToThrow = true;

        if (transferAmount > 0)
            SpawnHandKnife();
    }

    public void SpawnHandKnife()
    {
        if (handKnife == null || handKnife.activeSelf)
            return;

        handKnife.SetActive(true);

        if (_printCoroutine != null)
            StopCoroutine(_printCoroutine);

        var t = handKnife.transform;
        t.localScale = new Vector3(_knifeRestScale.x, 0f, _knifeRestScale.z);
        _printCoroutine = StartCoroutine(PrintCoroutine());
    }

    private IEnumerator PrintCoroutine()
    {
        transferAnimator.ResetTrigger("Inspect");
        transferAnimator.ResetTrigger("FP_Windup");

        float elapsed = 0f;
        Transform t = handKnife.transform;
        lightning.Play();

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            t.localScale = new Vector3(
                _knifeRestScale.x,
                Mathf.Lerp(0f, _knifeRestScale.y, p),
                _knifeRestScale.z
            );
            transferAnimator.ResetTrigger("Inspect");
            transferAnimator.ResetTrigger("FP_Windup");
            yield return null;
        }

        // ensure exact final scale
        t.localScale = _knifeRestScale;
        lightning.Stop();
        transferAnimator.ResetTrigger("Inspect");
        transferAnimator.ResetTrigger("FP_Windup");
        _printCoroutine = null;
    }

    private IEnumerator Warp()
    {
        float elapsed = 0f;
        float warpIntensity = -1f;

        while (elapsed < warpDuration)
        {
            warp.intensity.value = Mathf.SmoothStep(warpIntensity, 0f, elapsed / warpDuration);
            elapsed += Time.deltaTime;

            yield return null;
        }

        warp.intensity.value = 0f;
        _warpCoroutine = null;
    }

    public void SetTransfferLockout(bool b)
    {
        if (b)
        {
            if (isPreparingThrow)
            {
                rightAnimController.PlayThrowAnim();
                leftAnimController.PlayThrowAnim();
            }
            isPreparingThrow = false;

            handKnife.SetActive(false);
        }
        TransferLockout = b;
    }
}
