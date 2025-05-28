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

    //Check if player is wallrunning using rb.useGravity

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    public Vector3 wallNormal {  get; private set; }

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
        wallLeft = false;
        wallRight = false;

        Vector3 origin = transform.position;
        Vector3 leftDir = -orientation.right;
        Vector3 leftAngledDir = Quaternion.AngleAxis(-45f, Vector3.up) * leftDir;

        Vector3 rightDir = orientation.right;
        Vector3 rightAngledDir = Quaternion.AngleAxis(45f, Vector3.up) * rightDir;

        RaycastHit hit;

        // LEFT side raycasts
        if ((Physics.Raycast(origin, leftDir, out hit, wallDistance) ||
             Physics.Raycast(origin, leftAngledDir, out hit, wallDistance)) &&
            !hit.collider.CompareTag("NoWallRun"))
        {
            wallLeft = true;
            leftWallHit = hit;
        }

        // RIGHT side raycasts
        if ((Physics.Raycast(origin, rightDir, out hit, wallDistance) ||
             Physics.Raycast(origin, rightAngledDir, out hit, wallDistance)) &&
            !hit.collider.CompareTag("NoWallRun"))
        {
            wallRight = true;
            rightWallHit = hit;
        }

        // Debug rays
        Debug.DrawRay(origin, leftDir * wallDistance, Color.red);
        Debug.DrawRay(origin, leftAngledDir * wallDistance, Color.red * 0.75f);
        Debug.DrawRay(origin, rightDir * wallDistance, Color.blue);
        Debug.DrawRay(origin, rightAngledDir * wallDistance, Color.blue * 0.75f);
        Debug.DrawRay(transform.position, Vector3.down * minimumJumpHeight, Color.green);
    }

    private void Update()
    {
        CheckWall();

        if (CanWallRun() && !playerMovement.isGrounded && playerMovement.verticalMovement > 0)
        {
            if (wallLeft)
            {
                wallNormal = leftWallHit.normal;
                StartWallRun();
            }
            else if (wallRight)
            {
                wallNormal = rightWallHit.normal;
                StartWallRun();
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
        if (rb.useGravity)
        {
            //stop upwards momentum when wall running
            Vector3 velocity = rb.linearVelocity;
            if (velocity.y > 0)
            {
                velocity.y = velocity.y/2;
                rb.linearVelocity = velocity;
            }
        }
        rb.useGravity = false;

        //apply wall run gravity instead of unity's gravity
        //rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);
        //removed for testing new wall run

        //this is the logic for adjusting the camera to give a nice transition to and from wall running
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRunfov, wallRunfovTime * Time.deltaTime);

        if (wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -cameraTilt, cameraTiltTime * Time.deltaTime);
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(tilt, cameraTilt, cameraTiltTime * Time.deltaTime);
        }

        //this is the logic for wall jumping / jumping off of a wall
        if (playerMovement.jumpButton.action.triggered)
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
