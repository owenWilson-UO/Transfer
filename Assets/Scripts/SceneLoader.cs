using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
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
        LoadSceneByName("IntroSequence");
    }

    public void LoadLevel1()
    {
        LoadSceneByName("Level1");
    }

    public void LoadLevel2()
    {
        LoadSceneByName("Level2");
    }

    public void LoadLevel3()
    {
        LoadSceneByName("Level3");
    }
}

