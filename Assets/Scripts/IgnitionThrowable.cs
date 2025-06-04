using System.Collections;
using UnityEngine;

public class IgnitionThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform knife;                 // The Transform of the “knife model” in the player's hand
    [SerializeField] private GameObject objectToThrow;
    [SerializeField] private AnimationStateController rightAnimController;
    [SerializeField] private AnimationStateController leftAnimController;

    [Header("Throw Settings")]
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwUpwardForce = 2f;
    [SerializeField] private float knifeMaxLifetime = 10f;   // fallback despawn

    [Header("Cooldown")]
    [SerializeField, Tooltip("Time in seconds between successive throws")]
    private float cooldown = 1f;                             // This is only used for preventing super‐rapid throws

    [Header("Player Data")]
    [SerializeField] private PlayerUpgradeData playerUpgradeData; // To read maxIgnitionAmount

    [Header("Hand‐Knife Model")]
    [SerializeField, Tooltip("The knife GameObject that shows in the player's hand when ready")]
    private GameObject handKnife;                            // ← new: this holds the in‑hand knife model

    // === PUBLIC FIELDS exposed to HUDManager ===
    [HideInInspector] public int ignitionAmount;             // ← how many charges we currently have
    public bool IgnitionLockout { get; private set; } = false;  // ← whether we’re in lockout

    // === Internal State ===
    private bool isPreparingThrow = false;
    private float _nextAllowedThrowTime = 0f;

    void Start()
    {
        // 1) Default our ammo to the player’s max on Start
        ignitionAmount = playerUpgradeData.maxIgnitionAmount;

        // 2) If the in‑hand knife was assigned in Inspector, show it only if ammo > 0
        if (handKnife != null)
        {
            handKnife.SetActive(ignitionAmount > 0);
        }

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
        if (handKnife != null)
        {
            handKnife.SetActive(ignitionAmount > 0);
        }

        // --- Standard throw logic below (very similar to your original) ---

        float spawnDistance = 0.2f;
        Vector3 spawnPoint = playerCamera.ViewportToWorldPoint(new Vector3(0.1f, 0.5f, spawnDistance));
        // 1) direction straight ahead
        Vector3 dir = playerCamera.transform.forward;

        // 2) orient so Z+ is the blade
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion offset = Quaternion.Euler(0, 90f, 0);  // adjust per your model
        Quaternion rot = lookRot * offset;

        // 3) spawn the clone
        GameObject proj = Instantiate(objectToThrow, spawnPoint, rot);

        // 4) add the self‑destruct behaviour
        var despawner = proj.AddComponent<DespawnOnCollision>();
        despawner.maxLifetime = knifeMaxLifetime;

        // 5) launch via Rigidbody
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

    /// <summary>
    /// If something sets IgnitionLockout = true (e.g. the player died or picked up
    /// an upgrade that disables ignition), we immediately zero out ammo and hide UI.
    /// </summary>
    public void SetIgnitionLockout(bool enabled)
    {
        IgnitionLockout = enabled;

        if (enabled)
        {
            // Wipe ammo count and hide the in‑hand knife
            ignitionAmount = 0;
            if (handKnife != null)
                handKnife.SetActive(false);
        }
    }

    /// <summary>
    /// Called by HUDManager.IgnitionCooldown() when one charge has “recovered.”
    /// This re‑enables the hand‑knife model in preparation for the next throw.
    /// </summary>
    public void SpawnHandKnife()
    {
        // If we have at least 1 charge and the knife is currently hidden → show & animate
        if (handKnife == null || handKnife.activeSelf)
            return;

        // Activate it
        handKnife.SetActive(true);

        // Optionally: you could tween scale/fade or play a VFX here (analogous to Transfer's PrintCoroutine).
        // For now, we’ll just set it active, since its “rest scale” is defined in the prefab.
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
