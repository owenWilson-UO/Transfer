using UnityEngine;

public class ThrowableDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }

    public bool targetHit {  get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //logic for stopping the transfer when it collides with an object
        if (collision.gameObject.CompareTag("Player")) { return; }
        if (targetHit)
        {
            return;
        } 
        else
        {
            targetHit = true;
        }
        rb.isKinematic = true;
    }
}
