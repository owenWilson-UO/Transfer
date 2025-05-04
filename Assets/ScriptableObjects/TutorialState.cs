using UnityEngine;

[CreateAssetMenu(fileName = "TutorialState", menuName = "Scriptable Objects/TutorialState")]
public class TutorialState : ScriptableObject
{
    public bool showMovementPopup = true;
    public bool showSprintJumpPopup = true;
    public bool showSlidePopup = true;
    public bool showWallRunPopup = true;

    private void OnEnable()
    {
        showMovementPopup = true;
        showSprintJumpPopup = true;
        showSlidePopup = true;
        showWallRunPopup = true;
    }
}
