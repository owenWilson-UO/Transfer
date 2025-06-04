using UnityEngine;

public class WallChaseStopTrigger : MonoBehaviour
{
    [SerializeField] private ChaseWall ChaseWall;

    private void OnTriggerEnter(Collider other)
    {
        ChaseWall.StopChase();
        ChaseWall.gameObject.SetActive(false);
    }
}