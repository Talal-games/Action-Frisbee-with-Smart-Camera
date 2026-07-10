using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerStateController))]
[RequireComponent(typeof(PlayerInputController))]
[RequireComponent(typeof(FrisbeeMovementController))]
[RequireComponent(typeof(TargetSelector))]
[RequireComponent(typeof(PlayerFeedback))]
public class PlayerController : MonoBehaviour
{
    private PlayerStateController stateController;
    private PlayerInputController inputController;
    private TargetSelector targetSelector;
    private FrisbeeVisuals frisbeeVisuals;
    private FrisbeeMovementController movementController;
    private PlayerFeedback playerFeedback;

    [Header("Global Player Settings")]
    [SerializeField] private int startingThrows = 2;
    [SerializeField] private int throwsAddedPerTargetHit = 1;
    [SerializeField] private float collisionThreshold = 0.3f;
    [SerializeField, ReadOnly] private int currentThrows;

    [FormerlySerializedAs("isAimingClearsTarget")]
    [SerializeField] private bool isAimingClearTarget = false;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask targetHitLayers = 1 << 6;
    [SerializeField] private LayerMask lethalObstacleLayers = 1 << 7;

    [Header("Cameras")]
    [SerializeField] private PlayerCameraController playerCameraController;

    [Header("UI (Optional)")]
    [SerializeField] private PlayerStatsUI statsUI;

    private Rigidbody rb;
    private TargetCollisionHandler lastCollided;
    private Coroutine collisionTimerCoroutine;

    public int CurrentThrows => currentThrows;

    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        stateController = GetComponent<PlayerStateController>();
        inputController = GetComponent<PlayerInputController>();
        movementController = GetComponent<FrisbeeMovementController>();
        targetSelector = GetComponent<TargetSelector>();
        playerFeedback = GetComponent<PlayerFeedback>();
        if (playerCameraController == null) playerCameraController = GetComponent<PlayerCameraController>();
        if (statsUI == null) statsUI = Object.FindFirstObjectByType<PlayerStatsUI>();
        frisbeeVisuals = GetComponentInChildren<FrisbeeVisuals>();

