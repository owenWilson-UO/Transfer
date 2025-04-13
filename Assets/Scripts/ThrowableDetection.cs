using UnityEngine;

public class ThrowableDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }
    TransferThrowable tt;

    public bool targetHit {  get; private set; }

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
        //transform.SetParent(collision.transform);
    }
}
