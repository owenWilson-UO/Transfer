using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class EndScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUpgradeData playerUpgradeData;
    [SerializeField] private Timer timer;
    [SerializeField] private UpgradeManagerUI upgradeManagerUI;
    [SerializeField] private Volume volume;
    [SerializeField] private TextMeshProUGUI endTime;

    [Header("Settings")]
    [SerializeField] private LevelName level;

    [Header("Time Requirements")]
    [SerializeField] private float battery1;
    [SerializeField] private float battery2;
    [SerializeField] private float battery3;

    [Header("Battery Images")]
    [SerializeField] private Image batteryImage1;
    [SerializeField] private Image batteryImage2;
    [SerializeField] private Image batteryImage3;

    private DepthOfField dof;
    private CanvasGroup cg;
    private Coroutine fadeToEndScreen;

    private Coroutine batCoroutine1;
    private Coroutine batCoroutine2;
    private Coroutine batCoroutine3;

    private float timeToComplete;

    private float timeSpeed = 10f;

    public bool levelComplete { get; private set; }
    public bool animatiionDone { get; private set; } = false;
    void Start()
    {
        cg = GetComponent<CanvasGroup>();

        if (volume != null && volume.profile.TryGet(out dof))
        {
            dof.focalLength.overrideState = true;
        }

        timeToComplete = battery1;
        endTime.text = timeToComplete.ToString("0.00");
    }

    private void Update()
    {
        if (animatiionDone && timeToComplete > timer.currentTime)
        {

            timeToComplete = timeToComplete - Time.unscaledDeltaTime * timeSpeed > timer.currentTime ? timeToComplete - Time.unscaledDeltaTime * timeSpeed : timer.currentTime;
            endTime.text = timeToComplete.ToString("0.00");

            if (timeToComplete < battery1 && batCoroutine1 == null) { batCoroutine1 = StartCoroutine(IncreaseScale(batteryImage1)); }
            if (timeToComplete < battery2 && batCoroutine2 == null) { batCoroutine2 = StartCoroutine(IncreaseScale(batteryImage2)); }
            if (timeToComplete < battery3 && batCoroutine3 == null) { batCoroutine3 = StartCoroutine(IncreaseScale(batteryImage3)); }
        }
    }

    public void StartEndScreen()
    {
        levelComplete = true;
        fadeToEndScreen = StartCoroutine(OpenSequence());
    }

    IEnumerator OpenSequence()
    {
        yield return new WaitForSeconds(4f);
        yield return StartCoroutine(upgradeManagerUI.FadeUI(true));

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float lerp = Mathf.Clamp01(elapsed / duration);
            float easeOut = 1f - Mathf.Pow(1f - lerp, 3f);

            cg.alpha = lerp;
            Time.timeScale = 1f - easeOut;
            dof.focalLength.value = Mathf.Lerp(0f, 100f, lerp);

            yield return null;
        }

        cg.alpha = 1f;
        dof.focalLength.value = 50f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        animatiionDone = true;

        if (timer.currentTime > battery1)
        {
            timeToComplete = timer.currentTime;
            endTime.text = timeToComplete.ToString("0.00");
        }

        int batteryCount = 0;
        if (timer.currentTime < battery1 && playerUpgradeData.batteriesCollectedByLevel[level] < 1) { batteryCount++; playerUpgradeData.batteriesCollectedByLevel[level]++; }
        if (timer.currentTime < battery2 && playerUpgradeData.batteriesCollectedByLevel[level] < 2) { batteryCount++; playerUpgradeData.batteriesCollectedByLevel[level]++; }
        if (timer.currentTime < battery3 && playerUpgradeData.batteriesCollectedByLevel[level] < 3) { batteryCount++; playerUpgradeData.batteriesCollectedByLevel[level]++; }
        playerUpgradeData.batteries += batteryCount;
    }

    IEnumerator IncreaseScale(Image image)
    {
        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            image.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        image.rectTransform.localScale = Vector3.one;
    }
}
