using System;
using UnityEngine;

[Serializable]
public class PlayerTunneling : SMState
{
    public PlayerTunneling(PlayerSM playerSm) : base(playerSm) { }
    
    public Vector2 entranceSpeed;
    public float currentAngle;
    
    public float minSpeed;
    public float currentSpeed;
    public float targetSpeed;
    public float turnSpeed;
    public float exitForce;

    public Collider2D[] detectedCollectibles = new Collider2D[5];
    public LayerMask collectibleLayerMask;
    public int collectiblePower;
    int collectibleLayerMaskRaw;

    public float superDashSpeed = 5f;
    public ParticleSystem emergeParticles;
    public ParticleSystem dashParticles;
    public AudioSource diggingAudioSource;
    public float diggingVolume = 0.3f;
    
    public override void Enter(SMState oldState)
    {
        // "Start" if hadn't started already
        if (playerSM.playerCore.startTime < 0f) playerSM.playerCore.RealStart();
        
        // Disable collider
        playerSM.playerCore.playerCollider.enabled = false;
        
        // Movement
        playerSM.playerCore.velocity = new Vector2(playerSM.playerCore.velocity.x * entranceSpeed.x, entranceSpeed.y);
        currentSpeed = playerSM.playerCore.velocityMagnitude;
        Vector2 dir = playerSM.playerCore.velocity / currentSpeed;
        currentAngle = Vector2.SignedAngle(dir, Vector2.up);
        
        // Enable particles
        playerSM.playerCore.trailParticles.Play();
        var s = playerSM.playerCore.trailParticles.main;
        s.startColor = new ParticleSystem.MinMaxGradient(Palettes.instance.colors.a);
        
        // Start audio
        diggingAudioSource.Play();
        AudioManager.instance.PlaySound("Submerge");
    }

    public override void Start()
    {
        collectibleLayerMaskRaw = collectibleLayerMask.value;
        var m = emergeParticles.main;
        m.startColor = Palettes.instance.colors.d;
    }

    public override void Update()
    {
        // Angle
        if (playerSM.playerCore.directionalInput.x != 0 ||
            playerSM.playerCore.directionalInput.y != 0)
        {
            float inputAngle = Vector2.SignedAngle(playerSM.playerCore.directionalInput.normalized, Vector2.up);
            currentAngle = Mathf.LerpAngle(currentAngle, inputAngle, Time.deltaTime * turnSpeed);
        }
        
        // Dash
        if (playerSM.playerCore.superJumpDown && playerSM.playerCore.power > 0)
        {
            currentSpeed += superDashSpeed;
            --playerSM.playerCore.power;
            dashParticles.transform.position = playerSM.playerCore.transform.position;
            dashParticles.Play();
            AudioManager.instance.PlaySound("Woosh");
        }

        // Speed
        if (currentSpeed < minSpeed) currentSpeed = minSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 2f);
        
        diggingAudioSource.volume = Mathf.Lerp(diggingAudioSource.volume, diggingVolume, Time.deltaTime * 2f);
    }

    void ProcessMovement()
    {
        if (playerSM.playerCore.velocity.y > 0f)
        {
            // Fully over floor
            if (playerSM.playerCore.transform.position.y - playerSM.playerCore.playerDimensions.y / 2f > playerSM.playerCore.stageManager.floor.position.y)
            {
                playerSM.playerCore.velocity = new Vector2(playerSM.playerCore.velocity.x,exitForce);
                playerSM.ChangeState(playerSM.playerAirborne);
                emergeParticles.transform.position = new Vector2(playerSM.playerCore.transform.position.x, playerSM.playerCore.stageManager.floor.position.y);
                emergeParticles.Play();
                AudioManager.instance.PlaySound("Emerge");
                return;
            }

            // If top of player is in roof
            if (playerSM.playerCore.transform.position.y + playerSM.playerCore.playerDimensions.y / 2f >
                playerSM.playerCore.stageManager.roofHeight)
            {
                playerSM.playerCore.PushRoof();
                currentAngle = 180f - currentAngle;
            }
        }
        
        Vector2 pos = playerSM.playerCore.transform.position;
        if ((playerSM.playerCore.velocity.x < 0f && pos.x - playerSM.playerCore.playerDimensions.x / 2f <= -playerSM.playerCore.stageManager.stageWidth) ||
            (playerSM.playerCore.velocity.x > 0f && pos.x + playerSM.playerCore.playerDimensions.x / 2f >= playerSM.playerCore.stageManager.stageWidth))
        {
            currentAngle = -currentAngle;
            AudioManager.instance.PlaySound("SideHit");
        }
        
        playerSM.playerCore.velocity = new Vector2(Mathf.Sin(Mathf.Deg2Rad * currentAngle), Mathf.Cos(Mathf.Deg2Rad * currentAngle)) * currentSpeed;
    }
    
    void CollectibleQuery()
    {
        int detected = Physics2D.OverlapBoxNonAlloc(playerSM.playerCore.transform.position, playerSM.playerCore.playerDimensions, 0f,
            detectedCollectibles, collectibleLayerMaskRaw);

        for (int i = 0; i < detected; ++i)
        {
            detectedCollectibles[i].gameObject.SetActive(false);
            playerSM.playerCore.power += collectiblePower;
            AudioManager.instance.PlaySound("Collect");
        }
    }
    
    public override void FixedUpdate()
    {
        ProcessMovement();
        CollectibleQuery();
    }

    public override void Exit()
    {
        playerSM.playerCore.playerCollider.enabled = true;
        diggingAudioSource.Stop();
        diggingAudioSource.volume = 0f;
    }
}
