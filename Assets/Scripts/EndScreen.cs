using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
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

    [Header("Batteries Collected Messages")]
    [SerializeField] private TextMeshProUGUI batteriesCollected;
    [SerializeField] private TextMeshProUGUI batteryMessage;
    [SerializeField] private TextMeshProUGUI nextBatteryTime;

    [Header("Button to Activate")]
    [SerializeField] private GameObject nextButton;

    private DepthOfField dof;
    private CanvasGroup cg;
    private Coroutine fadeToEndScreen;

    private Coroutine batCoroutine1;
    private Coroutine batCoroutine2;
    private Coroutine batCoroutine3;

    private Coroutine batteriesCollectedCoroutine;
    private Coroutine batteryMessageCoroutine;
    private Coroutine nextBatteryTimeCoroutine;

    private float timeToComplete;

    private float timeSpeed = 10f;

    public bool levelComplete { get; private set; }
    public bool animationDone { get; private set; } = false;

    private bool LastInputWasKeyboardOrMouse;
    private bool PreviousLastInputWasKeyboardOrMouse;

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

        if (animationDone)
        {
            Cursor.lockState = LastInputWasKeyboardOrMouse ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = LastInputWasKeyboardOrMouse;

            if (!LastInputWasKeyboardOrMouse && PreviousLastInputWasKeyboardOrMouse)
            {
                EventSystem.current.SetSelectedGameObject(nextButton);
            }
        }

        PreviousLastInputWasKeyboardOrMouse = LastInputWasKeyboardOrMouse;


        if (animationDone && timeToComplete > timer.currentTime)
        {

            timeToComplete = timeToComplete - Time.unscaledDeltaTime * timeSpeed > timer.currentTime ? timeToComplete - Time.unscaledDeltaTime * timeSpeed : timer.currentTime;
            endTime.text = timeToComplete.ToString("0.00");

            if (timeToComplete < battery1 && batCoroutine1 == null) { batCoroutine1 = StartCoroutine(IncreaseScale(batteryImage1)); }
            if (timeToComplete < battery2 && batCoroutine2 == null) { batCoroutine2 = StartCoroutine(IncreaseScale(batteryImage2)); }
            if (timeToComplete < battery3 && batCoroutine3 == null) { batCoroutine3 = StartCoroutine(IncreaseScale(batteryImage3)); }
        }
        else if (timeToComplete <= timer.currentTime)
        {
            int numberOfBatteriesCollected = playerUpgradeData.batteriesCollectedByLevel[LevelName.Tutorial];
            if (numberOfBatteriesCollected == 3 && batteriesCollectedCoroutine == null)
            {
                batteriesCollectedCoroutine = StartCoroutine(IncreaseScale(batteriesCollected));
            }
            else
            {
                switch (numberOfBatteriesCollected)
                {
                    case 0:
                        nextBatteryTime.text = battery1.ToString("0.00");
                        break;
                    case 1:
                        nextBatteryTime.text = battery2.ToString("0.00");
                        break;
                    case 2:
                        nextBatteryTime.text = battery3.ToString("0.00");
                        break;
                }

                if (numberOfBatteriesCollected < 3 && batteryMessageCoroutine == null && nextBatteryTimeCoroutine == null)
                {
                    batteryMessageCoroutine = StartCoroutine(IncreaseScale(batteryMessage));
                    nextBatteryTimeCoroutine = StartCoroutine(IncreaseScale(nextBatteryTime));
                }
            }
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
        dof.focalLength.value = 100f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        Time.timeScale = 0f;
        animationDone = true;

        nextButton.GetComponent<Button>().interactable = true;
        nextButton.GetComponent<Button>().enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
        if (!LastInputWasKeyboardOrMouse)
        {
            EventSystem.current.SetSelectedGameObject(nextButton);
        }

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

    IEnumerator IncreaseScale(Graphic image)
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
