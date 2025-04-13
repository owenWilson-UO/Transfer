using UnityEngine;

public class ThrowableDetection : MonoBehaviour
{
    public Rigidbody rb;
    TransferThrowable tt;

    private bool targetHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tt = FindFirstObjectByType<TransferThrowable>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) { return; }
        tt.ResetThrow();
        if (targetHit)
        {
            return;
        } 
        else
        {
            targetHit = true;
        }
        rb.isKinematic = true;
        Destroy(transform.gameObject);
        //transform.SetParent(collision.transform);
    }
}
