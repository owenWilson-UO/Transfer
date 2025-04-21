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
    [SerializeField] public KeyCode throwKey = KeyCode.Mouse1;
    [SerializeField] public float throwForce;
    [SerializeField] public float throwUpwardForce;
    
    [Header("Player")]
    [SerializeField] PlayerUpgradeData upgradeData;
    private int transferAmount;

    bool readyToThrow;
    Rigidbody rb;
    PlayerMovement playerMovement;
    

    private void Start()
    {
        readyToThrow = true;
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        

        transferAmount = upgradeData.maxTransferAmount; //start at 0 since we will have to get this item in the tutorial and then pickup stations will replenish to maxTransfers
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
            if (readyToThrow && transferAmount > 0)
            {
                Throw();
                transferAmount--;
            }
            else
            {
                if (td)
                {
                    Transfer(td.transform.position, td.rb.linearVelocity, td.rb.angularVelocity);
                    Destroy(td.gameObject);
                }
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
        playerMovement.gravityMultiplier = 0;
        rb.isKinematic = false;
        rb.linearVelocity = toLinearVelocity;
        rb.angularVelocity = toAngularVelocity;
    }
}
