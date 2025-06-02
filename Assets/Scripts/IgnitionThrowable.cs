using UnityEngine;

public class IgnitionThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera   playerCamera;
    [SerializeField] private Transform knife;
    [SerializeField] private GameObject objectToThrow;
    [SerializeField] private AnimationStateController rightAnimController;
    [SerializeField] private AnimationStateController leftAnimController;

    [Header("Throw Settings")]
    [SerializeField] private KeyCode  throwKey         = KeyCode.Mouse0;
    [SerializeField] private float    throwForce       = 15f;
    [SerializeField] private float    throwUpwardForce =  2f;
    [SerializeField] private float    knifeMaxLifetime = 10f;  // fallback despawn

    [Header("Cooldown")]
    [SerializeField, Tooltip("Time in seconds between successive throws")] 
    private float cooldown = 1f;

    private bool isPreparingThrow = false;
    private float _nextAllowedThrowTime = 0f;

    void Start()
    {
        if (leftAnimController == null)
            leftAnimController = GetComponentInChildren<AnimationStateController>();
    }

    void Update()
    {
        // Only allow starting a new wind‑up if cooldown has expired
        if (Input.GetKeyDown(throwKey) 
            && !isPreparingThrow 
            && Time.time >= _nextAllowedThrowTime)
        {
            isPreparingThrow = true;
            leftAnimController.PlayLeftWindup();
        }

        // When player releases the key and we were in wind‑up...
        if (Input.GetKeyUp(throwKey) && isPreparingThrow)
        {
            isPreparingThrow = false;
            leftAnimController.PlayThrowLeft();
            Throw();

            // Start cooldown: next allowed throw is current time + cooldown
            _nextAllowedThrowTime = Time.time + cooldown;
        }
    }

    private void Throw()
    {
        // 1) direction straight ahead
        Vector3 dir = playerCamera.transform.forward;

        // 2) orient so Z+ is the blade
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        Quaternion offset  = Quaternion.Euler(0, 90f, 0);  // adjust per your model
        Quaternion rot     = lookRot * offset;

        // 3) spawn the clone
        GameObject proj = Instantiate(objectToThrow, knife.position, rot);

        // 4) add the self‑destruct behaviour
        var despawner = proj.AddComponent<DespawnOnCollision>();
        despawner.maxLifetime = knifeMaxLifetime;

        // 5) launch via Rigidbody
        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity  = true;
            rb.linearVelocity = dir * throwForce 
                              + Vector3.up * throwUpwardForce;
        }
        else
        {
            Debug.LogError("Thrown prefab needs a non‑kinematic Rigidbody!", proj);
        }
    }
}

/// <summary>
/// When attached at runtime to the clone, this destroys it on first collision
/// (and after maxLifetime seconds if no collision occurs).
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
