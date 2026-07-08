using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class TargetSelector : MonoBehaviour
{
    public Camera mainCamera;
    [FormerlySerializedAs("currentTarget")]
    public GameObject aimTarget;
    public LayerMask targetLayer;
    public GameObject targetedEffectsPrefab;

    private GameObject currentEffectsInstance;
    [SerializeField] private float targetSearchRadius = 50f;
    [SerializeField] private float maxScreenDistanceFromCenter = 250f;
    [SerializeField, Min(0f)] private float targetSwitchCooldown = 0.35f;
    [SerializeField, Range(0f, 1f)] private float switchThresholdRatio = 0.8f;
    [SerializeField] private Rigidbody rb;

    private float nextTargetSwitchTime;

    private bool downPressed;
    private bool upPressed;
    private bool rightPressed;
    private bool leftPressed;
    private bool isPressingDirection;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        TargetCollisionHandler.TargetBroken += HandleTargetBroken;

        if (aimTarget == null)
        {
            InitializeTarget();
        }
    }

    private void OnDestroy()
    {
        TargetCollisionHandler.TargetBroken -= HandleTargetBroken;
    }

    private void InitializeTarget()
    {
        TargetCollisionHandler[] allTargets = Object.FindObjectsByType<TargetCollisionHandler>(FindObjectsSortMode.None);
        if (allTargets.Length == 0) return;

        TargetCollisionHandler best = null;
        float bestDistSqr = float.MaxValue;
        Vector3 origin = transform.position;

        foreach (var handler in allTargets)
        {
            if (!handler.IsActive) continue;

            float distSqr = (handler.transform.position - origin).sqrMagnitude;
            if (distSqr <= targetSearchRadius * targetSearchRadius && distSqr < bestDistSqr)
            {
                best = handler;
                bestDistSqr = distSqr;
            }
        }

        if (best != null)
        {
            SetAimTarget(best.gameObject);
        }
    }

    void Update()
    {
        rightPressed = Input.GetKey(KeyCode.RightArrow);
        leftPressed = Input.GetKey(KeyCode.LeftArrow);
        upPressed = Input.GetKey(KeyCode.UpArrow);
        downPressed = Input.GetKey(KeyCode.DownArrow);

        if (Input.GetKeyUp(KeyCode.RightArrow) ||
            Input.GetKeyUp(KeyCode.LeftArrow) ||
            Input.GetKeyUp(KeyCode.UpArrow) ||
            Input.GetKeyUp(KeyCode.DownArrow))
        {
            isPressingDirection = false;
        }

    }
    public void RemoveAimTarget()
    {
        aimTarget = null;
        nextTargetSwitchTime = 0f;
        if (currentEffectsInstance != null)
        {
            Destroy(currentEffectsInstance);
            currentEffectsInstance = null;
        }
    }

    public void RunTargetSelection()
    {
        /* if (rightPressed && ! isPressingDirection) SelectInDirection(Vector2.right, targetSearchRadius);
         if (leftPressed && !isPressingDirection) SelectInDirection(Vector2.left, targetSearchRadius);
         if (upPressed && !isPressingDirection) SelectInDirection(Vector2.up, targetSearchRadius);
         if (downPressed && !isPressingDirection) SelectInDirection(Vector2.down, targetSearchRadius);*/
        SelectTargetNearCenter();

    }
    public void SelectTargetNearCenter()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        TargetCollisionHandler[] allTargets = Object.FindObjectsByType<TargetCollisionHandler>(FindObjectsSortMode.None);
        if (allTargets.Length == 0) return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        float bestScore = float.MaxValue;
        float currentAimTargetScore = float.MaxValue;
        GameObject bestTarget = null;

        foreach (var handler in allTargets)
        {
            if (!handler.IsActive) continue;

            GameObject target = handler.gameObject;

            Vector3 worldPos = target.transform.position;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
            if (screenPos.z < 0) continue; // behind camera

            float distToPlayer = Vector3.Distance(rb.transform.position, worldPos);
            if (distToPlayer > targetSearchRadius) continue;

            float distToCenter = Vector2.Distance(screenCenter, new Vector2(screenPos.x, screenPos.y));
            if (distToCenter > maxScreenDistanceFromCenter) continue;

            if (target == aimTarget)
            {
                currentAimTargetScore = distToCenter;
                continue;
            }

            if (distToCenter < bestScore)
            {
                bestScore = distToCenter;
                bestTarget = target;
            }
        }

        bool shouldSwitch = aimTarget == null || bestScore < currentAimTargetScore * switchThresholdRatio;
        if (CanSwitchTo(bestTarget) && shouldSwitch)
        {
            SwitchTarget(bestTarget);
            Debug.Log("Targeted (center screen): " + bestTarget.name);
        }
    }


    void SelectInDirection(Vector2 inputDir,float targetSelectionRange)
    {
        isPressingDirection = true;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;
        if (aimTarget == null) InitializeTarget();

        // Gather all active targets in range
        TargetCollisionHandler[] allTargets = Object.FindObjectsByType<TargetCollisionHandler>(FindObjectsSortMode.None);
        if (allTargets.Length == 0) return;

        Vector2 currentScreenPos = mainCamera.WorldToScreenPoint(aimTarget.transform.position);
        GameObject best = null;
        float bestScore = float.MaxValue;

        foreach (var handler in allTargets)
        {
            if (!handler.IsActive) continue;

            GameObject target = handler.gameObject;
            if (target == aimTarget) continue;
            if (Vector3.Distance(aimTarget.transform.position, rb.transform.position) > targetSelectionRange) continue;

            Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(target.transform.position);
            Vector2 dirToTarget = (targetScreenPos - currentScreenPos).normalized;

            float dot = Vector2.Dot(inputDir.normalized, dirToTarget);
            if (dot < 0.5f) continue;

            float distance = Vector2.Distance(currentScreenPos, targetScreenPos);
            float score = distance - dot * 1000f;

            if (score < bestScore)
            {
                bestScore = score;
                best = target;
            }
        }

        if (best != null)
        {
            Debug.Log("Targeting " + best);
            if (CanSwitchTo(best))
            {
                SwitchTarget(best);
            }
        }
    }

    private void HandleTargetBroken(TargetCollisionHandler brokenTarget)
    {
        if (brokenTarget == null || aimTarget != brokenTarget.gameObject) return;

        RemoveAimTarget();
    }
    public GameObject GetAimTarget()
    {
        return aimTarget;
    }

    void SpawnTargetedEffects()
    {
        if (targetedEffectsPrefab == null || aimTarget == null) return;

        if (currentEffectsInstance != null)
        {
            Destroy(currentEffectsInstance);
        }

        currentEffectsInstance = Instantiate(targetedEffectsPrefab, aimTarget.transform);
        currentEffectsInstance.transform.localPosition = Vector3.zero;
    }

    private void SetAimTarget(GameObject target)
    {
        aimTarget = target;
        SpawnTargetedEffects();
    }

    private bool CanSwitchTo(GameObject target)
    {
        return target != null && target != aimTarget && Time.time >= nextTargetSwitchTime;
    }

    private void SwitchTarget(GameObject target)
    {
        SetAimTarget(target);
        nextTargetSwitchTime = Time.time + targetSwitchCooldown;
    }

    private void OnDrawGizmosSelected()
    {
        if (rb == null) Gizmos.DrawWireSphere(transform.position, targetSearchRadius);

        else Gizmos.DrawWireSphere(rb.transform.position, targetSearchRadius);
    }
}
