using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    public PlayerCore playerCore;
    public Camera cam;
    
    // Common
    [Header("Common")]
    public float horizontalBounds;
    public float lerpSpeed = 10f;
    
    // Normal Camera
    [Header("Normal Camera")]
    public float elevation;
    public float maxElevation = 10f;
    public float superJumpOffset = 6f;
    
    // Tunneling Camera
    [Header("Tunneling Camera")]
    public float lookAhead;

    [Header("Game Over Camera")]
    public float gameOverTime;
    public float gameOverCameraSize = 7f;
    public float normalCameraSize;

    public float impactFreezeTime;
    public float freezeTimer;

    public Animator shatterAnim;
    public float shatterDelay;
    public SpriteRenderer shatterBackground;
    
    void Start()
    {
        transform.position = new Vector3(0f, elevation, -10f);
        cam.orthographicSize = normalCameraSize / cam.aspect * 2f;
        gameOverCameraSize = gameOverCameraSize / cam.aspect * 2f;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(horizontalBounds * 2f, 0f, 0f));
    }

    void LandCamera()
    {
        Vector3 clampedPosition;
        
        if (playerCore.playerSM.currentState == playerCore.playerSM.playerAirborne && playerCore.playerSM.playerAirborne.superJump)
        {
            clampedPosition = new Vector3
            (
                Mathf.Clamp(playerCore.transform.position.x, -horizontalBounds, horizontalBounds), 
                Mathf.Clamp(playerCore.transform.position.y + superJumpOffset, elevation, maxElevation),
                -10f
            );
        }
        else
        {
            clampedPosition = new Vector3
            (
                Mathf.Clamp(playerCore.transform.position.x, -horizontalBounds, horizontalBounds), 
                elevation, 
                -10f
            );
        }
        
        transform.position = Vector3.Lerp(transform.position, clampedPosition, Time.deltaTime * lerpSpeed);
    }
    
    void TunnelCamera()
    {
        Vector3 lookAheadPos = playerCore.transform.position + new Vector3(playerCore.velocity.x, playerCore.velocity.y) * lookAhead;
        Vector3 clampedPosition = new Vector3(Mathf.Clamp(lookAheadPos.x, -horizontalBounds, horizontalBounds), lookAheadPos.y, -10f);
        transform.position = Vector3.Lerp(transform.position, clampedPosition, Time.deltaTime * lerpSpeed);
    }

    void DeadCamera()
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(0f, elevation, -10f), Time.deltaTime * lerpSpeed * 0.2f);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, gameOverCameraSize, Time.deltaTime * 2f);
    }

    public void FreezeCamera()
    {
        freezeTimer = impactFreezeTime;
    }
    
    public IEnumerator Shatter()
    {
        shatterBackground.color = new Color(shatterBackground.color.r, shatterBackground.color.g, shatterBackground.color.b, 0f);
        shatterBackground.gameObject.SetActive(true);
        shatterAnim.Play("Enable", 0, 0f);
        yield return new WaitForSeconds(shatterDelay);
        shatterAnim.Play("Shatter", 0, 0f);
        AudioManager.instance.PlaySound("Die");
    }
    
    void Update()
    {
        if (playerCore.stageManager.gameOver)
        {
            gameOverTime -= Time.deltaTime;
            if (gameOverTime <= 0) DeadCamera();
            shatterBackground.color = Color.Lerp
            (
                shatterBackground.color,
                new Color(shatterBackground.color.r, shatterBackground.color.g, shatterBackground.color.b, 1f),
                Time.deltaTime * 10f
            );
        }
        else if (freezeTimer > 0)
        {
            freezeTimer -= Time.deltaTime;
        }
        else
        {
            if (playerCore.playerSM.currentState == playerCore.playerSM.playerTunneling) TunnelCamera();
            else LandCamera();
        }
    }
}
