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
        Time.timeScale = 1f;
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("IntroSequence");
    }

    public void LoadLevel1()
    {
        Time.timeScale = 1f;
        playerUpgradeData.maxTransferAmount = 1;
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("Level1");
    }

    public void LoadLevel2()
    {
        Time.timeScale = 1f;
        playerUpgradeData.maxTransferAmount = 1;
        playerUpgradeData.maxSlowMotionDuration = 1f;
        LoadSceneByName("Level2");
    }

    public void LoadLevel3()
    {
        Time.timeScale = 1f;
        LoadSceneByName("Level3");
    }
}

