using UnityEngine;
using System.Linq;

public class PsylinkInteractableObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PsylinkThrowable pt;
    [SerializeField] private Transform b;
    private Vector3 a;

    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    public bool isMoving {  get; private set; }
    private Vector3 velocity = Vector3.zero;
    private float easeAmount = 2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource interactionAudio;
    [SerializeField] private AudioClip onPsylinkAttachedClip;
    private bool wasActive = false;


    private void Start()
    {
        a = transform.position;
    }

    private void Update()
    {
        bool isActive = pt.activePsylinks.Any(item => item.obj == gameObject);

        // Play sound on transition from not active to active
        bool shouldPlaySound = isActive && !wasActive && Vector3.Distance(transform.position, b.position) > 0.1f;

        if (shouldPlaySound && interactionAudio && onPsylinkAttachedClip)
        {
            interactionAudio.PlayOneShot(onPsylinkAttachedClip);
        }
        Vector3 target = isActive ? b.position : a;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        isMoving = Vector3.Distance(transform.position, target) > 0.01f;

        wasActive = isActive; 
    }
}
