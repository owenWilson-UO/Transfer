using UnityEngine;
using System.Collections;

public class BurnColorLerper : MonoBehaviour
{
    /// <summary>
    /// Kicks off a coroutine that fades each renderer’s base color → black over `duration` seconds,
    /// but only on materials that actually have a _BaseColor or _Color property.
    /// </summary>
    public void StartColorLerp(Renderer[] renderers, float duration)
    {
        StartCoroutine(LerpColors(renderers, duration));
    }

    private IEnumerator LerpColors(Renderer[] renderers, float duration)
    {
        int n = renderers.Length;
        Material[]  materials     = new Material[n];
        string[]    colorProperty = new string[n];
        Color[]     startColors   = new Color[n];

        // 1) For each renderer, grab its runtime material instance and figure out which color property to use:
        for (int i = 0; i < n; i++)
        {
            materials[i] = renderers[i].material;

            // Prefer "_BaseColor" (URP Lit), else try "_Color" (legacy shaders), else null:
            if (materials[i].HasProperty("_BaseColor"))
            {
                colorProperty[i] = "_BaseColor";
                startColors[i]   = materials[i].GetColor("_BaseColor");
            }
            else if (materials[i].HasProperty("_Color"))
            {
                colorProperty[i] = "_Color";
                startColors[i]   = materials[i].GetColor("_Color");
            }
            else
            {
                // This material has no color we can lerp—mark as skip:
                colorProperty[i] = null;
                startColors[i]   = Color.clear;
            }
        }

        // 2) Lerp from startColors[i] → black over “duration” seconds only on supported properties:
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < n; i++)
            {
                if (colorProperty[i] != null)
                {
                    // Compute the interpolated color:
                    Color c = Color.Lerp(startColors[i], Color.black, t);
                    materials[i].SetColor(colorProperty[i], c);
                }
            }

            yield return null;
        }

        // 3) At the end, ensure any material that had a color property is fully black:
        for (int i = 0; i < n; i++)
        {
            if (colorProperty[i] != null)
                materials[i].SetColor(colorProperty[i], Color.black);
        }
    }
}
