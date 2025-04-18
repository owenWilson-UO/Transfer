using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManagerUI : MonoBehaviour
{
    [SerializeField] PlayerUpgradeData upgradeData;

    [Header("Buttons")]
    [SerializeField] Button SMT2;
    [SerializeField] Button SMT3;
    [SerializeField] Button TT2;
    [SerializeField] Button TT3;

    [Header("Links")]
    [SerializeField] Image SM12;
    [SerializeField] Image SM23;
    [SerializeField] Image T12;
    [SerializeField] Image T23;

    [Header("Canvas Groups")]
    CanvasGroup cg;
    [SerializeField] CanvasGroup playerCG;

    [Header("KeyBinds")]
    [SerializeField] KeyCode upgradeMenuKey;

    public bool isOpen { private set; get; }
    private Coroutine fadeRoutine;

    private readonly Color blue = new Color(0f, 188f, 255f, 125f) / 255f;


    private void Start()
    {
        #region Slow Motion On Start
        switch (upgradeData.maxSlowMotionDuration)
        {
            case 2f:
                SMT2.interactable = false;
                SM12.color = blue;
                break;
            case 3f:
                SMT3.interactable = false;
                SMT2.interactable = false;
                SM12.color = blue;
                SM23.color = blue;
                break;
            default:
                break;
        }
        #endregion

        #region Transfer On Start
        switch (upgradeData.maxTransferAmount)
        {
            case 2:
                TT2.interactable = false;
                T12.color = blue;
                break;
            case 3:
                TT3.interactable = false;
                TT2.interactable = false;
                T12.color = blue;
                T23.color = blue;
                break;
            default:
                break;
        }
        #endregion

        cg = GetComponent<CanvasGroup>();
        
        isOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(upgradeMenuKey))
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                cg.interactable = true;
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeUI(isOpen));

            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }

        if (isOpen)
        {
            Time.timeScale = 0f;
        }
    }

    #region Slow Motion Upgrades
    public void OnSlowMotionTier2Press()
    {
        upgradeData.maxSlowMotionDuration = 2f;
        SMT2.interactable = false;
        SM12.color = blue;
    }

    public void OnSlowMotionTier3Press()
    {
        if (upgradeData.maxSlowMotionDuration == 2f)
        {
            upgradeData.maxSlowMotionDuration = 3f;
            SMT3.interactable = false;
            SM23.color = blue;
        }
    }
    #endregion

    #region Transfer Upgrades
    public void OnTransferTier2Press()
    {
        upgradeData.maxTransferAmount = 2;
        TT2.interactable = false;
        T12.color = blue;
    }

    public void OnTransferTier3Press()
    {
        if (upgradeData.maxTransferAmount == 2)
        {
            upgradeData.maxTransferAmount = 3;
            TT3.interactable = false;
            T23.color = blue;
        }
    }
    #endregion

    IEnumerator FadeUI(bool opening)
    {
        float t = 0f;
        float duration = 0.1f;

        float startAlphaCG = cg.alpha;
        float targetAlphaCG = opening ? 1f : 0f;

        float startAlphaPlayer = playerCG.alpha;
        float targetAlphaPlayer = opening ? 0f : 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            cg.alpha = Mathf.Lerp(startAlphaCG, targetAlphaCG, lerp);
            playerCG.alpha = Mathf.Lerp(startAlphaPlayer, targetAlphaPlayer, lerp);

            yield return null;
        }

        cg.alpha = targetAlphaCG;
        playerCG.alpha = targetAlphaPlayer;
        cg.interactable = opening;
        if (!opening)
        {
            Time.timeScale = 1f;
        }
    }
}
