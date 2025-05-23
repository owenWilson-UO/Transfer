using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathHandler : MonoBehaviour
{
    private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] LevelName levelName;
    [SerializeField] private AbilityPickup transferPickup;
    [SerializeField] private PsylinkAbilityPickup psylinkPickup;
    [SerializeField] PlayerUpgradeData playerUpgradeData;
    private UpgradeManagerUI upgradeManagerUI;

    private void Start()
    {
        fadeImage = GameObject.Find("playerFade").GetComponent<Image>();
        upgradeManagerUI = FindAnyObjectByType<UpgradeManagerUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Transfer"))
        {
            ResetAbilities();
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
        StartCoroutine(FadeToBlack(0f));
    } 

    private void ResetAbilities()
    {
        if (levelName == LevelName.Tutorial && transferPickup.firstTimeGrabbed && playerUpgradeData.maxTransferAmount == 1)
        {
            playerUpgradeData.maxTransferAmount = 0;
        }

        if (levelName == LevelName.Level2 && psylinkPickup.firstTimeGrabbed && playerUpgradeData.maxPsylinkAmount == 1)
        {
            playerUpgradeData.maxPsylinkAmount = 0;
        }
        upgradeManagerUI.canOpen = false;
    }
}
