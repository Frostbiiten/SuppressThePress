using System;
using UnityEditor.Rendering;
using UnityEngine;

[Serializable]
public class PlayerAirborne : SMState
{
    public Vector2 gravity;
    public float moveSpeed;
    public float groundedMinDistance = 0.1f;
    public float drag;
    
    [Header("Superjump")]
    public bool superJump;
    public Vector2 superJumpVelocity;
    
    public PlayerAirborne(PlayerSM playerSm) : base(playerSm) { }

    public override void Enter(SMState oldState)
    {
        playerSM.playerCore.trailParticles.Stop();
    }

    public void NormalPhysics()
    {
        if (playerSM.playerCore.floorDistance < groundedMinDistance)
        {
            playerSM.ChangeState(playerSM.playerGrounded);
        }
        else
        {
            playerSM.playerCore.velocity += gravity;
            playerSM.playerCore.velocity = new Vector2(playerSM.playerCore.velocity.x + (playerSM.playerCore.directionalInput.x * moveSpeed), playerSM.playerCore.velocity.y);
            playerSM.playerCore.velocity -= new Vector2(playerSM.playerCore.velocity.x * drag, 0f);
        }
    }

    public void SuperJumpPhysics()
    {
        if (playerSM.playerCore.transform.position.y + playerSM.playerCore.playerDimensions.y / 2f < playerSM.playerCore.stageManager.roofHeight - 0.1f)
        {
            playerSM.playerCore.velocity = superJumpVelocity;
        }
        else
        {
            playerSM.playerCore.playerSkin.StopDash();
            playerSM.playerCore.trailParticles.Stop();
            playerSM.playerCore.PushRoof();
            superJump = false;
        }
    }
    
    public override void FixedUpdate()
    {
        if (superJump) SuperJumpPhysics();
        else NormalPhysics();
    }
}