        currentThrows = startingThrows;
        UpdateThrowCountUI();
        EnterThrowingState();
    }

    private void Update()
    {
        frisbeeVisuals.SetThrowPower(movementController.CurrentThrowPower);
        playerFeedback.UpdateThrowChargeFeedback(inputController.JumpHeld && movementController.CurrentThrowPower > 0f, movementController.CurrentThrowPower, movementController.MaxThrowPower, rb.transform, rb);
        frisbeeVisuals.SpinFrisbee();
        frisbeeVisuals.TiltFrisbee();
        playerFeedback.UpdateFlyingSound(rb);

        if (inputController.RestartPressed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        playerCameraController?.PrepareThirdPersonCamera(rb);
    }

    public void EnterThrowingState()
    {
        stateController.ChangeState(new ThrowingState(this));
    }

    public void EnterFlyingState()
    {
        stateController.ChangeState(new FlyingState(this));
    }

    public void EnterDeadState()
    {
        stateController.ChangeState(new DeadState(this));
    }

    public void SetTurningTargetFromAimTarget()
    {
        targetSelector.SetTurningTargetFromAimTarget();
    }

    public void ClearAimTargetForThrowing()
    {
        targetSelector.ClearAimTargetForThrowing(isAimingClearTarget);
    }

    public void PlayThrowSound()
    {
        playerFeedback.PlayThrowSound(rb.transform.position);
    }

    public void UpdateThrowChargeFeedback()
    {
        bool isCharging = inputController.JumpHeld && movementController.CurrentThrowPower > 0f;
        playerFeedback.UpdateThrowChargeFeedback(isCharging, movementController.CurrentThrowPower, movementController.MaxThrowPower, rb.transform, rb);
    }

    public void StopThrowChargeSound()
    {
        playerFeedback.StopThrowChargeSound();
    }

    public void ThrowFrisbee()
    {
        PlayThrowSound();
        ReduceThrows();
        movementController.ApplyThrowImpulse();
    }

    public void UpdateAimTargetSelection()
    {
        targetSelector.UpdateAimTargetSelection();
    }

    public void UpdateAimedThrowDirection()
    {
        movementController.UpdateAimedThrowDirection(inputController.Horizontal, targetSelector.AimTarget);
    }

    public void ChargeThrowPowerFromInput()
    {
        movementController.ChargeThrowPowerIfHeld(inputController.JumpHeld);
    }

    public void EnterFlyingStateIfThrowReleased()
    {
        if (movementController.CurrentThrowPower > movementController.MinThrowPower && inputController.JumpReleaseBuffered)
        {
            EnterFlyingState();
            movementController.ResetThrowPower();
            inputController.ConsumeJumpReleaseBuffer();
        }
        else if (inputController.JumpReleaseBuffered)
        {
            movementController.ResetThrowPower();
            inputController.ConsumeJumpReleaseBuffer();
        }
    }

    public void EnterThrowingStateIfJumpHeld()
    {
        if (inputController.JumpHeld)
        {
            Debug.Log("Switching to throwing state due to press");
            EnterThrowingState();
        }
    }

    public void ResetThrowingSwitchBuffer()
    {
        inputController.ResetSwitchingBuffer();
    }

    public void HandleLowSpeedStateTransition()
    {
        if (movementController.IsMovingSlowEnoughToReset() && currentThrows < 1)
        {
            EnterDeadState();
            return;
        }

        if (movementController.IsMovingSlowEnoughToReset() && inputController.SwitchingBufferElapsed)
        {
            Debug.Log("Switching to throwing state because very slow");

            EnterThrowingState();
            movementController.TryStartSafePositionReset(targetSelector.TurningTarget);
        }
    }

    public void SetSafeResetHandled(bool isHandled)
    {
        movementController.IsResetted = isHandled;
    }

    public void RotateTowardTurningTargetWhenSlow()
    {
        movementController.RotateTowardTurningTargetWhenSlow(targetSelector.TurningTarget);
    }

    public void UpdateThrowAimVisuals()
    {
        movementController.UpdateThrowAimVisuals();
    }

    public void StopRigidbodyMotion()
    {
        movementController.StopRigidbodyMotion();
    }

    public void ResetThrowDirectionToForward()
    {
        movementController.ResetThrowDirectionToForward();
    }

    public void UpdateFlightTurn()
    {
        movementController.UpdateFlightTurn(inputController.Horizontal, targetSelector.TurningTarget);
    }

    public void FaceMovementDirection()
    {
        movementController.FaceMovementDirection();
    }

    public void UpdateFlightElevationTowardTarget()
    {
        movementController.UpdateFlightElevationTowardTarget(targetSelector.TurningTarget);
    }

    public Transform GetFrisbeeRigidbodyTransform()
    {
        return movementController.RigidbodyTransform;
    }

    public void ReleaseRigidbodyConstraints()
    {
        movementController.ReleaseRigidbodyConstraints();
    }

    public void HandleFrisbeeCollision(GameObject collidedObject)
    {
        var currentTarget = collidedObject.GetComponentInParent<TargetCollisionHandler>();

        if (lastCollided == currentTarget && collisionTimerCoroutine != null) return;

        collisionTimerCoroutine = StartCoroutine(CollisionTimer());

        Debug.Log("Collided With" + collidedObject);

        if (IsInLayerMask(collidedObject.layer, lethalObstacleLayers))
        {
            EnterDeadState();
            Debug.Log("the wall killed me");
        }
        else if (IsInLayerMask(collidedObject.layer, targetHitLayers) || collidedObject.GetComponentInParent<TargetCollisionHandler>())
        {
            targetSelector.ClearAimAndTurningTargets();
            AddThrows(throwsAddedPerTargetHit);
            Debug.Log("currentthrows increased to " + currentThrows);
        }

        lastCollided = currentTarget;
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private IEnumerator CollisionTimer()
    {
        yield return new WaitForSeconds(collisionThreshold);
        Debug.Log("Timer finished!");
        collisionTimerCoroutine = null;
    }

    private void AddThrows(int amount)
    {
        currentThrows += amount;
        UpdateThrowCountUI();
    }

    public void ReduceThrows()
    {
        currentThrows--;
        UpdateThrowCountUI();
    }

    private void UpdateThrowCountUI()
    {
        if (statsUI != null)
        {
            statsUI.SetVisibleThrowCount(currentThrows);
        }
    }

    public void ShowFlyingCamera()
    {
        playerCameraController?.ShowHeliCamera();
    }

    public void ShowThrowingCamera()
    {
        playerCameraController?.ShowThirdPersonCamera();
    }
}
