using UnityEngine;

public class TransferLockout : MonoBehaviour
{
    [SerializeField] private TransferThrowable tt;
    [SerializeField] private Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Transfer"))
        {
            tt.SetTransfferLockout(true);
            animator.SetTrigger("Lockout");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Transfer"))
        {
            tt.SetTransfferLockout(false);
            animator.ResetTrigger("Lockout");
        }
    }
}
