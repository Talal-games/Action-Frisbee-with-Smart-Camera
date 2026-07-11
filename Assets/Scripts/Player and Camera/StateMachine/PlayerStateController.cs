using System.Collections.Generic;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    private readonly Stack<PlayerState> stateStack = new Stack<PlayerState>();

    private void FixedUpdate()
    {
        if (stateStack.Count > 0)
        {
            stateStack.Peek().ExecuteStateLogic();
        }
    }

    public void ChangeState(PlayerState newState)
    {
        PopState();
        PushState(newState);
    }

    public void PushState(PlayerState newState)
    {
        if (stateStack.Count > 0)
        {
            stateStack.Peek().Exit();
        }

        stateStack.Push(newState);
        newState.Enter();
    }

    public void PopState()
    {
        if (stateStack.Count == 0) return;

        stateStack.Pop().Exit();
        if (stateStack.Count > 0)
        {
            stateStack.Peek().Enter();
        }
    }
}
