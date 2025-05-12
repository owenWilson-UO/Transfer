using System.Collections;
using UnityEngine;

public class TutorialPopupHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TutorialState tutorialState;
    [SerializeField] private CanvasGroup movement;
    [SerializeField] private CanvasGroup sprintJump;
    [SerializeField] private CanvasGroup slide;
    [SerializeField] private CanvasGroup wallrun;
    [SerializeField] private CanvasGroup transfer;

    [Header("Seetings")]
    [SerializeField] private float fadeDuration;
    [SerializeField] private float duration;

    private Coroutine toast;
    private CanvasGroup activeCG;

    public void ShowPopup(string id)
    {
        if (toast != null) {
            StopCoroutine(toast);
            if (activeCG)
            {
                activeCG.alpha = 0f;
            }
            activeCG = null;
        }
        
        // handles logic for showing and hiding the tutorial notifications

        switch (id)
        {
            case "movement":
                if (tutorialState.showMovementPopup)
                {
                    tutorialState.showMovementPopup = false;
                    toast = StartCoroutine(ToastCoroutine(movement));
                }
                break;
            case "sprintJump":
                if (tutorialState.showSprintJumpPopup)
                {
                    tutorialState.showSprintJumpPopup = false;
                    toast = StartCoroutine(ToastCoroutine(sprintJump));
                }
                break;
            case "slide":
                if (tutorialState.showSlidePopup)
                {
                    tutorialState.showSlidePopup = false;
                    toast = StartCoroutine(ToastCoroutine(slide));
                }
                break;
            case "wallrun":
                if (tutorialState.showWallRunPopup)
                {
                    tutorialState.showWallRunPopup = false;
                    toast = StartCoroutine(ToastCoroutine(wallrun));
                }
                break;
            case "TransferPickup":
                if (tutorialState.showTransferPopup)
                {
                    tutorialState.showTransferPopup = false;
                    toast = StartCoroutine(ToastCoroutine(transfer));
                }
                break;
            default: break;
        }
    }

    IEnumerator ToastCoroutine(CanvasGroup cg)
    {
        activeCG = cg;

        float elapsed = 0f;

        while (elapsed <= fadeDuration)
        {
            cg.alpha = Mathf.Clamp01(elapsed/fadeDuration);
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
    }
}
