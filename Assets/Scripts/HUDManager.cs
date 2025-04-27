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
    [SerializeField] private TransferThrowable tt;
    [SerializeField] private PsylinkThrowable pt;

    [Header("Cooldowns")]
    [SerializeField] private Image transferFillImage;
    [SerializeField] private Image transferFillBorder;
    [SerializeField] private Image transferTextImage;

    private Coroutine transferCoroutine;
    private Color blue = new Color(0f, 186f/255f, 255f/255f);
    private Color grey = new Color(150f/255f, 150f/255f, 150f/255f);

    void Update()
    {
        transferAmountText.text = tt.transferAmount.ToString();
        psylinkAmountText.text = (playerUpgradeData.maxPsylinkAmount - pt.activePsylinks.Count).ToString();

        transferTextImage.color = tt.transferAmount == 0 ? grey : blue; 
        
        if (tt.transferAmount < playerUpgradeData.maxTransferAmount && transferCoroutine == null)
            transferCoroutine = StartCoroutine(TransferCooldown());
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
}
