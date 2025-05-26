using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] PlayerUpgradeData playerUpgradeData;
    private void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void LoadMainMenu()
    {
        LoadSceneByName("MainMenu");
    }

    public void LoadTutorial()
    {
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("IntroSequence");
    }

    public void LoadLevel1()
    {
        playerUpgradeData.maxTransferAmount = 1;
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("Level1");
    }

    public void LoadLevel2()
    {
        playerUpgradeData.maxTransferAmount = 1;
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("Level2");
    }

    public void LoadLevel3()
    {
        LoadSceneByName("Level3");
    }
}

