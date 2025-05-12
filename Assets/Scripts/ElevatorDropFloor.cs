using System.Collections;
using UnityEngine;

public class ElevatorDropFloor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Elevator dropElevator;
    [SerializeField] private Transform floorLeft;
    [SerializeField] private Transform floorRight;
    [SerializeField] private Collider ground;

    [Header("Settings")]
    [SerializeField] private float duration;

    private Vector3 left_I;
    private Vector3 right_I;
    private Vector3 leftTo;
    private Vector3 rightTo;

    private void Start()
    {
        left_I = floorLeft.position;
        right_I = floorRight.position;

        leftTo = new Vector3(left_I.x - 2.95f, left_I.y, left_I.z);
        rightTo = new Vector3(right_I.x + 2.95f, right_I.y, right_I.z);
    }

    public void StartMove()
    {
        StartCoroutine(MoveFloor());
    }

    IEnumerator MoveFloor()
    {
        yield return new WaitForSeconds(5f);

        ground.gameObject.SetActive(false);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            floorLeft.position = Vector3.Lerp(left_I, leftTo, elapsed/duration);
            floorRight.position = Vector3.Lerp(right_I, rightTo, elapsed/duration);
            yield return null;
        }

        floorLeft.position = leftTo;
        floorRight.position = rightTo;

        yield return new WaitForSeconds(1f);

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            floorLeft.position = Vector3.Lerp(leftTo, left_I, elapsed / duration);
            floorRight.position = Vector3.Lerp(rightTo, right_I, elapsed / duration);
            yield return null;
        }

        floorLeft.position = left_I;
        floorRight.position = right_I;
    }

}
