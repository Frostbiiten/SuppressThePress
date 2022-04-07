using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public PlayerCore playerCore;
    public bool gameOver;
    
    // Walls
    [Header("Walls")]
    public float stageWidth;
    public Transform leftWall;
    public Transform rightWall;
    
    // Roof
    [Header("Roof")] 
    public float baseFallSpeed;
    public float roofFallRateCoefficient;
    public float maxRoofHeight;
    public float roofHeight;
    public float offsetDecel;
    public float offset;
    public Transform roof;

    // Floor
    [Header("Floor")] public Transform floor;

    public Animator scoreCanvasAnimator;
    public TMP_Text scoreLabel;
    
    public void Start()
    {
        roof.position = new Vector3(0f, roofHeight);
    }

    public void GameOver()
    {
        gameOver = true;
        playerCore.playerSkin.GameOver();
        playerCore.playerSM.ChangeState(playerCore.playerSM.playerDead);
        playerCore.CancelInvoke("UpdateScore");
        scoreCanvasAnimator.Play("Idle", 0, 0f);
        scoreCanvasAnimator.SetBool("GameOver", true);
        Music.instance.music.Stop();

        bool highScore = false;
        if (PlayerPrefs.HasKey("HighScore"))
        {
            if (playerCore.score > PlayerPrefs.GetInt("HighScore"))
            {
                highScore = true;
                PlayerPrefs.SetInt("HighScore", playerCore.score);
                PlayerPrefs.Save();
            }
        }
        else
        {
            highScore = true;
            PlayerPrefs.SetInt("HighScore", playerCore.score);
            PlayerPrefs.Save();
        }

        if (highScore) scoreLabel.text = "New High Score!";
        AudioManager.instance.PlaySound("Crack");
        StartCoroutine(playerCore.playerCam.Shatter());
    }

    public void Retry()
    {
        SceneLoader.instance.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadMenu()
    {
        SceneLoader.instance.LoadScene("Menu");
    }
    
    public void Update()
    {
        if (playerCore.startTime < 0f) return;
        if (gameOver)
        {
            leftWall.position = new Vector3(-stageWidth, playerCore.playerCam.transform.position.y);
            rightWall.position = new Vector3(stageWidth, playerCore.playerCam.transform.position.y);

            if (playerCore.playerCam.freezeTimer < 0f)
            {
                roof.position = Vector3.Lerp(roof.position, Vector3.zero, Time.deltaTime * 3f);
            }
            return;
        }

        float elapsed = Time.timeSinceLevelLoad - playerCore.startTime;
        roofHeight -= baseFallSpeed * Time.deltaTime;
        roofHeight -= roofFallRateCoefficient * elapsed * Time.deltaTime;
        offset = Mathf.Lerp(offset, 0f, Time.deltaTime * offsetDecel); // Or movetowards
        roofHeight += offset * Time.deltaTime;
        if (roofHeight > maxRoofHeight)
        {
            roofHeight = maxRoofHeight;
            offset = 0f;
        }
        
        if (playerCore.playerSM.currentState == playerCore.playerSM.playerGrounded)
        {
            if (roofHeight < playerCore.playerDimensions.y)
            {
                GameOver();
                return;
            }
        }
        else
        {
            if (roofHeight <= 0f)
            {
                GameOver();
                return;
            }
        }
        
        // Walls
        leftWall.position = new Vector3(-stageWidth, playerCore.transform.position.y);
        rightWall.position = new Vector3(stageWidth, playerCore.transform.position.y);
        
        // Roof
        roof.position = new Vector3(0f, roofHeight);
    }
}
