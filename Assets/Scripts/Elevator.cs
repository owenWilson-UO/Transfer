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
            doorLToPosition = doorL.position;
            doorRToPosition = doorR.position;
            doorL.position = new Vector3(doorL.position.x - 3, doorL.position.y, doorL.position.z);
            doorR.position = new Vector3(doorR.position.x + 3, doorR.position.y, doorR.position.z);
        }
        else
        {
            doorLToPosition = new Vector3(doorL.position.x - 3, doorL.position.y, doorL.position.z);
            doorRToPosition = new Vector3(doorR.position.x + 3, doorR.position.y, doorR.position.z);
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
            doorL.position = Vector3.MoveTowards(doorL.position, doorLToPosition, 2f * Time.deltaTime);
            doorR.position = Vector3.MoveTowards(doorR.position, doorRToPosition, 2f * Time.deltaTime);
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
