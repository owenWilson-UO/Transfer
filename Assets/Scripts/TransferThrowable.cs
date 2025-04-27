// TransferThrowable.cs
using System.Net.NetworkInformation;
using UnityEngine;

public class TransferThrowable : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    [SerializeField] private GameObject handKnife;    // assign your in-hand knife here
    public UpgradeManagerUI upgradeManagerUI;

    [Header("Throwing")]
    [SerializeField] public KeyCode throwKey = KeyCode.Mouse1;
    [SerializeField] public float throwForce;
    [SerializeField] public float throwUpwardForce;

    [Header("Player")]
    [SerializeField] PlayerUpgradeData upgradeData;
    public int transferAmount { get; set; }

    bool readyToThrow;
    Rigidbody rb;
    PlayerMovement playerMovement;

    private void Start()
    {
        readyToThrow = true;
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        transferAmount = upgradeData.maxTransferAmount;

        // Initialize the hand-knife visibility
        if (handKnife != null)
            handKnife.SetActive(transferAmount > 0);
    }

    private void Throw()
    {
        readyToThrow = false;

        // Hide the knife in your hand
        if (handKnife != null)
            handKnife.SetActive(false);

        // Spawn projectile
        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        projectile.transform.rotation = Quaternion.LookRotation(new Ray(cam.position, cam.forward).direction);
        projectile.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
        projectile.transform.rotation *= Quaternion.Euler(0f, 90f, 0f);

        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();

        Vector3 forceDir = cam.forward;
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f))
            forceDir = (hit.point - attackPoint.position).normalized;

        Vector3 forceToAdd = forceDir * throwForce + transform.up * throwUpwardForce;
        projectileRB.AddForce(forceToAdd, ForceMode.Impulse);
    }

    public void ResetThrow()
    {
        transferAmount--;
        readyToThrow = true;

        // if you still have knives left, immediately respawn the next one
        if (transferAmount > 0)
            SpawnHandKnife();
    }

    // Called by HUDManager when cooldown finishes
    public void SpawnHandKnife()
    {
        if (handKnife != null)
            handKnife.SetActive(true);
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
            else if (td)
            {
                Transfer(td.transform.position, td.rb.linearVelocity, td.rb.angularVelocity);
                Destroy(td.gameObject);
                ResetThrow();
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
        rb.isKinematic = true;
        rb.position = toPosition;
        playerMovement.gravityMultiplier = 0;
        rb.isKinematic = false;
        rb.linearVelocity = toLinearVelocity * 1.25f;
        rb.angularVelocity = toAngularVelocity * 1.25f;
    }
}
