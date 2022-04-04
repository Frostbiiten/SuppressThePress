using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

[Serializable] 
public class SMState
{
    [NonSerialized] public PlayerSM playerSM;

    public SMState (PlayerSM psm)
    {
        playerSM = psm;
    }
    
    public virtual void Awake () { }
    public virtual void Start () { }
    public virtual void Enter (SMState oldState) { }
    public virtual void Update () { }
    public virtual void FixedUpdate () { }
    public virtual void Exit () { }
}


[Serializable]
public class PlayerDead : SMState
{
    public PlayerDead(PlayerSM playerSm) : base(playerSm) {}

    public override void Enter(SMState oldState)
    {
        if (oldState == playerSM.playerGrounded) playerSM.playerCore.velocity = Vector2.zero;
        playerSM.playerCore.playerCollider.enabled = false;
    }

    public override void FixedUpdate()
    {
        Vector2 pos = playerSM.playerCore.transform.position;
        playerSM.playerCore.velocity = Vector2.Lerp(playerSM.playerCore.velocity, Vector2.zero, 0.1f);
        
        // Flip velo if hit wall
        if ((playerSM.playerCore.velocity.x < 0f && pos.x - playerSM.playerCore.playerDimensions.x / 2f <= -playerSM.playerCore.stageManager.stageWidth) ||
            (playerSM.playerCore.velocity.x > 0f && pos.x + playerSM.playerCore.playerDimensions.x / 2f >= playerSM.playerCore.stageManager.stageWidth))
        {
            playerSM.playerCore.velocity = new Vector2(-playerSM.playerCore.velocity.x, playerSM.playerCore.velocity.y);
        }

        // Flip velo if hit top
        if (playerSM.playerCore.transform.position.y + playerSM.playerCore.playerDimensions.y / 2f >
            playerSM.playerCore.stageManager.roofHeight)
        {
            playerSM.playerCore.velocity = new Vector2(playerSM.playerCore.velocity.x, -playerSM.playerCore.velocity.y);
        }
    }
}

[Serializable]
public class PlayerSM
{
    public PlayerCore playerCore;

    [HideInInspector]
    public SMState currentState;

    public PlayerGrounded playerGrounded;
    public PlayerAirborne playerAirborne;
    public PlayerTunneling playerTunneling;
    public PlayerDead playerDead;

    public PlayerSM(PlayerCore playerCore)
    {
        this.playerCore = playerCore;
        playerGrounded = new PlayerGrounded(this);
        playerAirborne = new PlayerAirborne(this);
        playerTunneling = new PlayerTunneling(this);
        playerDead = new PlayerDead(this);
    }

    public void Start()
    {
        playerGrounded.Start();
        playerAirborne.Start();
        playerTunneling.Start();
        playerDead.Start();
        
        currentState = playerAirborne;
        currentState.Enter(null);
    }
    public void Update()
    {
        currentState.Update();
    }
    public void FixedUpdate()
    {
        currentState.FixedUpdate();
    }
    public void ChangeState(SMState newState)
    {
        currentState.Exit();
        SMState oldState = currentState;
        currentState = newState;
        currentState.Enter(oldState);
    }
}

public class PlayerCore : MonoBehaviour
{
    // State machine
    public PlayerSM playerSM = new(null);
    
    // Camera
    public PlayerCamera playerCam;
    
    // Stage
    public StageManager stageManager;
    
    // Input
    [Header("Input")]
    public InputActionAsset playerInputAsset;
    InputAction directionalInputAction;
    public Vector2 directionalInput;
    InputAction jumpInputAction;
    [HideInInspector] public bool jumpDown;
    InputAction superJumpInputAction;
    [HideInInspector] public bool superJumpDown;
    
    // Collider (?)
    [Header("Collider")]
    public Vector2 playerDimensions;
    public BoxCollider2D playerCollider;
    
    // Physics
    [Header("Physics")]
    public Rigidbody2D rb;
    public float velocityMagnitude { get; private set; }
    public Vector2 velocity 
    {
        get => rb.velocity;
        set
        {
            rb.velocity = value;
            velocityMagnitude = value.magnitude;
        }
    }
    
    // Floor detect
    [Header("Floor Detection")]
    public bool floorDetected;
    public float floorDistance;
    
    // Raycast
    [Header("Raycasting")]
    public RaycastHit2D[] hitResults = new RaycastHit2D[5];
    public LayerMask floorMask;
    int floorMaskRaw;
    public float raycastMaxDistance;
    
    // UI
    [Header("UI")] 
    public RectTransform indicator;
    public Image indicatorBackground;
    public Image indicatorFillbar;
    public RectTransform powerBar;
    public Image powerBarBackground;
    public Image powerFillBar;
    public TMP_Text scoreText;
    public RectTransform depthIndicator;
    public RectTransform canvasRef;
    public TMP_Text depthText;
    public Image depthArrow;
    public float indicatorDepth = -10f;
    
    // Game Mechanics
    [Header("Game mechanics")]
    public float startTime = -1f;
    public float knockbackPower;
    public int score;
    public int maxPower;
    public int power;
    
    // VFX
    public PlayerSkin playerSkin;
    public ParticleSystem trailParticles;
    public ParticleSystem impactParticles;
    public ParticleSystem largeImpact;
    
    void UpdateScore() { ++score; scoreText.text = score.ToString("0000"); }
    
    public void RealStart()
    {
        playerSM.playerCore.startTime = Time.timeSinceLevelLoad;
        InvokeRepeating("UpdateScore", 0f, 1f);
    }
    
