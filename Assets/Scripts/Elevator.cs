using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform doorL;
    [SerializeField] private Transform doorR;
    [SerializeField] private AudioSource doorLSound;
    [SerializeField] private AudioSource doorRSound;

    [Header("Settings")]
    [SerializeField] bool closed;
    [SerializeField] public bool startDoorMovement;

    private Coroutine openDoorsCoroutine;
    private Vector3 doorLToPosition;
    private Vector3 doorRToPosition;
    private bool move = false;

    private void Start()
    {
        if (!closed)
        {
            doorLToPosition = doorL.localPosition;
            doorRToPosition = doorR.localPosition;
            doorL.localPosition = new Vector3(doorL.localPosition.x - 3, doorL.localPosition.y, doorL.localPosition.z);
            doorR.localPosition = new Vector3(doorR.localPosition.x + 3, doorR.localPosition.y, doorR.localPosition.z);
        }
        else
        {
            doorLToPosition = new Vector3(doorL.localPosition.x - 3, doorL.localPosition.y, doorL.localPosition.z);
            doorRToPosition = new Vector3(doorR.localPosition.x + 3, doorR.localPosition.y, doorR.localPosition.z);
            if (startDoorMovement)
            {
                openDoorsCoroutine = StartCoroutine(MoveDoors());
            }
        }
    }

    private void Update()
    {
        if (move)
        {
            doorL.localPosition = Vector3.MoveTowards(doorL.localPosition, doorLToPosition, 2f * Time.deltaTime);
            doorR.localPosition = Vector3.MoveTowards(doorR.localPosition, doorRToPosition, 2f * Time.deltaTime);
        }
        if (startDoorMovement && !move && openDoorsCoroutine == null)
        {
            openDoorsCoroutine = StartCoroutine(MoveDoors());
        }
    }

    IEnumerator MoveDoors()
    {
        yield return new WaitForSeconds(2f);
        doorLSound.Play();
        doorRSound.Play();
        move = true;
        openDoorsCoroutine = null;
    }
}
