using System.Collections;
using UnityEngine;

public class AbilityPickupIgnition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUpgradeData playerUpgradeData;
    [SerializeField] private IgnitionThrowable it;       // Your IgnitionThrowable component

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 50f;   // How fast the pickup spins
    [SerializeField] private float despawnDuration = 0.2f;// How long it takes to scale down

    private bool firstTimeGrabbed;

    private void Start()
    {
        // Only show this pickup if the player hasn’t unlocked ignition yet
        firstTimeGrabbed = (playerUpgradeData.maxIgnitionAmount == 0);
        if (!firstTimeGrabbed)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Simple rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!firstTimeGrabbed) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(DespawnAndGrant());
        }
    }

    private IEnumerator DespawnAndGrant()
    {
        // Scale down over despawnDuration seconds
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < despawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / despawnDuration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        // Ensure fully zero‐scaled, then disable
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);

        // Grant ignition ability
        if (playerUpgradeData.maxIgnitionAmount == 0)
        {
            playerUpgradeData.maxIgnitionAmount = 1;
            it.ignitionAmount = 1;
            it.SpawnHandKnife();
        }
    }
}
