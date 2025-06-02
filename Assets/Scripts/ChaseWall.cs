using UnityEngine;

public class ChaseWall : MonoBehaviour
{
    [SerializeField] private Transform target; // usually the player
    [SerializeField] private float speed = 10.0f;
    [SerializeField] private bool isChasing = false;

    void Update()
    {
        if (!isChasing) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // keep it flat
        transform.position += direction * speed * Time.deltaTime;
    }

    public void StartChase()
    {
        isChasing = true;
    }

    public void StopChase()
    {
        isChasing = false;
    }
}
