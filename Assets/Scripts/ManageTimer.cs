using UnityEngine;

public class ManageTimer : MonoBehaviour
{
    [SerializeField] private Timer timer;
    [SerializeField] private bool start;

    [Header("Finish")]
    [SerializeField] private Elevator finishElevator;
    [SerializeField] private EndScreen endScreen;
    [SerializeField] private Collider doorWall;

    [Header("Close Door Only")]
    [SerializeField] private bool closeDoorOnly;
    [SerializeField] private ElevatorDropFloor drop;

    private void OnTriggerEnter(Collider other)
    {
        if (closeDoorOnly)
        {
            finishElevator.startDoorMovement = true;
            doorWall.isTrigger = false;
            drop.StartMove();
        }
        else
        {
            if (start)
            {
                timer.StartTimer();
            }
            else
            {
                timer.StopTimer();
                finishElevator.startDoorMovement = true;
                doorWall.isTrigger = false;
                endScreen.StartEndScreen();
            }
        }
    }
}
