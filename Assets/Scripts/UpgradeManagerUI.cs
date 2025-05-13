using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UpgradeManagerUI : MonoBehaviour
{
    [SerializeField] PlayerUpgradeData upgradeData;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] EndScreen endScreen;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI batteryCount;

    [Header("Trees")]
    [SerializeField] GameObject SlowMotion;
    [SerializeField] GameObject Transfer;
    [SerializeField] GameObject Psylink;
    //[SerializeField] GameObject Ignition; NOT IMPLEMENTED YET

    [Header("Buttons")]
    [SerializeField] Button SMT2;
    [SerializeField] Button SMT3;
    [SerializeField] Button TT2;
    [SerializeField] Button TT3;
    [SerializeField] Button P2;
    [SerializeField] Button P3;

    [Header("Links")]
    [SerializeField] Image SM12;
    [SerializeField] Image SM23;
    [SerializeField] Image T12;
    [SerializeField] Image T23;
    [SerializeField] Image P12;
    [SerializeField] Image P23;

    [Header("Canvas Groups")]
    CanvasGroup cg;
    [SerializeField] CanvasGroup playerCG;
    [SerializeField] CanvasGroup pauseCG;

    [Header("Pause")]
    [SerializeField] private Volume volume;
    private DepthOfField dof;

    [Header("KeyBinds")]
    [SerializeField] InputActionReference upgradeMenuButton;
    [SerializeField] InputActionReference pauseButton;

    public bool isOpen { private set; get; }
    public bool isPaused { private set; get; }
    public bool canOpen = true;
    private Coroutine fadeRoutine;
    private Animator upgradeAnimator;

    private readonly Color blue = new Color(0f, 188f, 255f, 125f) / 255f;

    private void OnEnable()
    {
        upgradeMenuButton.action.Enable();
        pauseButton.action.Enable();
    }

    private void OnDisable()
    {
        upgradeMenuButton.action.Disable();
        pauseButton.action.Disable();
    }

    private void Start()
    {
        //When the next level starts, in order for the upgrades to persist, we need to read
        //in the values from the scriptable object and assign to the UI accordingly upon start

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

        #region Psylink On Start
        switch (upgradeData.maxPsylinkAmount)
        {
            case 2:
                P2.interactable = false;
                P12.color = blue;
                break;
            case 3:
                P3.interactable = false;
                P2.interactable = false;
                P12.color = blue;
                P23.color = blue;
                break;
            default:
                break;
        }
        #endregion

        cg = GetComponent<CanvasGroup>();
        upgradeAnimator = GetComponent<Animator>();
        isOpen = false;
        isPaused = false;
        canOpen = true;

        if (volume != null && volume.profile.TryGet(out dof))
        {
            dof.focalLength.overrideState = true;
        }
    }

    private void Update()
    {
        SlowMotion.SetActive(upgradeData.maxSlowMotionDuration > 0f);
        Transfer.SetActive(upgradeData.maxTransferAmount > 0);
        Psylink.SetActive(upgradeData.maxPsylinkAmount > 0);

        if (upgradeMenuButton.action.triggered && canOpen && !endScreen.levelComplete && !playerMovement.isInSlowMotion && !isPaused && (upgradeData.maxSlowMotionDuration> 0f || upgradeData.maxTransferAmount > 0 || upgradeData.maxPsylinkAmount > 0))
        {
            //logic for opening the upgrade menu and closing it based on the key press
            isOpen = !isOpen;

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);

            if (isOpen)
            {
                cg.interactable = true;
                fadeRoutine = StartCoroutine(OpenSequence());
            }
            else
            {
                //this boolean controls the animation state of the upgrade menu
                upgradeAnimator.SetBool("isOpen", false);
                fadeRoutine = StartCoroutine(FadeUI(false));
            }


            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;
        }
        
        if (isOpen)
        {
            upgradeAnimator.Update(Time.unscaledDeltaTime);
            //Time.timeScale = 0f;
        }

        if (pauseButton.action.triggered && canOpen && !endScreen.levelComplete && !playerMovement.isInSlowMotion && !isOpen)
        {
            isPaused = !isPaused;

            playerCG.alpha = isPaused ? 0f : 1f;
            Time.timeScale = isPaused ? 0f : 1f;
            pauseCG.alpha = isPaused ? 1f : 0f;
            pauseCG.blocksRaycasts = isPaused;
            pauseCG.interactable = isPaused;
            dof.focalLength.value = isPaused ? 100f : 0f;

            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }

        batteryCount.text = upgradeData.batteries.ToString();
        
    }

    //Methods that are tied to the upgrade buttons

    #region Slow Motion Upgrades
    public void OnSlowMotionTier2Press()
    {
        if (upgradeData.batteries >= 1)
        {
            upgradeData.maxSlowMotionDuration = 2f;
            SMT2.interactable = false;
            SM12.color = blue;
            upgradeData.batteries--;
        }
    }

    public void OnSlowMotionTier3Press()
    {
        if (upgradeData.maxSlowMotionDuration == 2f && upgradeData.batteries >= 2)
        {
            upgradeData.maxSlowMotionDuration = 3f;
            SMT3.interactable = false;
            SM23.color = blue;
            upgradeData.batteries-=2;
        }
    }
    #endregion

    #region Transfer Upgrades
    public void OnTransferTier2Press()
    {
        if (upgradeData.batteries >= 1)
        {
            upgradeData.maxTransferAmount = 2;
            TT2.interactable = false;
            T12.color = blue;

            upgradeData.batteries--;
        }
    }

    public void OnTransferTier3Press()
    {
        if (upgradeData.maxTransferAmount == 2 && upgradeData.batteries >= 2)
        {
            upgradeData.maxTransferAmount = 3;
            TT3.interactable = false;
            T23.color = blue;

            upgradeData.batteries-=2;
        }
    }
    #endregion

    #region Psylink Upgrades
    public void OnPsylinkTier2Press()
    {
        if (upgradeData.batteries >= 1)
        {
            upgradeData.maxPsylinkAmount = 2;
            P2.interactable = false;
            P12.color = blue;

            upgradeData.batteries--;
        }
    }

    public void OnPsylinkTier3Press()
    {
        if (upgradeData.maxPsylinkAmount == 2 && upgradeData.batteries >= 2)
        {
            upgradeData.maxPsylinkAmount = 3;
            P3.interactable = false;
            P23.color = blue;

            upgradeData.batteries-=2;
        }
    }
    #endregion

    public IEnumerator FadeUI(bool opening)
    {
        float t = 0f;
        float duration = 0.1f;

        float startAlphaPlayer = playerCG.alpha;
        float targetAlphaPlayer = opening ? 0f : 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.Clamp01(t / duration);

            playerCG.alpha = Mathf.Lerp(startAlphaPlayer, targetAlphaPlayer, lerp);

            yield return null;
        }

        playerCG.alpha = targetAlphaPlayer;
        cg.interactable = opening;
        if (!opening)
        {
            Time.timeScale = 1f;
        }
    }

    IEnumerator OpenSequence()
    {
        yield return StartCoroutine(FadeUI(true));
        upgradeAnimator.SetBool("isOpen", true);

        Time.timeScale = 0f;
    }
}
