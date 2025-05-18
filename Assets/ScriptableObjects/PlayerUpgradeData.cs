using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LevelName
{
    Tutorial,
    Level1,
    Level2,
    Level3
}

[CreateAssetMenu(fileName = "PlayerUpgradeData", menuName = "Scriptable Objects/PlayerUpgradeData")]
public class PlayerUpgradeData : ScriptableObject
{
    public int batteries = 0;

    public float maxSlowMotionDuration = 0f;
    public int maxTransferAmount = 0;
    public int maxPsylinkAmount = 0;

    public float sensMultiplier = 1.5f;

    public Dictionary<LevelName, int> batteriesCollectedByLevel;


    private void OnEnable()
    {
        batteries = 0;

        maxSlowMotionDuration = 1f;
        maxTransferAmount = 1;
        maxPsylinkAmount = 1;
        batteriesCollectedByLevel = new Dictionary<LevelName, int>
        {
            {LevelName.Tutorial, 0 },
            {LevelName.Level1, 0 },
            {LevelName.Level2, 0 },
            {LevelName.Level3, 0 },
        };
        Debug.Log("OnEnable");
    }
}

