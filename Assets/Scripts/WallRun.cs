using UnityEngine;

public class WallRun : MonoBehaviour
{
    [SerializeField] Transform orientation;
    [SerializeField] PlayerMovement playerMovement;

    [Header("Detection")]
    [SerializeField] private float wallDistance = 0.5f;
    [SerializeField] private float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunJumpForce;

    [Header("Camera")]
    [SerializeField] private Camera cam;
    [SerializeField] private float fov;
    [SerializeField] private float wallRunfov;
    [SerializeField] private float wallRunfovTime;
    [SerializeField] private float cameraTilt;
    [SerializeField] private float cameraTiltTime;
    
    public float tilt { get; private set; }

    bool wallLeft = false;
    bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position + Vector3.up, -orientation.right, out leftWallHit, wallDistance);
        wallRight = Physics.Raycast(transform.position + Vector3.up, orientation.right, out rightWallHit, wallDistance);

        Debug.DrawRay(transform.position + Vector3.up, -orientation.right * wallDistance, Color.red);
        Debug.DrawRay(transform.position + Vector3.up, orientation.right * wallDistance, Color.blue);
        Debug.DrawRay(transform.position, Vector3.down * minimumJumpHeight, Color.green);
    }

    private void Update()
    {
        CheckWall();

        if (CanWallRun() && !playerMovement.isGrounded)
        {
            if (wallLeft)
            {
                StartWallRun();
                Debug.Log("Wall is on the left");
            }
            else if (wallRight)
            {
                StartWallRun();
                Debug.Log("Wall is on the right");
            }
            else
            {
                StopWallRun();
            }
        } 
        else
        {
            StopWallRun();
        }
    }

    void StartWallRun()
    {
        rb.useGravity = false;

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        if (wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -cameraTilt, cameraTiltTime * Time.deltaTime);
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(tilt, cameraTilt, cameraTiltTime * Time.deltaTime);
        }

        if (Input.GetKeyDown(playerMovement.jumpKey))
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDir = transform.up + leftWallHit.normal;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(wallRunJumpDir * wallRunJumpForce * 100, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDir = transform.up + rightWallHit.normal;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(wallRunJumpDir * wallRunJumpForce * 100, ForceMode.Force);
            }            
        }
    }

    void StopWallRun()
    {
        rb.useGravity = true;

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, wallRunfovTime * Time.deltaTime);
        tilt = Mathf.Lerp(tilt, 0, cameraTiltTime * Time.deltaTime);


    }
}
