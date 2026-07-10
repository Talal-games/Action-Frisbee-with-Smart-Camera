using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(TargetFeedback))]
public class TargetSelector : MonoBehaviour
{
    private TargetFeedback targetFeedback;
    [SerializeField] private Camera mainCamera;
    [FormerlySerializedAs("currentTarget")]
    [SerializeField, ReadOnly] private GameObject aimTarget;

    [SerializeField, ReadOnly] private GameObject turningTarget;
    [SerializeField] private float targetSearchRadius = 50f;
    [SerializeField] private float maxScreenDistanceFromCenter = 250f;
    [SerializeField, Min(0f)] private float targetSwitchCooldown = 0.35f;
    [SerializeField, Range(0f, 1f)] private float switchThresholdRatio = 0.8f;
    [SerializeField] private Rigidbody rb;

    private float nextTargetSwitchTime;

    public GameObject AimTarget => aimTarget;
    public GameObject TurningTarget => turningTarget;

    void Start()
    {
        targetFeedback = GetComponent<TargetFeedback>();
        if (mainCamera == null) mainCamera = Camera.main;

        TargetCollisionHandler.TargetBroken += HandleTargetBroken;

        if (aimTarget == null)
        {
            SelectInitialAimTarget();
        }
    }

    private void OnDestroy()
    {
        TargetCollisionHandler.TargetBroken -= HandleTargetBroken;
    }

    private void SelectInitialAimTarget()
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

    public void ClearAimTarget()
    {
        aimTarget = null;
        nextTargetSwitchTime = 0f;
        targetFeedback?.ClearAimTargetEffects();
    }

    public void ClearTurningTarget()
    {
        turningTarget = null;
        targetFeedback?.ClearTurningTargetEffects();
    }

    public void ClearAimAndTurningTargets()
    {
        ClearAimTarget();
        ClearTurningTarget();
    }

    public void SetTurningTargetFromAimTarget()
    {
        turningTarget = aimTarget;
        targetFeedback?.ShowTurningTargetEffects(turningTarget);
    }

    public void ClearAimTargetForThrowing(bool shouldClearTurningTarget)
    {
        ClearAimTarget();
        if (shouldClearTurningTarget)
        {
            ClearTurningTarget();
        }
    }

    public void UpdateAimTargetSelection()
    {
        SelectAimTargetNearScreenCenter();

    }
    private void SelectAimTargetNearScreenCenter()
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


    private void HandleTargetBroken(TargetCollisionHandler brokenTarget)
    {
        if (brokenTarget == null) return;

        if (aimTarget == brokenTarget.gameObject)
        {
            ClearAimTarget();
        }

        if (turningTarget == brokenTarget.gameObject)
        {
            ClearTurningTarget();
        }
    }

    private void SetAimTarget(GameObject target)
    {
        aimTarget = target;
        targetFeedback?.ShowAimTargetEffects(aimTarget);
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

