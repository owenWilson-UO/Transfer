using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] WallRun wallRun;
    [SerializeField] UpgradeManagerUI upgradeManagerUI;

    [Header("Mouse Movement")]
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] float multiplier = 0.01f;

    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;


    float mouseX;
    float mouseY;


    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cam.localPosition = new Vector3(0f, 0f, 0.15f); // this positions the camera to a point that feels like the correct fov with the fps hands
    }

    private void Update()
    {
        MyInput();
    }

    void MyInput()
    {
        if (!upgradeManagerUI.isOpen)
        {
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");

            yRotation += mouseX * sensX * multiplier;
            xRotation -= mouseY * sensY * multiplier;

            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
            orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
            // using eulers here to avoid any unity physics based camera movements because of the slow motion ability,
            // we want the player's ability to look around be independent from the time slow
        }
    }
}
