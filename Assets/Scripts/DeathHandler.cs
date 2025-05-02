using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathHandler : MonoBehaviour
{
    private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        fadeImage = GameObject.Find("playerFade").GetComponent<Image>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeToBlack());
        }
    }

    public IEnumerator FadeToBlack()
    {
        Color color = fadeImage.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    //public IEnumerator FadeFromBlack()
    //{
    //    Color color = fadeImage.color;
    //    float time = 0f;

    //    while (time < fadeDuration)
    //    {
    //        time += Time.deltaTime;
    //        color.a = 1f - Mathf.Clamp01(time / fadeDuration);
    //        fadeImage.color = color;
    //        yield return null;
    //    }

    //    color.a = 0f;
    //    fadeImage.color = color;
    //}
}
