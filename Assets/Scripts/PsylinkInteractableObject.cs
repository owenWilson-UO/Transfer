using UnityEngine;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

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

    private void Start()
    {
        a = transform.position;
    }

    private void Update()
    {

        //updates the objects position go towards point b if a psylink is attatched, or back to point a if there is no psylink attatched
        Vector3 target = pt.activePsylinks.Any(item => item.obj == gameObject) ? b.position : a;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        
        //this boolean is being used to stop the player from being able to throw a psylink at the object if it is moving
        isMoving = Vector3.Distance(transform.position, target) > 0.01f;
    }
}
