using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInputController : MonoBehaviour
{
    [Header("Input Buffers")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float switchingBufferTime = 1.0f;

    private float jumpReleaseBuffer;
    private float switchingBuffer;

    public bool JumpHeld { get; private set; }
    public bool JumpReleaseBuffered => jumpReleaseBuffer > 0f;
    public bool SwitchingBufferElapsed => switchingBuffer < 0f;
    public bool RestartPressed { get; private set; }
    public float Horizontal => Input.GetAxis("Horizontal");

    private void Update()
    {
        if (Input.GetButtonUp("Jump"))
        {
            jumpReleaseBuffer = jumpBufferTime;
        }

        if (jumpReleaseBuffer > 0f)
        {
            jumpReleaseBuffer -= Time.deltaTime;
        }

        switchingBuffer -= Time.deltaTime;
        JumpHeld = Input.GetButton("Jump");
        RestartPressed = Input.GetKeyUp(KeyCode.P);
    }

    public void ConsumeJumpReleaseBuffer()
    {
        jumpReleaseBuffer = 0f;
    }

    public void ResetSwitchingBuffer()
    {
        switchingBuffer = switchingBufferTime;
    }
}
