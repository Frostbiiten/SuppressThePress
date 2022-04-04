using System;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public String gameSceneName = "Main";
    public String tutorialSceneName = "Tutorial";
    
    public void LoadGame()
    {
        if (PlayerPrefs.HasKey("Played"))
        {
            SceneLoader.instance.LoadScene(gameSceneName);
        }
        else
        {
            PlayerPrefs.SetInt("Played", 1);
            PlayerPrefs.Save();
        }
    }
    
    public void LoadTutorial()
    { 
        SceneLoader.instance.LoadScene(tutorialSceneName);
    }
}
