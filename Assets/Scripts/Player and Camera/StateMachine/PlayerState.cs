public abstract class PlayerState
{
    protected PlayerController player;
    public abstract void ExecuteStateLogic();


    public PlayerState(PlayerController player)
    {
        this.player = player;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
