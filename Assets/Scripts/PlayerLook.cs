using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] WallRun wallRun;
    [SerializeField] UpgradeManagerUI upgradeManagerUI;
    [SerializeField] EndScreen endScreen;
    [SerializeField] PlayerUpgradeData playerUpgradeData;

    [Header("Mouse Movement")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] Slider _slider;
    [SerializeField] TextMeshProUGUI sliderVal;

    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;


    float mouseX;
    float mouseY;

    float rightStickX;
    float rightStickY;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam.localPosition = new Vector3(0f, 0f, 0.15f); // this positions the camera to a point that feels like the correct fov with the fps hands
    }

    void Awake()
    {
        sliderVal.text = playerUpgradeData.sensMultiplier.ToString("0");
        _slider.SetValueWithoutNotify(playerUpgradeData.sensMultiplier);

        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnDestroy()
    {
        _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void Update()
    {
        MyInput();
    }

    public void OnSliderValueChanged(float newValue)
    {
        playerUpgradeData.sensMultiplier = newValue;
        sliderVal.text = newValue.ToString("0");
    }

    void MyInput()
    {
        if (!upgradeManagerUI.isOpen && !upgradeManagerUI.isPaused && !endScreen.animationDone)
        {
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");

            //Controller
            rightStickX = Input.GetAxis("RightStickHorizontal");
            rightStickY = Input.GetAxis("RightStickVertical");

            if (rightStickX != 0f || rightStickY != 0f)
            {
                Debug.Log("look around");
            }

            float lookX = mouseX + rightStickX;
            float lookY = mouseY + rightStickY;

            yRotation += lookX * sensX * playerUpgradeData.sensMultiplier;
            xRotation -= lookY * sensY * playerUpgradeData.sensMultiplier;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
            orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            // using eulers here to avoid any unity physics based camera movements because of the slow motion ability,
            // we want the player's ability to look around be independent from the time slow
        }
    }
}
