using UnityEngine;

public class PsylinkDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }
    PsylinkThrowable pt;

    public bool targetHit {  get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pt = FindFirstObjectByType<PsylinkThrowable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) { return; }
        pt.readyToThrow = true;
        if (targetHit)
        {
            return;
        } 
        else
        {
            targetHit = true;
        }
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        //transform.SetParent(collision.transform);
    }
}
