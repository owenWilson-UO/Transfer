using System.Linq;
using UnityEngine;

public class PsylinkDetection : MonoBehaviour
{
    public Rigidbody rb {  get; private set; }
    PsylinkThrowable pt;
    public bool targetHit {  get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        pt = FindFirstObjectByType<PsylinkThrowable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //logic for stopping the psylink when it hits a psylink interactable object
        if (other.gameObject.CompareTag("Player")) { return; }
        pt.readyToThrow = true;
        if (targetHit)
        {
            return;
        } 
        else
        {
            foreach (var item in pt.activePsylinks.ToList()) //use ToList to avoid modifying while iterating
            {
                if (item.obj == other.gameObject)
                {
                    Destroy(item.psylink); 
                    pt.activePsylinks.Remove(item);
                }
            }

            pt.activePsylinks.Add(new PsylinkAndObject()
            {
                obj = other.gameObject,
                psylink = gameObject
            });
            Debug.Log(pt.activePsylinks.ToString());
            targetHit = true;
        }
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetParent(other.transform);
    }
}
