// HUDManager.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUpgradeData playerUpgradeData;
    [SerializeField] private TextMeshProUGUI transferAmountText;
    [SerializeField] private TextMeshProUGUI psylinkAmountText;
    [SerializeField] private TextMeshProUGUI ignitionAmountText;

    [SerializeField] private TransferThrowable tt;
    [SerializeField] private PsylinkThrowable pt;
    [SerializeField] private IgnitionThrowable it;

    [SerializeField] private RectTransform slowMotionTimer;
    [SerializeField] private RectTransform transferUI;
    [SerializeField] private RectTransform psylinkUI;
    [SerializeField] private RectTransform ignitionUI;

    [Header("Cooldowns")]
    [SerializeField] private Image transferFillImage;
    [SerializeField] private Image transferFillBorder;
    [SerializeField] private Image transferTextImage;

    [SerializeField] private Image ignitionFillImage;
    [SerializeField] private Image ignitionFillBorder;
    [SerializeField] private Image ignitionTextImage;

    private Coroutine transferCoroutine;
    private Coroutine ignitionCoroutine;
    private Color blue = new Color(0f, 186f / 255f, 255f / 255f);
    private Color scarlet = new Color(255f, 62f / 255f, 0f / 255f);
    private Color grey = new Color(150f / 255f, 150f / 255f, 150f / 255f);

    void Update()
    {
        slowMotionTimer.localScale = playerUpgradeData.maxSlowMotionDuration == 0f ? Vector3.zero : new Vector3(1.4f, 1.38f, 1.5f);
        transferUI.localScale = playerUpgradeData.maxTransferAmount == 0 ? Vector3.zero : new Vector3(0.65f, 0.65f, 0.65f);
        psylinkUI.localScale = playerUpgradeData.maxPsylinkAmount == 0 ? Vector3.zero : new Vector3(0.65f, 0.65f, 0.65f);
        ignitionUI.localScale = playerUpgradeData.maxIgnitionAmount == 0 ? Vector3.zero : new Vector3(0.65f, 0.65f, 0.65f);

        transferAmountText.text = tt.transferAmount.ToString();
        psylinkAmountText.text = (playerUpgradeData.maxPsylinkAmount - pt.activePsylinks.Count).ToString();
        ignitionAmountText.text = it.ignitionAmount.ToString();

        transferTextImage.color = tt.transferAmount == 0 ? grey : blue;
        ignitionTextImage.color = (it.ignitionAmount == 0) ? grey : scarlet;   // ← new

        if (tt.transferAmount < playerUpgradeData.maxTransferAmount && transferCoroutine == null && !tt.TransferLockout)
        {
            transferCoroutine = StartCoroutine(TransferCooldown());
        }

        if (tt.TransferLockout)
        {
            if (transferCoroutine != null) { StopCoroutine(transferCoroutine); }
            transferCoroutine = null;
            tt.transferAmount = 0;
            transferFillImage.fillAmount = 0f;
            transferFillBorder.fillAmount = 0f;
        }

        if (it.ignitionAmount < playerUpgradeData.maxIgnitionAmount
            && ignitionCoroutine == null
            && !it.IgnitionLockout)
        {
            ignitionCoroutine = StartCoroutine(IgnitionCooldown());
        }

        // If ignitor is locked‐out (e.g. “held” or banned), forcibly reset UI
        if (it.IgnitionLockout)
        {
            if (ignitionCoroutine != null)
            {
                StopCoroutine(ignitionCoroutine);
            }
            ignitionCoroutine = null;
            it.ignitionAmount = 0;
            ignitionFillImage.fillAmount = 0f;
            ignitionFillBorder.fillAmount = 0f;
        }
    }

    IEnumerator TransferCooldown()
    {
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transferFillImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            if (tt.transferAmount == 0)
                transferFillBorder.fillAmount = Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        // Replenish one transfer
        tt.transferAmount++;
        transferFillImage.fillAmount = 1f;
        transferFillBorder.fillAmount = 1f;

        // Tell TransferThrowable to respawn the knife in hand
        tt.SpawnHandKnife();

        StopCoroutine(transferCoroutine);
        transferCoroutine = null;
    }

    IEnumerator IgnitionCooldown()
    {
        
        float duration = 5f;   
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            ignitionFillImage.fillAmount = Mathf.Clamp01(elapsed / duration);
            if (it.ignitionAmount == 0)
                ignitionFillBorder.fillAmount = Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        // Replenish one ignition charge
        it.ignitionAmount++;
        ignitionFillImage.fillAmount = 1f;
        ignitionFillBorder.fillAmount = 1f;

        // Tell IgnitionThrowable to respawn the knife in hand
        it.SpawnHandKnife();

        StopCoroutine(ignitionCoroutine);
        ignitionCoroutine = null;
    }
}
