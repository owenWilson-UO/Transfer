using System.Collections;
using UnityEngine;

public class IgnitionThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform knife;                 
    [SerializeField] private GameObject objectToThrow;
    [SerializeField] private AnimationStateController rightAnimController;
    [SerializeField] private AnimationStateController leftAnimController;

    [Header("Throw Settings")]
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwUpwardForce = 2f;
    [SerializeField] private float knifeMaxLifetime = 10f;

    [Header("Cooldown")]
    [SerializeField, Tooltip("Seconds between successive throws")]
    private float cooldown = 1f;

    [Header("Player Data")]
    [SerializeField] private PlayerUpgradeData playerUpgradeData;

    [Header("Hand‐Knife Model")]
    [SerializeField, Tooltip("The knife GameObject that shows in the player's hand when ready")]
    private GameObject handKnife;                            // ← this is the in‑hand knife model

    [Header("Spawn Animation")]                             // ← NEW
    [SerializeField, Tooltip("A small fire‐in‐hand VFX prefab")]
    private GameObject fireVFXPrefab;                        // ← NEW
    [SerializeField, Tooltip("Time (seconds) it takes to grow the knife into place")]
    private float spawnDuration = 0.3f;                      // ← NEW

    // === PUBLIC FIELDS exposed to HUDManager ===
    [HideInInspector] public int ignitionAmount;             
    public bool IgnitionLockout { get; private set; } = false;  

    // === Internal State ===
    private Vector3 _knifeRestScale;                         // ← NEW: store the knife’s intended final scale
    private bool isPreparingThrow = false;
    private float _nextAllowedThrowTime = 0f;

    void Start()
    {
        // Cache the knife’s “rest” scale, then hide it and set to zero
        if (handKnife != null)
        {
            _knifeRestScale = handKnife.transform.localScale;   // ← NEW
            handKnife.transform.localScale = Vector3.zero;      // ← NEW
            handKnife.SetActive(false);
        }

        // 1) Default our ammo to the player’s max on Start
        ignitionAmount = playerUpgradeData.maxIgnitionAmount;

        // (We no longer need to show it here, since SpawnHandKnife() will handle showing.)
        // 2) If the in‑hand knife was assigned in Inspector, show it only if ammo > 0
        //    but we want the “fire then grow” so we remove immediate SetActive(true).
        //    handKnife.SetActive(ignitionAmount > 0);

        // 3) If leftAnimController is null, find it in children
        if (leftAnimController == null)
            leftAnimController = GetComponentInChildren<AnimationStateController>();

        // 4) Ditto for rightAnimController (if needed)
        if (rightAnimController == null)
            rightAnimController = GetComponentInChildren<AnimationStateController>();
    }

    void Update()
    {
        // 1) Prevent any input if we’re in lockout
        if (IgnitionLockout)
            return;

        // 2) When player presses the throw key, begin windup only if not already preparing
        if (Input.GetKeyDown(throwKey) 
            && !isPreparingThrow 
            && Time.time >= _nextAllowedThrowTime 
            && ignitionAmount > 0)     // ← only allow windup if we have ammo
        {
            isPreparingThrow = true;
            leftAnimController.PlayLeftWindup();
        }

        // 3) When player releases the key, actually throw
        if (Input.GetKeyUp(throwKey) && isPreparingThrow)
        {
            isPreparingThrow = false;
            leftAnimController.PlayThrowLeft();
            PerformThrow();
            _nextAllowedThrowTime = Time.time + cooldown;
        }
    }

    private void PerformThrow()
    {
        // --- Decrement ammo, hide the hand‐knife if we just emptied last charge ---
        ignitionAmount--;
        if (handKnife != null && ignitionAmount <= 0)
        {
            handKnife.SetActive(false);
        }

        // --- Standard throw logic below ---
        float spawnDistance = 0.2f;
        Vector3 spawnPoint = playerCamera.ViewportToWorldPoint(new Vector3(0.1f, 0.5f, spawnDistance));
        Vector3 dir = playerCamera.transform.forward;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion offset = Quaternion.Euler(0, 90f, 0);
        Quaternion rot = lookRot * offset;

        GameObject proj = Instantiate(objectToThrow, spawnPoint, rot);
        var despawner = proj.AddComponent<DespawnOnCollision>();
        despawner.maxLifetime = knifeMaxLifetime;

        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = dir * throwForce + Vector3.up * throwUpwardForce;
        }
        else
        {
            Debug.LogError("Thrown prefab needs a non‑kinematic Rigidbody!", proj);
        }
    }

    public void SetIgnitionLockout(bool enabled)
    {
        IgnitionLockout = enabled;
        if (enabled)
        {
            ignitionAmount = 0;
            if (handKnife != null)
                handKnife.SetActive(false);
        }
    }

    /// <summary>
    /// Called by HUDManager.IgnitionCooldown(): we now animate a brief fire VFX
    /// then grow the knife back into the player’s hand over spawnDuration.
    /// </summary>
    public void SpawnHandKnife()
    {
        // If we’re already showing the knife (or there’s no knife assigned), do nothing
        if (handKnife == null || handKnife.activeSelf)
            return;

        // 1) Activate the knife at zero scale
        handKnife.SetActive(true);
        handKnife.transform.localScale = Vector3.zero;

        // 2) If there’s a fire prefab, instantiate it as a child of the knife
        GameObject fire = null;
        if (fireVFXPrefab != null)
        {
            fire = Instantiate(
                fireVFXPrefab,
                handKnife.transform.position,
                handKnife.transform.rotation,
                handKnife.transform  // parent to the knife, so it moves with the player’s hand
            );
        }

        // 3) Start a coroutine that scales the knife from 0 → rest over spawnDuration,
        //    and destroy the fire VFX once complete.
        StartCoroutine(FireAndGrowCoroutine(fire));
    }

    private IEnumerator FireAndGrowCoroutine(GameObject fire)
    {
        float elapsed = 0f;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            handKnife.transform.localScale = _knifeRestScale * t;
            yield return null;
        }

        // Ensure exact final scale
        handKnife.transform.localScale = _knifeRestScale;

        // Destroy the fire VFX (or let it auto‐stop if your ParticleSystem does so)
        if (fire != null)
            Destroy(fire);
    }
}

/// <summary>
/// Destroys the clone either on first collision or after maxLifetime seconds.
/// </summary>
public class DespawnOnCollision : MonoBehaviour
{
    [HideInInspector] public float maxLifetime = 10f;

    void Start()
    {
        if (maxLifetime > 0f)
            Destroy(gameObject, maxLifetime);
    }

    private void OnCollisionEnter(Collision _)
    {
        Destroy(gameObject);
    }
}
