using UnityEngine;

public class DeadState : PlayerState
{
    public DeadState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        player.ReleaseRigidbodyConstraints();
        Debug.Log("Entered Dead");
        player.ShowFlyingCamera();
    }

    public override void ExecuteStateLogic()
    {
    }
}
