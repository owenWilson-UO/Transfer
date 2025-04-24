using System.Net.NetworkInformation;
using UnityEngine;

public class TransferThrowable : MonoBehaviour
{
    
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UpgradeManagerUI upgradeManagerUI;


    //[Header("Settings")]

    [Header("Throwing")]
    [SerializeField] public KeyCode throwKey = KeyCode.Mouse1;
    [SerializeField] public float throwForce;
    [SerializeField] public float throwUpwardForce;
    
    [Header("Player")]
    [SerializeField] PlayerUpgradeData upgradeData;
    public int transferAmount {  get; set; } // public set because we impliment cooldown logic in the HUDManager script

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
        projectile.transform.rotation = Quaternion.LookRotation(new Ray(cam.position, cam.forward).direction);
        Quaternion xTilt = Quaternion.Euler(90f, 0f, 0f);
        projectile.transform.rotation = projectile.transform.rotation * xTilt;

        Quaternion yTilt = Quaternion.Euler(0f, 90f, 0f);
        projectile.transform.rotation = projectile.transform.rotation * yTilt;

        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();

        Vector3 forceDir = cam.transform.forward;

        RaycastHit hit;

        //using a raycast to give the transfer a target to throw towards and add force to the transfer on instantiation
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
        transferAmount--;
    }

    private void Update()
    {
        ThrowableDetection td = FindFirstObjectByType<ThrowableDetection>();
        if (Input.GetKeyDown(throwKey) && !upgradeManagerUI.isOpen)
        {
            if (readyToThrow && transferAmount > 0)
            {
                Throw();
            }
            else
            {
                // If the user has thrown a Transfer but it hasn't collided with anything, teleport the user to the transfer
                // along with the transfer's velocity

                if (td)
                {
                    Transfer(td.transform.position, td.rb.linearVelocity, td.rb.angularVelocity);
                    Destroy(td.gameObject);
                    ResetThrow();
                }
                readyToThrow = true; //just in case something wierd happens
            }
        }

        if (td && td.targetHit)
        {
            Transfer(td.transform.position, rb.linearVelocity, rb.angularVelocity);
            Destroy(td.gameObject);
            ResetThrow();
        }
    }

    private void Transfer(Vector3 toPosition, Vector3 toLinearVelocity, Vector3 toAngularVelocity)
    {
        //Logic for teleportation

        rb.isKinematic = true;
        rb.position = toPosition;
        playerMovement.gravityMultiplier = 0;
        rb.isKinematic = false;
        rb.linearVelocity = toLinearVelocity * 1.25f;
        rb.angularVelocity = toAngularVelocity * 1.25f;
    }
}
