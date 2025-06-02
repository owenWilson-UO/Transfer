using UnityEngine;
using System.Collections;

/// <summary>
/// When added to any GameObject, this will:
/// 1) Fade all of its Renderer materials to black over `fadeDuration` seconds.
/// 2) Spawn a fire‐particle prefab at its position (as a child).
/// 3) Wait `destroyDelay` seconds, then destroy the entire GameObject.
/// </summary>
public class IgniteAndDestroy : MonoBehaviour
{
    [Header("Fire & Timing Settings")]
    [Tooltip("Particle System prefab that represents the “ignited” flames.")]
    public GameObject firePrefab;

    [Tooltip("How long (in seconds) to interpolate the material’s color to black.")]
    public float fadeDuration = 1f;

    [Tooltip("How long (in seconds) to wait AFTER fading before destroying this object.")]
    public float destroyDelay = 2f;

    private void Start()
    {
        // As soon as this component is added, begin the fade → ignite → destroy sequence.
        StartCoroutine(FadeThenIgnite());
    }

    private IEnumerator FadeThenIgnite()
    {
        // 1) Grab every Renderer in this object (and its children).
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        // Store original colors for each material.
        Material[][] materialsPerRenderer = new Material[renderers.Length][];
        Color[][] originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            // Duplicate the material array reference so we can modify the colors in place.
            materialsPerRenderer[i] = renderers[i].materials;
            originalColors[i] = new Color[materialsPerRenderer[i].Length];
            for (int j = 0; j < materialsPerRenderer[i].Length; j++)
            {
                originalColors[i][j] = materialsPerRenderer[i][j].color;
            }
        }

        // 2) Over `fadeDuration` seconds, lerp each material’s color to black.
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < materialsPerRenderer[i].Length; j++)
                {
                    Color orig = originalColors[i][j];
                    materialsPerRenderer[i][j].color = Color.Lerp(orig, Color.black, t);
                }
            }

            yield return null;
        }

        // 3) Ensure it's fully black
        for (int i = 0; i < renderers.Length; i++)
        {
            for (int j = 0; j < materialsPerRenderer[i].Length; j++)
            {
                materialsPerRenderer[i][j].color = Color.black;
            }
        }

        // 4) Spawn the fire effect at this object’s position
        if (firePrefab != null)
        {
            GameObject fireInstance = Instantiate(
                firePrefab,
                transform.position,
                Quaternion.identity
            );
            // Parent it so that if the object moves/dies, the fire effect follows
            fireInstance.transform.SetParent(transform, worldPositionStays: true);
        }

        // 5) Wait a bit, then destroy the target GameObject
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }
}
