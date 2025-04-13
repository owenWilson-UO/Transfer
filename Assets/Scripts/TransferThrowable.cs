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
        if (Input.GetKeyDown(throwKey))
        {
            if (readyToThrow)
            {
                Throw();
            }
            else
            {
                ThrowableDetection td = FindFirstObjectByType<ThrowableDetection>();
                rb.isKinematic = true;
                rb.position = td.transform.position;
                rb.linearVelocity = td.rb.linearVelocity;
                rb.angularVelocity = td.rb.angularVelocity;
                rb.isKinematic = false;
                Destroy(td.gameObject);
                readyToThrow = true;
            }
        }
    }
}
