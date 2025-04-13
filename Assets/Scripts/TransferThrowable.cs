using System.Net.NetworkInformation;
using UnityEngine;

public class TransferThrowable : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    //[Header("Settings")]

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse1;
    public float throwForce;
    public float throwUpwardForce;

    bool readyToThrow;
    Rigidbody rb;

    private void Start()
    {
        readyToThrow = true;
        rb = GetComponent<Rigidbody>();
    }

    private void Throw ()
    {
        readyToThrow = false;

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();

        Vector3 forceDir = cam.transform.forward;

        RaycastHit hit;

        if (Physics.Raycast(cam.position, cam.forward, out hit, 500f))
        {
            forceDir = (hit.point - attackPoint.position).normalized;
        }

        Vector3 forceToAdd = forceDir * throwForce + transform.up * throwUpwardForce;

        projectileRB.AddForce(forceToAdd, ForceMode.Impulse);
    }

    public void ResetThrow()
    {
        readyToThrow = true;
    }

    private void Update()
    {
        ThrowableDetection td = FindFirstObjectByType<ThrowableDetection>();
        if (Input.GetKeyDown(throwKey))
        {
            if (readyToThrow)
            {
                Throw();
            }
            else
            {
                Transfer(td.transform.position, td.rb.linearVelocity, td.rb.angularVelocity);
                Destroy(td.gameObject);
                readyToThrow = true;
            }
        }

        if (td && td.targetHit)
        {
            Transfer(td.transform.position, rb.linearVelocity, rb.angularVelocity);
            Destroy(td.gameObject);
            readyToThrow = true;
        }
    }

    private void Transfer(Vector3 toPosition, Vector3 toLinearVelocity, Vector3 toAngularVelocity)
    {
        rb.isKinematic = true;
        rb.position = toPosition;
        rb.isKinematic = false;
        rb.linearVelocity = toLinearVelocity;
        rb.angularVelocity = toAngularVelocity;
    }
}
