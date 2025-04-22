using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PsylinkThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private RawImage dot;
    [SerializeField] private RawImage outerCrossHair;
    [SerializeField] private RawImage circleDot;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject objectToThrow;

    [Header("KeyBinds")]
    [SerializeField] KeyCode throwKey;

    public bool psylinkInSight {  get; private set; }
    public bool readyToThrow;

    private void Start()
    {
        readyToThrow = true; //change when we add pickups
    }

    private void Update()
    {
        //This is the logic for changing the crosshair when the player is looking
        //at a psyling interactable object (object with the PsylinkInteractable tag)
        RaycastHit hit;
        psylinkInSight = Physics.Raycast(cam.position, cam.forward, out hit, 20f) && hit.transform.CompareTag("PsylinkInteractable");
        if (psylinkInSight && readyToThrow)
        {
            dot.color = new(dot.color.r, dot.color.g, dot.color.b, 0f);
            circleDot.color = new(dot.color.r, dot.color.g, dot.color.b, 1f);
            outerCrossHair.rectTransform.Rotate(0f, 0f, 100f * Time.unscaledDeltaTime);
        }
        else
        {
            dot.color = new(dot.color.r, dot.color.g, dot.color.b, 1f);
            circleDot.color = new(dot.color.r, dot.color.g, dot.color.b, 0f);
            outerCrossHair.rectTransform.rotation = Quaternion.identity;
        }

        if (Input.GetKeyDown(throwKey))
        {
            if (psylinkInSight && readyToThrow) //later on add max Psylink var and skill tree upgrades
            {
                Throw(hit.point); // We pass in the exact point of the interactable that the
                                  // player is looking at so when the user throws the psylink, it goes straight to that point
                readyToThrow = false;
            }
        }

    }

    private void Throw(Vector3 point)
    {

        GameObject projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        Rigidbody projectileRB = projectile.GetComponent<Rigidbody>();
        Vector3 direction = (point - attackPoint.position).normalized;
        projectileRB.linearVelocity = direction * 30f;
    }

    private void OnDrawGizmos()
    {
        //This is for drawing out the raycast in the scene view for testing
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cam.position, cam.forward * 20f + cam.position);
    }
}
