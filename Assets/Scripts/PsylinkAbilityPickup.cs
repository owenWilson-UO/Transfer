using System.Collections;
using UnityEngine;

public class PsylinkAbilityPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TutorialState tutorialState;
    [SerializeField] PlayerUpgradeData playerUpgradeData;
    [SerializeField] GameObject magicCircle;
    [SerializeField] PsylinkThrowable pt;
    [SerializeField] CanvasGroup cg;


    [Header("Settings")]
    [SerializeField] private float speed;

    public bool firstTimeGrabbed { get; private set; }

    private void Start()
    {
        firstTimeGrabbed = playerUpgradeData.maxPsylinkAmount == 0;

        if (!firstTimeGrabbed)
        {
            magicCircle.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Despawn());
            if (tutorialState.showPsylinkPopup)
            {
                tutorialState.showPsylinkPopup = false;
                StartCoroutine(ToastCoroutine());
            }
        }

        magicCircle.SetActive(false);
    }

    private IEnumerator Despawn()
    {
        float elapsed = 0f;
        float spawnDuration = 0.2f;
        Vector3 _knifeRestScale = transform.localScale;

        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, elapsed / spawnDuration);
            transform.localScale = new Vector3(
                Mathf.Lerp(_knifeRestScale.x, 0f, p),
                Mathf.Lerp(_knifeRestScale.y, 0f, p),
                Mathf.Lerp(_knifeRestScale.z, 0f, p)
            );
            yield return null;
        }
        transform.localScale = Vector3.zero;

        if (playerUpgradeData.maxPsylinkAmount == 0)
        {
            playerUpgradeData.maxPsylinkAmount = 1;
        }
    }

    IEnumerator ToastCoroutine()
    {

        float elapsed = 0f;
        float fadeDuration = 0.2f;
        float duration = 2f;

        while (elapsed <= fadeDuration)
        {
            cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        cg.alpha = 1f;
        yield return new WaitForSecondsRealtime(duration);

        elapsed = 1f;
        while (elapsed >= 0f)
        {
            cg.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            elapsed -= Time.unscaledDeltaTime;
            yield return null;
        }
        cg.alpha = 0f;
        gameObject.SetActive(false);

    }
}
