using UnityEngine;
using System.Collections;
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
        Vector3 spawnPos = other.bounds.center - Vector3.up * other.bounds.extents.y;

        if (firePrefab == null)
        {
            Debug.LogWarning("IgnitionDetection: firePrefab is not assigned.");
        }
        else
        {
            // 2) Instantiate the fire prefab WITHOUT parenting, so we can measure its native size.
            GameObject fireInstance = Instantiate(
                firePrefab,
                spawnPos,
                Quaternion.identity
            );

            // 3) Look for any Renderer on the fireInstance (or its children).
            Renderer fireRenderer = fireInstance.GetComponentInChildren<Renderer>();
            if (fireRenderer != null)
            {
                // 4) Get the world‐space size of the fire effect right now:
                Vector3 fireSize = fireRenderer.bounds.size;
                // 5) Get the world‐space size of the target object:
                Vector3 targetSize = other.bounds.size;

                // 6) Compute how much to scale the fire so its bounds match the target’s bounds.
                //    Avoid division by zero if fireSize.x (etc.) is zero.
                Vector3 scaleFactor = Vector3.one;
                if (fireSize.x > 0f) scaleFactor.x = targetSize.x / fireSize.x;
                if (fireSize.y > 0f) scaleFactor.y = targetSize.y / fireSize.y;
                if (fireSize.z > 0f) scaleFactor.z = targetSize.z / fireSize.z;

                // 7) Apply that local scale. Because we aren’t parented yet,
                //    setting localScale to scaleFactor gives us the correct world size.
                fireInstance.transform.localScale = scaleFactor;
            }
            else
            {
                Debug.LogWarning("IgnitionDetection: No Renderer found on firePrefab or its children. Cannot auto‐scale.");
            }

            // 8) Now parent the fireInstance under the hit object,
            //    so that as the object moves/rots, the fire follows.
            fireInstance.transform.SetParent(other.transform, worldPositionStays: true);
            
        }
        // Renderer[] targetRenderers = other.GetComponentsInChildren<Renderer>();
        // if (targetRenderers.Length > 0)
        // {
        //     // Add helper component to handle color transition
        //     BurnColorLerper colorLerper = other.gameObject.AddComponent<BurnColorLerper>();
        //     colorLerper.StartColorLerp(targetRenderers, destroyDelay);

        // }

        // 9) Destroy the target object after a delay:
        Destroy(other.gameObject, destroyDelay);

        // 10) Destroy the knife itself if requested:
        if (destroySelfOnHit)
            Destroy(gameObject);
    }
     
}
