using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.InputSystem.Controls;

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

    [Header("Select On Open Buttons")]
    [SerializeField] GameObject retryPause;
    [SerializeField] GameObject TT1;

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
    private Coroutine animationTimer;
    private Animator upgradeAnimator;

    private readonly Color blueOpaque = new Color(0f, 188f, 255f, 125f) / 255f;
    private readonly Color blue = new Color(0f, 188f, 255f, 255f) / 255f;

    private bool LastInputWasKeyboardOrMouse;
    private bool PreviousLastInputWasKeyboardOrMouse;

    private void OnEnable()
    {
        upgradeMenuButton.action.started += OnUpgradePress;
        upgradeMenuButton.action.Enable();

        pauseButton.action.started += OnPausePress;
        pauseButton.action.Enable();
    }

    private void OnDisable()
    {
        upgradeMenuButton.action.started -= OnUpgradePress;
        upgradeMenuButton.action.Disable();

        pauseButton.action.started -= OnPausePress;
        pauseButton.action.Disable();
    }

    private void OnUpgradePress(CallbackContext ctx)
    {
        if (canOpen && !endScreen.levelComplete && !playerMovement.isInSlowMotion && !isPaused && (upgradeData.maxSlowMotionDuration > 0f || upgradeData.maxTransferAmount > 0 || upgradeData.maxPsylinkAmount > 0))
        {
            //logic for opening the upgrade menu and closing it based on the key press
            isOpen = !isOpen;

            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isOpen;

            EventSystem.current.SetSelectedGameObject(null);
            if (isOpen)
            {
                if (animationTimer != null)
                {
                    StopCoroutine(animationTimer);
                }
                animationTimer = StartCoroutine(AnimationTimer(0.5f, ctx.control.device.name));

                if (ctx.control.device.name != "Keyboard")
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }

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
        }
    }

    private void OnPausePress(CallbackContext ctx)
    {
        if (canOpen && !endScreen.levelComplete && !playerMovement.isInSlowMotion && !isOpen)
        {
            isPaused = !isPaused;

            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;

            EventSystem.current.SetSelectedGameObject(null);
            if (isPaused)
            {
                Debug.Log(pauseButton.action.activeControl);
                if (ctx.control.device.name != "Keyboard")
                {
                    EventSystem.current.SetSelectedGameObject(retryPause);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            } 

            playerCG.alpha = isPaused ? 0f : 1f;
            Time.timeScale = isPaused ? 0f : 1f;
            pauseCG.alpha = isPaused ? 1f : 0f;
            pauseCG.blocksRaycasts = isPaused;
            pauseCG.interactable = isPaused;
            dof.focalLength.value = isPaused ? 100f : 0f;
        }
    }

    private void Start()
    {
        //When the next level starts, in order for the upgrades to persist, we need to read
        //in the values from the scriptable object and assign to the UI accordingly upon start

        #region Slow Motion On Start
        switch (upgradeData.maxSlowMotionDuration)
        {
            case 2f:
                SetColorBlock(SMT2);
                SM12.color = blueOpaque;
                break;
            case 3f:
                SetColorBlock(SMT3);
                SetColorBlock(SMT2);
                SM12.color = blueOpaque;
                SM23.color = blueOpaque;
                break;
            default:
                break;
        }
        #endregion

        #region Transfer On Start
        switch (upgradeData.maxTransferAmount)
        {
            case 2:
                SetColorBlock(TT2);      
                T12.color = blueOpaque;
                break;
            case 3:
                SetColorBlock(TT3);
                SetColorBlock(TT2);
                T12.color = blueOpaque;
                T23.color = blueOpaque;
                break;
            default:
                break;
        }
        #endregion

        #region Psylink On Start
        switch (upgradeData.maxPsylinkAmount)
        {
            case 2:
                SetColorBlock(P2);
                P12.color = blueOpaque;
                break;
            case 3:
                SetColorBlock(P3);
                SetColorBlock(P2);
                P12.color = blueOpaque;
                P23.color = blueOpaque;
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
        SlowMotion.SetActive(upgradeData.maxSlowMotionDuration > 0f && isOpen);
        Transfer.SetActive(upgradeData.maxTransferAmount > 0 && isOpen);
        Psylink.SetActive(upgradeData.maxPsylinkAmount > 0 && isOpen);

        GetLastInput();

        if (isOpen)
        {
            upgradeAnimator.Update(Time.unscaledDeltaTime);
        }


        batteryCount.text = upgradeData.batteries.ToString();
        
    }

    //Methods that are tied to the upgrade buttons

    #region Slow Motion Upgrades
    public void OnSlowMotionTier2Press()
    {
        if (upgradeData.maxSlowMotionDuration == 1f && upgradeData.batteries >= 1)
        {
            upgradeData.maxSlowMotionDuration = 2f;
            SetColorBlock(SMT2);
            SM12.color = blueOpaque;
            upgradeData.batteries--;
        }
    }

    public void OnSlowMotionTier3Press()
    {
        if (upgradeData.maxSlowMotionDuration == 2f && upgradeData.batteries >= 2)
        {
            upgradeData.maxSlowMotionDuration = 3f;
            SetColorBlock(SMT3);
            SM23.color = blueOpaque;
            upgradeData.batteries-=2;
        }
    }
    #endregion

    #region Transfer Upgrades
    public void OnTransferTier2Press()
    {
        if (upgradeData.maxTransferAmount == 1 && upgradeData.batteries >= 1)
        {

            upgradeData.maxTransferAmount = 2;
            T12.color = blue;
            SetColorBlock(TT2);
            upgradeData.batteries--;
        }
    }

    public void OnTransferTier3Press()
    {
        if (upgradeData.maxTransferAmount == 2 && upgradeData.batteries >= 2)
        {
            upgradeData.maxTransferAmount = 3;
            SetColorBlock(TT3);
            T23.color = blueOpaque;

            upgradeData.batteries-=2;
        }
    }
    #endregion

    #region Psylink Upgrades
    public void OnPsylinkTier2Press()
    {
        if (upgradeData.maxPsylinkAmount == 1 && upgradeData.batteries >= 1)
        {
            upgradeData.maxPsylinkAmount = 2;
            SetColorBlock(P2);
            P12.color = blueOpaque;

            upgradeData.batteries--;
        }
    }

    public void OnPsylinkTier3Press()
    {
        if (upgradeData.maxPsylinkAmount == 2 && upgradeData.batteries >= 2)
        {
            upgradeData.maxPsylinkAmount = 3;
            SetColorBlock(P3);
            P23.color = blueOpaque;

            upgradeData.batteries-=2;
        }
    }
    #endregion

    private void GetLastInput()
    {
        //need to detect if the last input was from the keyboard or not
        //to determine if we should select the ui element when the level is complete or not
        bool gamepadUsed = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
        bool keyboardOrMouseUsed = false;

        // Detect keyboard input
        foreach (KeyControl key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                keyboardOrMouseUsed = true;
                break;
            }
        }

        // Detect mouse input
        if (Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame ||
            Mouse.current.delta.ReadValue() != Vector2.zero)
        {
            keyboardOrMouseUsed = true;
        }

        // Determine last input source based on what happened this frame
        if (keyboardOrMouseUsed)
            LastInputWasKeyboardOrMouse = true;
        else if (gamepadUsed)
            LastInputWasKeyboardOrMouse = false;

        if (isPaused || isOpen)
        {
            Cursor.lockState = LastInputWasKeyboardOrMouse ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = LastInputWasKeyboardOrMouse;

            if (!LastInputWasKeyboardOrMouse && PreviousLastInputWasKeyboardOrMouse)
            {
                EventSystem.current.SetSelectedGameObject(isPaused ? retryPause : TT1);
            }
        }

        PreviousLastInputWasKeyboardOrMouse = LastInputWasKeyboardOrMouse;
    }

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

    private void SetColorBlock(Button b)
    {
        ColorBlock cb = b.colors;

        cb.normalColor = blue;
        cb.highlightedColor = blue;
        cb.pressedColor = blue;
        cb.selectedColor = blue;
        cb.disabledColor = blue;

        b.colors = cb;
    }

    IEnumerator AnimationTimer(float duration, string deviceName)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (isOpen)
        {
            if (deviceName != "Keyboard")
            {
                EventSystem.current.SetSelectedGameObject(TT1);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
