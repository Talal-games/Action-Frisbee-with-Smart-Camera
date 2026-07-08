using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;



public class PlayerStateController : MonoBehaviour
{
    private AimVisual aimVisual;
    private TargetSelector targetSelector;
    private FrisbeeVisuals frisbeeVisuals;


    private Vector3 throwDirection;
    private Vector3 throwReferenceDirection;
    private GameObject turningTarget;

    [Header("Global Player Settings")]
    [SerializeField] int startingThrows = 2;
    [SerializeField] int throwsAddedPerTargetHit = 1;
    [SerializeField] float collisionThreshold = 0.3f;
    private Coroutine timerCoroutine;
    private int currentThrows;
    public int GetCurrentThrows => currentThrows;
    

    [Header("Controls")]
    [SerializeField] float jumpBufferTime = 0.1f;
    private float jumpReleaseBuffer = 0f; // this will be used instead of jumpRelease because we check in FixedUpdate

    private bool jumpPressed;
    private bool jumpReleased;
    [SerializeField] float switchingBufferTime = 1.0f;//this buffer makes it so we don't check for slowness right after throwing
    private float switchingBuffer = 0f;

    [Header("Throwing Settings")]
    [SerializeField] public float throwForce = 5f;
    [SerializeField] public float angleRate = 1f; 
    [SerializeField] public float maxAimAngle = 90f;
    [SerializeField] private float throwPowerRate = 1f;
    [SerializeField] private float maxThrowPower = 10f;
    [SerializeField] private float minThrowPower = 2f;
    [FormerlySerializedAs("isAimingClearsTarget")]
    [SerializeField] private bool isAimingClearTarget = false;
    [SerializeField] private float aimAtTargetSpeed = 2f;
    private float curThrowPower = 0;


    [Header("Turning Settings")]
    [SerializeField] public float maxVelGainFromTurn = 1.2f;
    [SerializeField] public float turnRate = 5f; // This and the initial throw velocity are used to calculate turn speed
    [SerializeField] private float autoTurnThreshold = 0.95f;
    [SerializeField] private bool isAutoTurn;
    [SerializeField] private float ySpeed = 1f;
    [SerializeField] private float yVelocityBrakingSpeed = 10f;
    private Vector3 velAtThrow;

    [Header("Reset Settings")]
    [SerializeField] private float resetSpeed = 4f;
    public float resetDuration = 1f;
    public float safeDistance = 2f;
    private bool isResetted = false;
    public LayerMask obstacleMask;
    private Coroutine resetRoutine;

    [Header("Collision Layers")]
    [SerializeField] private LayerMask targetHitLayers;
    [SerializeField] private LayerMask lethalObstacleLayers;

    [Header("Cameras")]
    [SerializeField] private PlayerCameraController playerCameraController;

    private Stack<PlayerState> stateStack = new Stack<PlayerState>();
    private Rigidbody rb;

    TargetCollisionHandler lastCollided;

    [Header("Effects (Optional)")]

