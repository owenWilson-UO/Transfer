using UnityEngine;

public class TransferLockout : MonoBehaviour
{
    [SerializeField] private TransferThrowable tt;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Transfer"))
        {
            tt.SetTransfferLockout(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Transfer"))
        {
            tt.SetTransfferLockout(false);
        }
    }
}
