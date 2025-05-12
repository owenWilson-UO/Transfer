using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private TutorialPopupHandler popupHandler;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popupHandler.ShowPopup(name);
        }
    }
}
