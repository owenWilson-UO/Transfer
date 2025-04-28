using System.Collections;
using UnityEngine;

public class TransferThrowable : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;
    public UpgradeManagerUI upgradeManagerUI;
    [Tooltip("The knife model in the player's hand")]
    [SerializeField] private GameObject handKnife;

    [Header("Throwing")]
    public KeyCode throwKey = KeyCode.Mouse1;
    public float throwForce;
    public float throwUpwardForce;

    [Header("Player")]
    [SerializeField] private PlayerUpgradeData upgradeData;
    public int transferAmount { get; set; }

    [Header("Print-In Animation")]
    [Tooltip("How long the print-in takes")]
    [SerializeField] private float spawnDuration = 0.3f;

    private bool readyToThrow;
    private Rigidbody rb;
    private PlayerMovement playerMovement;

    // cache the knife’s “ready” scale
    private Vector3 _knifeRestScale;
    private Coroutine _printCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();
        readyToThrow = true;

        // initialize charge count
        transferAmount = upgradeData.maxTransferAmount;

        // cache the knife’s normal scale
        if (handKnife != null)
        {
            _knifeRestScale = handKnife.transform.localScale;
            handKnife.SetActive(transferAmount > 0);
        }
    }

    void Update()
    {
        var td = FindFirstObjectByType<ThrowableDetection>();

        if (Input.GetKeyDown(throwKey) && !upgradeManagerUI.isOpen)
        {
            if (readyToThrow && transferAmount > 0)
            {
                Throw();
            }
            else if (td != null)
            {
                TeleportToTransfer(td);
            }
        }

        if (td != null && td.targetHit)
            TeleportToTransfer(td);
    }

    private void Throw()
    {
        readyToThrow = false;
        handKnife?.SetActive(false);    // hide in-hand knife

        // spawn projectile
        var proj = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        var rbProj = proj.GetComponent<Rigidbody>();
        Vector3 dir = cam.forward;
        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, 500f))
            dir = (hit.point - attackPoint.position).normalized;

        rbProj.AddForce(dir * throwForce + transform.up * throwUpwardForce, ForceMode.Impulse);
    }

    private void TeleportToTransfer(ThrowableDetection td)
    {
        // move player
        rb.isKinematic = true;
        rb.position = td.transform.position;
        playerMovement.gravityMultiplier = 0f;
        rb.isKinematic = false;
        rb.linearVelocity = td.rb.linearVelocity * 1.25f;
        rb.angularVelocity = td.rb.angularVelocity * 1.25f;

        Destroy(td.gameObject);
        ResetThrow();
    }

    private void ResetThrow()
    {
        transferAmount--;
        readyToThrow = true;

        if (transferAmount > 0)
            SpawnHandKnife();
    }

    /// <summary>
    /// Activates the handKnife and “prints” it by animating its Y-scale from 0→full.
    /// </summary>
    public void SpawnHandKnife()
    {
        if (handKnife == null || handKnife.activeSelf) return;

        handKnife.SetActive(true);

        // stop any ongoing print animation
        if (_printCoroutine != null)
            StopCoroutine(_printCoroutine);

        // start from zero height
        var t = handKnife.transform;
        t.localScale = new Vector3(_knifeRestScale.x, 0f, _knifeRestScale.z);
        _printCoroutine = StartCoroutine(PrintCoroutine());
    }

    private IEnumerator PrintCoroutine()
    {
        float elapsed = 0f;
        var t = handKnife.transform;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            t.localScale = new Vector3(
                _knifeRestScale.x,
                Mathf.Lerp(0f, _knifeRestScale.y, p),
                _knifeRestScale.z
            );
            yield return null;
        }

        // ensure exact final scale
        t.localScale = _knifeRestScale;
        _printCoroutine = null;
    }
}
