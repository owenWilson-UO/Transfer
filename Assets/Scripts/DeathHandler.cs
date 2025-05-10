using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathHandler : MonoBehaviour
{
    private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    private UpgradeManagerUI upgradeManagerUI;

    private void Start()
    {
        fadeImage = GameObject.Find("playerFade").GetComponent<Image>();
        upgradeManagerUI = FindAnyObjectByType<UpgradeManagerUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            upgradeManagerUI.canOpen = false;
            StartCoroutine(FadeToBlack(fadeDuration));
        }
    }

    public IEnumerator FadeToBlack(float fadeDuration)
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void Reload()
    {
        upgradeManagerUI.canOpen = false;
        StartCoroutine(FadeToBlack(0f));
    } 
}
