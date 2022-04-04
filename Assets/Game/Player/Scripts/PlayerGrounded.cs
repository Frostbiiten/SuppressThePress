using System;
using UnityEngine;

[Serializable]
public class PlayerGrounded : SMState
{
    public PlayerGrounded(PlayerSM playerSm) : base(playerSm) { }
    
    public float baseSpeed = 10f;
    public float maxFloorDistance;
    public float jumpPower;

    public override void Enter(SMState oldState)
    {
        playerSM.playerCore.trailParticles.Stop();
    }

    public override void Update()
    {
        if (playerSM.playerCore.jumpDown)
        {
            playerSM.playerCore.velocity += Vector2.up * jumpPower;
            AudioManager.instance.PlaySound("Jump");
            playerSM.ChangeState(playerSM.playerAirborne);
        }

        if (playerSM.playerCore.directionalInput.y < 0f) playerSM.ChangeState(playerSM.playerTunneling);
    }
    
    public override void FixedUpdate()
    {
        if (playerSM.playerCore.floorDistance < maxFloorDistance)
        {
            playerSM.playerCore.velocity = new Vector2(playerSM.playerCore.directionalInput.x * baseSpeed, playerSM.playerCore.velocity.y);
        }
        else
        {
            playerSM.ChangeState(playerSM.playerAirborne);
        }
    }
}
