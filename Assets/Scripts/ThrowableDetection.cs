using UnityEngine;

public class ThrowableDetection : MonoBehaviour
{
    public Rigidbody rb { get; private set; }
    public bool targetHit { get; private set; }

    [SerializeField] private float spinSpeed = 1080f;
    [Tooltip("Local axis to spin around (e.g. right=X, up=Y, forward=Z)")]
    [SerializeField] private Vector3 spinAxis = Vector3.up;

    [SerializeField, Tooltip("Axis to pre-rotate around before spin")]
    private Vector3 initialRotationAxis = Vector3.forward;

    private bool isSpinning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // ONE-TIME 90° offset around your chosen axis (in local space):
        transform.Rotate(initialRotationAxis.normalized * 90f, Space.Self);
        
        isSpinning = true;
    }

    void Update()
    {
        if (isSpinning)
        {
            // rotate around the blade’s length axis in local space
            
            transform.Rotate(spinAxis * spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            return;

        if (targetHit) 
            return;

        targetHit = true;
        isSpinning = false;
        rb.isKinematic = true;
    }
}
