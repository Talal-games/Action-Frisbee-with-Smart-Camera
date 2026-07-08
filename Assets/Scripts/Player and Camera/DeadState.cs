using UnityEngine;

public class DeadState : PlayerState
{
    public DeadState(PlayerStateController player) : base(player) { }

    public override void Enter()
    {
        player.RemoveRbConstraints();
        Debug.Log("Entered Dead");
        player.ChooseFlyingCamera();
    }

    public override void ExecuteStateLogic()
    {
    }
}
