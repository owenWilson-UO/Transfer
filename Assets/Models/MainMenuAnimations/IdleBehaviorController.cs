using UnityEngine;
using UnityEngine.EventSystems;

public class IdleBehaviorController : MonoBehaviour, IPointerClickHandler
{
    private Animator animator;
    public Camera mainCamera;
    public float interval =10f;
    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private string[] triggers = { "LookAround", "LookDown", "SadIdle" };
    private float timer;
    public string idleStateName = "Idle";

    void Start()
    {
        timer = interval;
        animator = GetComponent<Animator>();
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            TriggerRandomIdle();
            timer = interval;

        }
    }

    private void LateUpdate()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (!stateInfo.IsName("Backflip"))
        {
            transform.localRotation = initialRotation;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, initialPosition.z);
        }
        else
        {
            transform.localPosition = new Vector3(initialPosition.x, transform.localPosition.y, initialPosition.z);
        }
    }

    void TriggerRandomIdle()
    {
        int index = Random.Range(0, triggers.Length);
        animator.SetTrigger(triggers[index]);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                animator.SetTrigger("Backflip");
                timer += 4.1f;
            }
        }
    }
}
