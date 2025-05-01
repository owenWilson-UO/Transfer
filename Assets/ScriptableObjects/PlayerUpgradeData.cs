using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerUpgradeData", menuName = "Scriptable Objects/PlayerUpgradeData")]
public class PlayerUpgradeData : ScriptableObject
{
    public float maxSlowMotionDuration = 0f;
    public int maxTransferAmount = 0;
    public int maxPsylinkAmount = 0;


    private void OnEnable()
    {
        maxSlowMotionDuration = 0f;
        maxTransferAmount = 0;
        maxPsylinkAmount = 0;
        Debug.Log("OnEnable");
    }
}

