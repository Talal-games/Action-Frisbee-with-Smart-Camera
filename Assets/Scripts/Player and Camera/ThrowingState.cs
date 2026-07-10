using UnityEngine;

public class ThrowingState : PlayerState
{
    public ThrowingState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.ResetThrowDirectionToForward();
        player.ClearAimTargetForThrowing();
        Debug.Log("Entered Throwing");
        player.ShowThrowingCamera();
    }

    public override void ExecuteStateLogic()
    {
        player.UpdateAimTargetSelection();
        player.UpdateAimedThrowDirection();
        player.ChargeThrowPowerFromInput();
        player.UpdateThrowChargeFeedback();
        player.UpdateThrowAimVisuals();
        player.UpdateFlightTurn();
        player.UpdateFlightElevationTowardTarget();
        player.FaceMovementDirection();
        player.RotateTowardTurningTargetWhenSlow();
        player.EnterFlyingStateIfThrowReleased();
    }

    public override void Exit()
    {
        player.SetTurningTargetFromAimTarget();
        player.StopThrowChargeSound();
    }
}
