using UnityEngine;

public class ThrowingState : PlayerState
{
    public ThrowingState(PlayerStateController player) : base(player) { }

    public override void Enter()
    {
        player.ResetThrowing();
        player.ClearAimTargetForThrowingState();
        Debug.Log("Entered Throwing");
        player.ChooseThrowingCamera();
    }

    public override void ExecuteStateLogic()
    {
        player.RunTargetSelection();
        player.FindThrowDirection();
        player.FindThrowPower();
        player.UpdateThrowChargeAudio();
        player.VisualizeThrowAngle();
        player.TurnManager();
        player.AutoChangeElevation();
        player.FaceVelocity();
        player.SmoothAimAtTarget();
        player.InitiateFly();
    }

    public override void Exit()
    {
        player.SetTurningTargetFromAimTarget();
        player.StopThrowChargeAudio();
    }
}
