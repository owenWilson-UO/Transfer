using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerUpgradeData", menuName = "Scriptable Objects/PlayerUpgradeData")]
public class PlayerUpgradeData : ScriptableObject
{
    public float maxSlowMotionDuration = 1f;
    public int maxTransferAmount = 1;


    private void OnEnable()
    {
        maxSlowMotionDuration = 1f;
        maxTransferAmount = 1;
        Debug.Log("OnEnable");
    }
}

