using UnityEngine;

public class Music : MonoBehaviour
{
    public static Music instance;
    public AudioSource music;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
}
