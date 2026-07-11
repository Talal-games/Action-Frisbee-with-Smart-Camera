using UnityEngine;

public class FlyingState : PlayerState
{
    public FlyingState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered Flying");
        player.ThrowFrisbee();
        player.ShowFlyingCamera();
        player.ResetThrowingSwitchBuffer();
        player.SetSafeResetHandled(false);
    }

    public override void ExecuteStateLogic()
    {
        player.UpdateFlightTurn();
        player.UpdateFlightElevationTowardTarget();
        player.FaceMovementDirection();
        if (player.CurrentThrows > 0)
        {
            player.EnterThrowingStateIfJumpHeld();
        }

        player.HandleLowSpeedStateTransition();
    }
}
