using UnityEngine;

public class ThrowableDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }

    public bool targetHit {  get; private set; }

    private bool isSpinning;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isSpinning = true;
    }

    private void Update()
    {
        if (isSpinning)
        {
            transform.Rotate(0f, 0f, 1080f * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //logic for stopping the transfer when it collides with an object
        if (collision.gameObject.CompareTag("Player")) { return; }
        isSpinning = false;
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