    public void Start()
    {
        directionalInputAction = playerInputAsset.FindAction("directionalInput");
        superJumpInputAction = playerInputAsset.FindAction("superJump");
        jumpInputAction = playerInputAsset.FindAction("jump");
        playerCollider.size = playerDimensions;
        floorMaskRaw = floorMask.value;
        playerSM.Start();
    }

    public void UpdateInput()
    {
        directionalInput = directionalInputAction.ReadValue<Vector2>();
        superJumpDown = superJumpInputAction.WasPressedThisFrame();
        jumpDown = jumpInputAction.WasPressedThisFrame();
    }

    public void UpdateUI()
    {
        if (playerSM.currentState == playerSM.playerDead)
        {
            indicatorBackground.color = Color.Lerp(indicatorBackground.color, new Color(
               indicatorBackground.color.r, indicatorBackground.color.g, indicatorBackground.color.b, 0f),
              Time.deltaTime * 4f
            );
            
            indicatorFillbar.color = Color.Lerp(indicatorFillbar.color, new Color(
               indicatorFillbar.color.r, indicatorFillbar.color.g, indicatorFillbar.color.b, 0f),
              Time.deltaTime * 4f
            );
            
            powerBarBackground.color = Color.Lerp(powerBarBackground.color, new Color(
               powerBarBackground.color.r, powerBarBackground.color.g, powerBarBackground.color.b, 0f),
              Time.deltaTime * 4f
            );
            
            powerFillBar.color = Color.Lerp(powerFillBar.color, new Color(
               powerFillBar.color.r, powerFillBar.color.g, powerFillBar.color.b, 0f),
              Time.deltaTime * 4f
            );
        }
        
        if (playerSM.currentState == playerSM.playerTunneling)
        {
            indicator.pivot = Vector2.Lerp(indicator.pivot, new Vector2(1f, 1f), Time.deltaTime * 5f);
            powerBar.anchoredPosition = Vector2.Lerp(powerBar.anchoredPosition, new Vector2(-indicator.sizeDelta.x - 10f, 0f), Time.deltaTime * 10f);
        }
        else
        {
            indicator.pivot = Vector2.Lerp(indicator.pivot, new Vector2(1f, -0.5f), Time.deltaTime * 5f);
            powerBar.anchoredPosition = Vector2.Lerp(powerBar.anchoredPosition,Vector2.zero, Time.deltaTime * 10f);
        }
        
        indicatorFillbar.fillAmount = 1f - (stageManager.roofHeight / stageManager.maxRoofHeight);
        powerFillBar.fillAmount = Mathf.Lerp(powerFillBar.fillAmount, (float) power / maxPower, Time.deltaTime * 5f);


        
        var screenPos = playerCam.cam.WorldToScreenPoint(new Vector3(-stageManager.stageWidth, 0f));
        depthIndicator.anchoredPosition = new Vector2
        (
            Mathf.Clamp(screenPos.x * (canvasRef.rect.width / Screen.width), 10f, Screen.width),
            -10f
        );
        
        depthText.text = (-transform.position.y).ToString("000 m");
        
        if (transform.position.y < indicatorDepth && !stageManager.gameOver)
            depthArrow.color = Color.Lerp(depthArrow.color, new Color(1f, 1f, 1f, 1f), Time.deltaTime * 5f);
        else
            depthArrow.color = Color.Lerp(depthArrow.color, new Color(1f, 1f, 1f, 0f), Time.deltaTime * 5f);
        
        depthText.color = depthArrow.color;
    }
    
    public void Update()
    {
        UpdateInput();
        
        if (power > 0 && superJumpDown && !playerSM.playerAirborne.superJump)
        {
            if (playerSM.currentState == playerSM.playerGrounded) playerSM.ChangeState(playerSM.playerAirborne);
            if (playerSM.currentState == playerSM.playerAirborne)
            {
                playerSM.playerAirborne.superJump = true;
                playerSM.playerTunneling.dashParticles.transform.position = transform.position;
                playerSM.playerTunneling.dashParticles.Play();
                
                // Enable particles
                playerSM.playerCore.trailParticles.Play();
                var s = playerSM.playerCore.trailParticles.main;
                s.startColor = new ParticleSystem.MinMaxGradient(Palettes.instance.colors.d);
                
                // Set dash face
                playerSM.playerCore.playerSkin.StartDash();
                AudioManager.instance.PlaySound("Dash");
            }
        }
        
        playerSM.Update();
        UpdateUI();
    }

    public void FloorCheck()
    {
        Debug.DrawRay(transform.position, Vector2.down * raycastMaxDistance, Color.blue, 0.02f);
        floorDetected = Physics2D.RaycastNonAlloc(transform.position, Vector2.down, hitResults, raycastMaxDistance, floorMaskRaw) > 0;
        floorDistance = floorDetected ? hitResults[0].distance - playerDimensions.y / 2f : raycastMaxDistance;
    }

    public void PushRoof()
    {
        if (power > 0)
        {
            playerCam.FreezeCamera();
            impactParticles.transform.position = transform.position + Vector3.up * playerDimensions.y / 2f;
            impactParticles.Play();


            if (power > maxPower / 2)
            {
                AudioManager.instance.PlaySound("HighImpact");
                largeImpact.transform.position = impactParticles.transform.position;
                largeImpact.Play();
            }
            else if (power <= 2) AudioManager.instance.PlaySound("SmallImpact");
            else AudioManager.instance.PlaySound("MediumImpact");
        }
        stageManager.offset += power * knockbackPower;
        power = 0;
    }
    
    public void FixedUpdate()
    {
        velocityMagnitude = velocity.magnitude;
        FloorCheck();
        playerSM.FixedUpdate();
    }
}
