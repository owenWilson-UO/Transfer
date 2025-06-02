using UnityEngine;

public class IgnitionDetection : MonoBehaviour
{
    [Header("Fire Effect")]
    [Tooltip("Drag your fire‐particle prefab here.")]
    [SerializeField] private GameObject firePrefab;

    [Tooltip("Seconds to wait before actually destroying the hit object.")]
    [SerializeField] private float destroyDelay = 2f;
    

    [Tooltip("If true, the knife destroys itself on impact.")]
    [SerializeField] private bool destroySelfOnHit = true;

    private void OnTriggerEnter(Collider other)
    {
        // Only act on objects tagged "IgnitionInteractable"
        if (!other.gameObject.CompareTag("IgnitionInteractable"))
            return;

        // 1) Compute a spawn position "on top" of the target's collider.
        //    We take the collider's bounds center + its half‐height along Y.
        Vector3 spawnPos = other.bounds.center - Vector3.up * other.bounds.extents.y;

        // 2) Instantiate the fire effect there.
        if (firePrefab != null)
        {
            GameObject fireInstance = Instantiate(
                firePrefab,
                spawnPos,
                Quaternion.identity,
                other.transform  // <— parent to “other”
            );
        }
        else
        {
            Debug.LogWarning("IgnitionDetection: firePrefab is not assigned.");
        }

        // 3) Destroy the target object immediately (or you can add a small delay if you like)
        Destroy(other.gameObject, destroyDelay);

        // 4) Destroy the knife itself, if requested
        if (destroySelfOnHit)
            Destroy(gameObject);
    }
}
