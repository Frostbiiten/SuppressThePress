using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    public Animator loadingAnimation;
    public bool loading;
    public GameObject loadingCanvas;
    
    String targetScene;
    
    void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += FinishLoading;
    }
    
    void FinishLoading(Scene scene, LoadSceneMode mode)
    {
        if (loadingAnimation) loadingAnimation.Play("Finish");
        if (Music.instance && !Music.instance.music.isPlaying) Music.instance.music.Play(); // Make sure music is playing when scene loads
        loading = false;
    }
    
    public void LoadScene(String name)
    {
        targetScene = name;
        loadingAnimation.Play("Start");
        loading = true;
    }

    public void LoadReal()
    {
        SceneManager.LoadScene(targetScene);
    }

    void Update()
    {
        if (loading)
        {
            var stateInfo = loadingAnimation.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.normalizedTime >= 1f && stateInfo.IsName("Start")) LoadReal();
        }
    }
}
