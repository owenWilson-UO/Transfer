using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Transform cameraPosition;
    void Update()
    {
        //Having this script as well as a camera holder gets rid of screen tearing
        transform.position = cameraPosition.position;
    }
}
