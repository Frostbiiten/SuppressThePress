using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    public PlayerCore playerCore;
    
    public Transform playerSkin;
    public Transform eyes;

    [Header("Eyes")]
    public SpriteRenderer eyesRenderer;
    public Sprite normalEyes;
    public Sprite dashEyes;
    public Sprite deadEyes;
    public float eyesOffset;
    
    [Header("Squash and Stretch")]
    public float velocityStrength;
    public float accelerationStrength;
    public float maxAccelerationDistort;
    
    public float squashSpeed = 5f;
    
    float acceleration;
    float oldVelo;
    
    Vector3 defaultScale;
    Vector3 targetScale;

    void Start()
    {
        defaultScale = transform.localScale;
    }

    public void GameOver()
    {
        eyesRenderer.sprite = deadEyes;
    }
    
    public void StartDash() { eyesRenderer.sprite = dashEyes; }
    public void StopDash() { eyesRenderer.sprite = normalEyes; }
    
    void Update()
    {
        acceleration = playerCore.velocityMagnitude - oldVelo;
        oldVelo = playerCore.velocityMagnitude;
        
        targetScale = defaultScale;
        if (playerCore.velocityMagnitude > 0.01f)
        {
            // Eyes
            Vector3 dir = playerCore.velocity / playerCore.velocityMagnitude;
            eyes.transform.localPosition = Vector3.Lerp(eyes.localPosition, dir * eyesOffset, Time.deltaTime * 5f);
            
            // Squash and stretch
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Vector2.SignedAngle(Vector2.up, playerCore.velocity));
            float strength = (playerCore.velocityMagnitude * velocityStrength + 1f) + Mathf.Clamp(acceleration * accelerationStrength, -maxAccelerationDistort, maxAccelerationDistort);
            targetScale = new Vector3
            ( 
                1f / strength, 
                strength, 
                1f
            );
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * squashSpeed);
        playerSkin.transform.rotation = Quaternion.identity;
    }
}
