public abstract class PlayerState
{
    protected PlayerStateController player;
    public abstract void ExecuteStateLogic();


    public PlayerState(PlayerStateController player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
