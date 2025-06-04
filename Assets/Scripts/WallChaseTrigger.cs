using UnityEngine;

public class WallChaseTrigger : MonoBehaviour
{
    [SerializeField] private ChaseWall ChaseWall;

    private void OnTriggerEnter(Collider other)
    {
        ChaseWall.gameObject.SetActive(true); // Spawn the wall once we hit trigger
        ChaseWall.StartChase();
    }
}

