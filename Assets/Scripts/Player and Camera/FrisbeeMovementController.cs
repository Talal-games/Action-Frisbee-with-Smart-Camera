using System.Collections;
using UnityEngine;

public class FrisbeeMovementController : MonoBehaviour
{
    [Header("Throwing Settings")]
    [SerializeField] private float angleRate = 1f;
    [SerializeField] private float maxAimAngle = 90f;
    [SerializeField] private float throwPowerRate = 1f;
    [SerializeField] private float maxThrowPower = 10f;
    [SerializeField] private float minThrowPower = 2f;
    [SerializeField] private float aimAtTargetSpeed = 2f;

    [Header("Turning Settings")]
    [SerializeField] private float turnRate = 5f;
    [SerializeField] private float autoTurnThreshold = 0.95f;
    [SerializeField] private bool isAutoTurn;
    [SerializeField] private float ySpeed = 1f;
    [SerializeField] private float yVelocityBrakingSpeed = 10f;

    [Header("Reset Settings")]
    [SerializeField] private float resetSpeed = 4f;
    [SerializeField] private float resetDuration = 1f;
    [SerializeField] private float safeDistance = 2f;
    [SerializeField] private LayerMask obstacleMask;

    private Rigidbody rb;
    private AimVisual aimVisual;
    private Vector3 throwDirection;
    private Vector3 throwReferenceDirection;
    [SerializeField, ReadOnly] private float currentThrowPower;
    [SerializeField, ReadOnly] private bool isResetted;
    private Coroutine resetRoutine;

    public float CurrentThrowPower => currentThrowPower;
    public float MaxThrowPower => maxThrowPower;
    public float MinThrowPower => minThrowPower;
    public bool IsResetting => resetRoutine != null;
    public bool IsResetted
    {
        get => isResetted;
        set => isResetted = value;
    }

