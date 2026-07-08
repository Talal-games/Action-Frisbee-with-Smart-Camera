using UnityEngine;

public class FlyingState : PlayerState
{
    public FlyingState(PlayerStateController player) : base(player) { }

    public override void Enter()
    {
        Debug.Log("Entered Flying");
        player.Throw();
        player.SaveVel();
        player.ChooseFlyingCamera();
        player.ResetSwitchingBuffer();
        player.SetIsResetted(false);
    }

    public override void ExecuteStateLogic()
    {
        player.TurnManager();
        player.AutoChangeElevation();
        player.FaceVelocity();
        if (player.GetCurrentThrows > 0)
        {
            player.SwitchToThrowingIfPressing();
        }

        player.SlowManager();
    }
}
