using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;


public class PsylinkThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cam;
    [SerializeField] private RawImage dot;
    [SerializeField] private RawImage outerCrossHair;
    [SerializeField] private RawImage circleDot;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private GameObject objectToThrow;
    [SerializeField] private PlayerUpgradeData playerUpgradeData;
    [SerializeField] private UpgradeManagerUI upgradeManagerUI;

    [SerializeField] private AnimationStateController rightAnimController;
    [SerializeField] private AnimationStateController leftAnimController;

    [Header("KeyBinds")]
    [SerializeField] InputActionReference throwButton;

    public bool psylinkInSight {  get; private set; }
    public bool readyToThrow;
    [SerializeField] private GameObject projectile;
    private bool playDespawn = false;
    public List<PsylinkAndObject> activePsylinks { get; private set; }

    private void Start()
    {
        readyToThrow = true; //change when we add pickups
        activePsylinks = new List<PsylinkAndObject>();
    }

    private void OnEnable()
    {
        throwButton.action.Enable();
    }

    private void OnDisable()
    {
        throwButton.action.Disable();
    }

    private void Update()
    {
        //This is the logic for changing the crosshair when the player is looking
        //at a psyling interactable object (object with the PsylinkInteractable tag)
        RaycastHit hit;
        psylinkInSight = Physics.Raycast(cam.position, cam.forward, out hit, 30f) && hit.transform.CompareTag("PsylinkInteractable") && !activePsylinks.Any(item => item.obj == hit.transform.gameObject) && !hit.transform.GetComponent<PsylinkInteractableObject>().isMoving && playerUpgradeData.maxPsylinkAmount > 0;
        if (psylinkInSight && readyToThrow)
        {
            dot.color = new(dot.color.r, dot.color.g, dot.color.b, 0f);
            circleDot.color = new(dot.color.r, dot.color.g, dot.color.b, 1f);
            outerCrossHair.rectTransform.Rotate(0f, 0f, 100f * Time.unscaledDeltaTime);
            leftAnimController.PlayPsylinkWindup(true);
            if (projectile.transform.localScale == Vector3.zero)
            {
                PsylinkDetection pd = projectile.GetComponent<PsylinkDetection>();
                pd.playSpawnAnimation = true;
                playDespawn = true;
                projectile.transform.rotation = Quaternion.LookRotation(new Ray(cam.position, cam.forward).direction);
                Quaternion tilt = Quaternion.Euler(90f, 0f, 0f);
                projectile.transform.rotation = projectile.transform.rotation * tilt;
            }
        }
        else
        {
            dot.color = new(dot.color.r, dot.color.g, dot.color.b, 1f);
            circleDot.color = new(dot.color.r, dot.color.g, dot.color.b, 0f);
            outerCrossHair.rectTransform.rotation = Quaternion.identity;
            
            if (playDespawn && readyToThrow)
            {
                playDespawn = false;
                PsylinkDetection pd = projectile.GetComponent<PsylinkDetection>();
                pd.playDestroyAnimation = true;

            }
        }
        if (!psylinkInSight)
        {
            leftAnimController.PlayPsylinkWindup(false);
        }

        if (throwButton.action.triggered && !upgradeManagerUI.isOpen)
        {
            if (psylinkInSight && readyToThrow) //later on add max Psylink var and skill tree upgrades
            {
                if (activePsylinks.Count >= playerUpgradeData.maxPsylinkAmount)
                {
                    Destroy(activePsylinks[0].psylink);
                    activePsylinks.RemoveAt(0);
                }
                leftAnimController.PlayPsylinkThrow();
                Throw(hit.point); // We pass in the exact point of the interactable that the
                                  // player is looking at so when the user throws the psylink, it goes straight to that point
                readyToThrow = false;
            }
        }
    }

    private void Throw(Vector3 point)
    {
        PsylinkDetection pd = projectile.GetComponent<PsylinkDetection>();
        pd.StopSpawnCoroutine();
        projectile.transform.localScale = Vector3.zero;
        playDespawn = false;


        GameObject projectileThrow = Instantiate(objectToThrow, attackPoint.position, cam.rotation);
        projectileThrow.transform.rotation = Quaternion.LookRotation(new Ray(cam.position, cam.forward).direction);
        Quaternion tilt = Quaternion.Euler(90f, 0f, 0f);
        projectileThrow.transform.rotation = projectileThrow.transform.rotation * tilt;
        Rigidbody projectileRB = projectileThrow.GetComponent<Rigidbody>();
        StartCoroutine(MoveToTarget(projectileThrow.transform, point, 0.25f));
    }

    private IEnumerator MoveToTarget(Transform obj, Vector3 target, float duration)
    {
        Vector3 start = obj.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (obj == null) yield break; //In case it's destroyed mid flight
            obj.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (obj != null)
            obj.position = target;
    }

    private void OnDrawGizmos()
    {
        //This is for drawing out the raycast in the scene view for testing
        Gizmos.color = Color.green;
        Gizmos.DrawLine(cam.position, cam.forward * 20f + cam.position);
    }
}

public class PsylinkAndObject
{
    public GameObject obj;
    public GameObject psylink;
    public PsylinkAndObject() { }
}