    public Rigidbody Rigidbody => rb;
    public Transform RigidbodyTransform => rb.transform;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        aimVisual = GetComponent<AimVisual>();
        ResetThrowDirectionToForward();
    }

    public void ApplyThrowImpulse()
    {
        rb.AddForce(throwDirection * currentThrowPower, ForceMode.Impulse);
        ResetThrowDirectionToForward();
        aimVisual.HideAimVisuals();
    }

    public void UpdateAimedThrowDirection(float horizontalInput, GameObject selectedTarget)
    {
        float throwAngleRaw = 0;
        Vector3 forward = rb.velocity.sqrMagnitude > 0.01f ? rb.velocity.normalized : rb.transform.forward;

        if (selectedTarget != null)
        {
            forward = (selectedTarget.transform.position - rb.transform.position).normalized;
        }

        throwReferenceDirection = forward;
        if (horizontalInput == 0)
        {
            throwDirection = forward;
        }
        else
        {
            throwAngleRaw = horizontalInput * angleRate;
            Quaternion rotation = Quaternion.AngleAxis(throwAngleRaw, rb.transform.up);
            if (Mathf.Abs(Vector3.Angle(throwReferenceDirection, throwDirection)) < maxAimAngle)
            {
                throwDirection = rotation * throwDirection;
            }
        }

        Debug.DrawRay(rb.transform.position, forward * 5f, Color.red);
    }

    public void ChargeThrowPowerIfHeld(bool isCharging)
    {
        if (isCharging && currentThrowPower < maxThrowPower)
        {
            currentThrowPower += throwPowerRate;
        }
    }

    public void ResetThrowPower()
    {
        currentThrowPower = 0f;
    }

    public void UpdateThrowAimVisuals()
    {
        aimVisual.UpdateAimVisuals(throwDirection, throwReferenceDirection, maxAimAngle, currentThrowPower, maxThrowPower);
    }

    public void StopRigidbodyMotion()
    {
        rb.velocity = Vector3.zero;
        rb.freezeRotation = true;
        rb.freezeRotation = false;
    }

    public void ResetThrowDirectionToForward()
    {
        if (rb == null) return;

        throwDirection = rb.transform.forward;
        throwReferenceDirection = rb.transform.forward;
    }

    public void UpdateFlightTurn(float horizontalInput, GameObject turningTarget)
    {
        if (isAutoTurn)
        {
            AutoTurnTowardTarget(turningTarget);
        }
        else
        {
            ApplyManualTurn(horizontalInput);
        }
    }

    private void ApplyManualTurn(float horizontalInput)
    {
        ApplyLateralTurnForce(horizontalInput);
    }

    public void FaceMovementDirection()
    {
        if (rb.velocity.magnitude > .5)
        {
            Vector3 velocityDirection = rb.velocity.normalized;
            rb.transform.rotation = Quaternion.LookRotation(velocityDirection);
        }
    }

    private void AutoTurnTowardTarget(GameObject turningTarget)
    {
        if (turningTarget == null) return;

        Vector3 velDirection = rb.velocity;
        velDirection.y = 0;
        velDirection = velDirection.normalized;

        Vector3 targetDirection = turningTarget.transform.position - rb.transform.position;
        targetDirection.y = 0;
        targetDirection = targetDirection.normalized;

        float angle = Vector3.SignedAngle(velDirection, targetDirection, Vector3.up);
        float direction = Mathf.Sign(angle);
        if (Vector3.Dot(targetDirection, velDirection) < autoTurnThreshold)
        {
            ApplyLateralTurnForce(direction);
        }
    }

    public void UpdateFlightElevationTowardTarget(GameObject turningTarget)
    {
        if (turningTarget == null)
        {
            BrakeVerticalVelocity();
            return;
        }

        Vector3 toTarget = turningTarget.transform.position - rb.transform.position;
        float verticalDistance = toTarget.y;

        Debug.Log("VERTICAL DISTANCE: " + verticalDistance);
        if (Mathf.Abs(verticalDistance) < 0.1f)
        {
            BrakeVerticalVelocity();
            return;
        }

        float speed = rb.velocity.magnitude;
        float gravityBoost = Mathf.Sign(verticalDistance) < 0 ? 2f : 1f;
        float direction = Mathf.Sign(verticalDistance);
        float forceMagnitude = direction * Mathf.Clamp(ySpeed * 0.5f * speed * gravityBoost, 0f, 20f);
        Debug.Log("applying y force of: " + forceMagnitude);

        Vector3 force = new Vector3(0, forceMagnitude, 0);
        rb.AddForce(force, ForceMode.Force);
    }

    private void BrakeVerticalVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = Mathf.MoveTowards(velocity.y, 0f, yVelocityBrakingSpeed * Time.fixedDeltaTime);
        rb.velocity = velocity;
    }

    private void ApplyLateralTurnForce(float direction)
    {
        Vector3 forceToAdd = rb.transform.right * turnRate * rb.velocity.magnitude * direction;
        rb.AddForce(forceToAdd, ForceMode.Force);
    }

    public void ReleaseRigidbodyConstraints()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }

    public bool IsMovingSlowEnoughToReset()
    {
        return rb.velocity.magnitude < resetSpeed;
    }

    public bool TryStartSafePositionReset(GameObject turningTarget)
    {
        if (isResetted || resetRoutine != null) return false;

        isResetted = true;
        if (IsInSafeSpot()) return false;

        resetRoutine = StartCoroutine(MoveToSafeSpot(turningTarget));
        return true;
    }

    public void RotateTowardTurningTargetWhenSlow(GameObject turningTarget)
    {
        if (resetRoutine != null) return;
        if (rb.velocity.magnitude > resetSpeed) return;
        if (turningTarget == null) return;

        Quaternion targetRotation = FindTargetRotation(turningTarget.transform);
        rb.transform.rotation = Quaternion.RotateTowards(rb.transform.rotation, targetRotation, aimAtTargetSpeed * Time.deltaTime);
    }

    private bool IsInSafeSpot()
    {
        return !Physics.CheckSphere(rb.position, safeDistance, obstacleMask);
    }

    private IEnumerator MoveToSafeSpot(GameObject turningTarget)
    {
        Debug.Log("Stopping and Finding Safe Spot");

        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;
        Vector3 targetPos = FindSafePosition(rb.position, safeDistance);
        Quaternion targetRot = turningTarget != null ? FindTargetRotation(turningTarget.transform) : Quaternion.identity;
        float elapsed = 0f;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        while (elapsed < resetDuration)
        {
            float t = elapsed / resetDuration;
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            rb.MoveRotation(Quaternion.Slerp(startRot, targetRot, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(targetPos);
        rb.MoveRotation(targetRot);

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        resetRoutine = null;
    }

    private Quaternion FindTargetRotation(Transform target)
    {
        Vector3 directionToTarget = target.transform.position - rb.transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.sqrMagnitude < 0.001f) return Quaternion.identity;

        return Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
    }

    private Vector3 FindSafePosition(Vector3 origin, float minDistance)
    {
        const int maxAttempts = 20;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 offset = Random.onUnitSphere * minDistance;
            Vector3 checkPos = origin + offset;
            if (!Physics.CheckSphere(checkPos, safeDistance, obstacleMask))
            {
                return checkPos;
            }
        }

        return origin;
    }
}
