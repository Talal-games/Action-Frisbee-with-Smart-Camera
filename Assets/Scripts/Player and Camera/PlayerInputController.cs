using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInputController : MonoBehaviour
{
    [Header("Input Buffers")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float switchingBufferTime = 1.0f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorWhileFocused = true;

    private float jumpReleaseBuffer;
    private float switchingBuffer;

    public bool JumpHeld { get; private set; }
    public bool JumpReleaseBuffered => jumpReleaseBuffer > 0f;
    public bool SwitchingBufferElapsed => switchingBuffer < 0f;
    public bool RestartPressed { get; private set; }
    public float Horizontal => Input.GetAxis("Horizontal");

    private void Awake()
    {
        SetCursorForFocus(Application.isFocused);
    }

    private void Update()
    {
        SetCursorForFocus(Application.isFocused);

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

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorForFocus(hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SetCursorForFocus(!pauseStatus && Application.isFocused);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetCursorForFocus(bool hasFocus)
    {
        if (!lockCursorWhileFocused)
        {
            return;
        }

        Cursor.lockState = hasFocus ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !hasFocus;
    }
}