    [SerializeField] private GameObject turnTargetEffectsPrefab;
    [SerializeField] private Light chargeLight;
    [SerializeField] private float minChargeLight = 0f;
    [SerializeField] private float maxChargeLight = 4f;
    private GameObject currentTurnTargetEffectsInstance;
    private FMOD.Studio.EventInstance chargeThrowPowerAudioInstance;
    private FMOD.Studio.EventInstance flyingAudioInstance;
    [SerializeField] private float maxFlySpeedForVolume = 30f;




    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        aimVisual = GetComponent<AimVisual>();
        targetSelector = GetComponent<TargetSelector>();
        if (playerCameraController == null) playerCameraController = GetComponent<PlayerCameraController>();
        TargetCollisionHandler.TargetBroken += HandleTargetBroken;
        frisbeeVisuals = GetComponentInChildren<FrisbeeVisuals>();
        currentThrows = startingThrows;
        PushState(new ThrowingState(this));  // Example initial state

    }

    private void OnDestroy()
    {
        TargetCollisionHandler.TargetBroken -= HandleTargetBroken;
    }

    void Update()
    {
        if (Input.GetButtonUp("Jump"))
            jumpReleaseBuffer = jumpBufferTime;

        if (jumpReleaseBuffer > 0f)
            jumpReleaseBuffer -= Time.deltaTime;

        jumpPressed = Input.GetButton("Jump");
       switchingBuffer -= Time.deltaTime;

        frisbeeVisuals.SetThrowPower(curThrowPower);
        UpdateThrowChargeLight();
       frisbeeVisuals.SpinFrisbee();
        frisbeeVisuals.TiltFrisbee();
        UpdateFlyingAudio();

        if (Input.GetKeyUp(KeyCode.P))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        playerCameraController?.PrepareThirdPersonCamera(rb);
    }
    private void FixedUpdate()
    {
        if (stateStack.Count > 0) stateStack.Peek().ExecuteStateLogic();
        jumpReleased = false;
    }

    public void ChangeState(PlayerState newState)
    {
        PopState();
        PushState(newState);
    }

    public void PushState(PlayerState newState)
    {
        if (stateStack.Count > 0) stateStack.Peek().Exit();
        stateStack.Push(newState);
        newState.Enter();
    }
    public void SetTurningTargetFromAimTarget()
    {
        turningTarget = targetSelector.GetAimTarget();
        SpawnTurnTargetEffects();
    }

    public void ClearAimTargetForThrowingState()
    {
        targetSelector.RemoveAimTarget();
        if (isAimingClearTarget)
        {
            ClearTurningTarget();
        }
    }

    private void ClearTurningTarget()
    {
        turningTarget = null;
        ClearTurnTargetEffects();
    }

    private void SpawnTurnTargetEffects()
    {
        ClearTurnTargetEffects();

        if (turnTargetEffectsPrefab == null || turningTarget == null) return;

        currentTurnTargetEffectsInstance = Instantiate(turnTargetEffectsPrefab, turningTarget.transform);
        currentTurnTargetEffectsInstance.transform.localPosition = Vector3.zero;
    }

    private void ClearTurnTargetEffects()
    {
        if (currentTurnTargetEffectsInstance == null) return;

        Destroy(currentTurnTargetEffectsInstance);
        currentTurnTargetEffectsInstance = null;
    }

    private void ClearAimTargetAndTurningTarget()
    {
        ClearTurningTarget();
        targetSelector.RemoveAimTarget();
    }

    private void HandleTargetBroken(TargetCollisionHandler brokenTarget)
    {
        if (brokenTarget == null) return;

        ClearAimTargetAndTurningTarget();
    }
    public void PopState()
    {
        if (stateStack.Count == 0) return;
        stateStack.Pop().Exit();
        if (stateStack.Count > 0) stateStack.Peek().Enter();
    }
    public void PlayThrowAudio()
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(FrisbeeFmodSoundDirectory.Instance.ThrowFrisbee, rb.transform.position);
        StopThrowChargeAudio();
    }

    public void UpdateThrowChargeAudio()
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        bool isCharging = jumpPressed && curThrowPower > 0f;
        if (!isCharging)
        {
            StopThrowChargeAudio();
            return;
        }

        if (!FrisbeeFmodAudioManager.Instance.IsPlaying(chargeThrowPowerAudioInstance))
        {
            chargeThrowPowerAudioInstance = FrisbeeFmodAudioManager.Instance.CreateEventInstance(
                FrisbeeFmodSoundDirectory.Instance.ChargeThrowPower,
                rb.transform,
                rb);

            if (chargeThrowPowerAudioInstance.isValid())
            {
                chargeThrowPowerAudioInstance.start();
            }
        }

        FrisbeeFmodAudioManager.Instance.SetEventInstanceParameter(chargeThrowPowerAudioInstance, "chargeup", curThrowPower);
    }

    private void UpdateThrowChargeLight()
    {
        if (chargeLight == null) return;

        float chargeRatio = maxThrowPower > 0f ? Mathf.Clamp01(curThrowPower / maxThrowPower) : 0f;
        chargeLight.intensity = Mathf.Lerp(minChargeLight, maxChargeLight, chargeRatio);
    }

    public void StopThrowChargeAudio()
    {
        if (FrisbeeFmodAudioManager.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.StopAndRelease(chargeThrowPowerAudioInstance);
        chargeThrowPowerAudioInstance = default;
    }

    private void EnsureFlyingAudioPlaying()
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;
        if (FrisbeeFmodAudioManager.Instance.IsPlaying(flyingAudioInstance)) return;

        flyingAudioInstance = FrisbeeFmodAudioManager.Instance.CreateEventInstance(
            FrisbeeFmodSoundDirectory.Instance.Flying,
            rb.transform,
            rb);

        if (flyingAudioInstance.isValid())
        {
            flyingAudioInstance.start();
        }
    }

    public void UpdateFlyingAudio()
    {
        EnsureFlyingAudioPlaying();
        if (!flyingAudioInstance.isValid() || FrisbeeFmodAudioManager.Instance == null) return;

        float flySpeedRatio = maxFlySpeedForVolume > 0f ? Mathf.Clamp01(rb.velocity.magnitude / maxFlySpeedForVolume) : 0f;
        FrisbeeFmodAudioManager.Instance.SetEventInstanceParameter(flyingAudioInstance, "flyspeed", flySpeedRatio);
    }
    public void Throw()
    {
        PlayThrowAudio();
        ReduceThrows();
        //rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        rb.AddForce(throwDirection * curThrowPower, ForceMode.Impulse);

        ResetThrowing();
        aimVisual.DisableArrow();
    }

    public void RunTargetSelection()
    {
        targetSelector.RunTargetSelection();
    }
    
    

    public void FindThrowDirection()
    {
        float throwAngleRaw = 0;
        Vector3 forward = rb.velocity.sqrMagnitude > 0.01f ? rb.velocity.normalized : rb.transform.forward;
        GameObject selectedTarget = targetSelector.GetAimTarget();

        if (selectedTarget != null)
        {
            forward = (selectedTarget.transform.position - rb.transform.position).normalized;
        }
        throwReferenceDirection = forward;
        if(Input.GetAxis("Horizontal") == 0)
        {
            throwDirection = forward;
        }
        else
        {
            
            throwAngleRaw = Input.GetAxis("Horizontal") * angleRate;
            Quaternion rotation = Quaternion.AngleAxis(throwAngleRaw, rb.transform.up);
            if(Mathf.Abs(Vector3.Angle(throwReferenceDirection, throwDirection)) < maxAimAngle)
                throwDirection = rotation * throwDirection;
        }
        Debug.DrawRay(rb.transform.position, forward * 5f, Color.red);
        //Debug.Log(throwDirection);
    }
    public void FindThrowPower()
    {
       if(jumpPressed && curThrowPower < maxThrowPower)
        {
            curThrowPower += throwPowerRate;
        }
        //Debug.Log("currthrowpower: " + curThrowPower);
    }
    public void InitiateFly()
    {
        if (curThrowPower > minThrowPower && jumpReleaseBuffer > 0f)
        {
            ChangeState(new FlyingState(this));
           // Debug.Log("throw conditions met");
            curThrowPower = 0;
            jumpReleaseBuffer = 0f;

        }
        else if (jumpReleaseBuffer > 0f)
        {
            curThrowPower = 0;
            jumpReleaseBuffer = 0f;

        }
    }

    public void SwitchToThrowingIfPressing()
    {
        if (jumpPressed)
        {
            Debug.Log("Switching to throwinstate due to press");

            ChangeState(new ThrowingState(this));
        }
    }
    public void ResetSwitchingBuffer()
    {
        switchingBuffer = switchingBufferTime;
    }

    public void SlowManager()
    {
        //SLOW AND NO THROWS LEFT = DIE
        if(rb.velocity.magnitude < resetSpeed && currentThrows < 1)
        {
            ChangeState(new DeadState(this));
            return;
        }
        //SLOW AND NOT JUST SWITCHED 
        if (rb.velocity.magnitude < resetSpeed && switchingBuffer < 0f)
        {
            Debug.Log("Switching to throwing state becuase very slow");

            ChangeState(new ThrowingState(this));

            if (!isResetted)
            {
                isResetted = true;
                if (resetRoutine != null) return;
                if (IsInSafeSpot()) return; // Already in a safe place

                resetRoutine = StartCoroutine(GoToSafeSpot());
            }
        }
       
    }
    
    private bool IsInSafeSpot()
    {
        return !Physics.CheckSphere(rb.position, safeDistance, obstacleMask);
    }
    
    public void SetIsResetted(bool state)
    {
        isResetted = state;
    }
    private IEnumerator GoToSafeSpot()
    {
        Debug.Log("Stopping and Finding Safe Spot");

        Vector3 startPos = rb.position;
        Quaternion startRot = rb.rotation;

        // Find a safe position
        Vector3 targetPos = FindSafePosition(rb.position, safeDistance);
        //find the rotation
        Quaternion targetRot;
        if (turningTarget != null)
        {

            targetRot = FindTargetRotation(turningTarget.transform);
        }
        else
            targetRot = Quaternion.identity;

        float elapsed = 0f;

        // Disable physics temporarily
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

        // Zero velocity and re-enable physics

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        resetRoutine = null;

    }
    public void SmoothAimAtTarget()
    {
        if (resetRoutine != null) return; // avoids two thing controlling rotation
        if (rb.velocity.magnitude > resetSpeed)
            return;// this is so this doesn't affect turning when the frisbee is flying
        if (turningTarget == null) return;
        Quaternion targetRotation = FindTargetRotation(turningTarget.transform);
        rb.transform.rotation = Quaternion.RotateTowards(rb.transform.rotation, targetRotation, aimAtTargetSpeed * Time.deltaTime);
    }
    private Quaternion FindTargetRotation(Transform target)
    {

        Vector3 directionToTarget = target.transform.position - rb.transform.position;
        directionToTarget.y = 0;    //get the vector in terms of x and z so that it remains same as world up

        if (directionToTarget.sqrMagnitude < 0.001f) return Quaternion.identity; // avoid zero direction

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
                return checkPos;
        }

        // Fallback if no safe spot found
        return origin;
    }

    public void VisualizeThrowAngle()
    {
        aimVisual.VisualizeArrow(throwDirection, throwReferenceDirection, maxAimAngle, curThrowPower, maxThrowPower);
    }
    public void StopFlying()
    {
        rb.velocity = Vector3.zero;
        rb.freezeRotation = true;
        rb.freezeRotation = false;
        
    }
    public void ResetThrowing()
    {
        throwDirection = rb.transform.forward;
        throwReferenceDirection = rb.transform.forward;
    }

    public void TurnManager()
    {
        if (isAutoTurn)
        {
            AutoTurn();
        }
        else
        {
            ManualTurn();
        }
    }
    public void ManualTurn()
    {
        ApplyTurn(Input.GetAxis("Horizontal"));
    }
    public void FaceVelocity()
    {
        if (rb.velocity.magnitude > .5)
        {
            Vector3 velocityDirection = rb.velocity.normalized;
            rb.transform.rotation = Quaternion.LookRotation(velocityDirection);
        }
        //transform.forward = rb.transform.forward;
    }
    public void AutoTurn()
    {
        if (turningTarget == null) return;
        Vector3 velDirection = rb.velocity;
        velDirection.y = 0;
        velDirection = velDirection.normalized;


        Vector3 targetDirection = turningTarget.transform.position - rb.transform.position;
        targetDirection.y = 0;
        targetDirection = targetDirection.normalized;

        //find if we should turn right or left to reach the target driection
        float angle = Vector3.SignedAngle(velDirection, targetDirection, Vector3.up); // Angle relative to the Y axis (for 2D or horizontal planes)
        float direction = Mathf.Sign(angle); // Returns -1 for left, 1 for right
        //Debug.Log("angle: " + angle + "direction: " + direction);
       // Debug.Log("dot between target direction and vel direction: " + Vector3.Dot(targetDirection, velDirection));
        if (Vector3.Dot(targetDirection, velDirection) < autoTurnThreshold)
        {
           // Debug.Log("applying auto turn in direction:" + direction);
            ApplyTurn(direction);
        }
        
    }
    public void AutoChangeElevation()
    {
        if (turningTarget == null)
        {
            BreakYVelocity();
            return;
        }

        Vector3 toTarget = turningTarget.transform.position - rb.transform.position;
        float verticalDistance = toTarget.y;

        //if vertically aligned, stop adjusting
        Debug.Log("VERTICAL DISTANCE: " + verticalDistance);
        if (Mathf.Abs(verticalDistance) < 0.1f)
        {
            BreakYVelocity();
            return;
        }
        float speed = rb.velocity.magnitude;

        //downward movement gets stronger boost
        float gravityBoost = Mathf.Sign(verticalDistance) < 0 ? 2f : 1f;

        //proportional force based on distance and speed
        float direction = Mathf.Sign(verticalDistance); // -1 if target below, +1 if target above
        float forceMagnitude = direction * Mathf.Clamp(ySpeed * 0.5f * speed * gravityBoost, 0f, 20f);
        Debug.Log("applying y force of: " + forceMagnitude);

        Vector3 force = new Vector3(0, forceMagnitude, 0);

        rb.AddForce(force, ForceMode.Force);
    }

    private void BreakYVelocity()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = Mathf.MoveTowards(velocity.y, 0f, yVelocityBrakingSpeed * Time.fixedDeltaTime);
        rb.velocity = velocity;
    }

    public void ApplyTurn(float direction)
    {
        Vector3 forceToAdd = rb.transform.right * turnRate * rb.velocity.magnitude * direction;
        rb.AddForce(forceToAdd, ForceMode.Force);
    }
    public Transform GetRBTransform()
    {
        return rb.transform;
    }
    public void SaveVel()
    {
        velAtThrow = rb.velocity;
        //Debug.Log("saved Vel at throw" + velAtThrow);
    }
    public void RemoveRbConstraints()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;
    }

    public void OnFrisbeeCollisionEnter(GameObject collidedObject)
    {
        var currTarget = collidedObject.GetComponentInParent<TargetCollisionHandler>();

        if (lastCollided == currTarget && timerCoroutine != null) return;
        //if this is the same object we just hit within the threshold dont react to collision again


        timerCoroutine = StartCoroutine(CollisionTimer());

        Debug.Log("Collided With" + collidedObject);

        if (IsInLayerMask(collidedObject.layer, lethalObstacleLayers))
        {
            ChangeState(new DeadState(this));
            Debug.Log("the wall killed me");
        }
        else if (IsInLayerMask(collidedObject.layer, targetHitLayers) || collidedObject.GetComponentInParent<TargetCollisionHandler>())
        {
            

            ClearAimTargetAndTurningTarget();
            currentThrows += throwsAddedPerTargetHit;
            Debug.Log("currentthrows increased to " + currentThrows);
            
        }
        lastCollided = currTarget;
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    IEnumerator CollisionTimer()
    {
        yield return new WaitForSeconds(collisionThreshold);

        // Timer finished — do something
        Debug.Log("Timer finished!");

        timerCoroutine = null;
    }
    public void ReduceThrows()
    {
        currentThrows--;
    }
    public void ChooseFlyingCamera()
    {
        playerCameraController?.ShowHeliCamera();
    }

    public void ChooseThrowingCamera()
    {
        playerCameraController?.ShowThirdPersonCamera();
    }
}





