using System.Collections;
using UnityEngine;

public class CloseSpawnRoom : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Elevator elevator;
    [SerializeField] public Transform roofToWall;
    [SerializeField] public BoxCollider wall;
    [SerializeField] private float rotationDuration = 1f;

    private Coroutine rotateCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            elevator.startDoorMovement = true;
            wall.isTrigger = false;
            StartZRotation();
        }
    }

    public void StartZRotation()
    {
        if (rotateCoroutine != null) StopCoroutine(rotateCoroutine);
        rotateCoroutine = StartCoroutine(RotateZAxis(roofToWall, 0f, rotationDuration));
    }

    private IEnumerator RotateZAxis(Transform target, float targetZ, float duration)
    {
        Quaternion startRotation = target.rotation;
        Quaternion endRotation = Quaternion.Euler(target.eulerAngles.x, target.eulerAngles.y, targetZ);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            target.rotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
            yield return null;
        }

        target.rotation = endRotation; 
    }
}
