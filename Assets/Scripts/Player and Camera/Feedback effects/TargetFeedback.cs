using UnityEngine;

public class TargetFeedback : MonoBehaviour
{
    [SerializeField] private GameObject targetedEffectsPrefab;
    [SerializeField] private GameObject turnTargetEffectsPrefab;

    private GameObject currentAimTargetEffectsInstance;
    private GameObject currentTurnTargetEffectsInstance;

    public void ShowAimTargetEffects(GameObject aimTarget)
    {
        ClearAimTargetEffects();
        if (targetedEffectsPrefab == null || aimTarget == null) return;

        currentAimTargetEffectsInstance = Instantiate(targetedEffectsPrefab, aimTarget.transform);
        currentAimTargetEffectsInstance.transform.localPosition = Vector3.zero;
    }

    public void ClearAimTargetEffects()
    {
        if (currentAimTargetEffectsInstance == null) return;

        Destroy(currentAimTargetEffectsInstance);
        currentAimTargetEffectsInstance = null;
    }

    public void ShowTurningTargetEffects(GameObject turningTarget)
    {
        ClearTurningTargetEffects();
        if (turnTargetEffectsPrefab == null || turningTarget == null) return;

        currentTurnTargetEffectsInstance = Instantiate(turnTargetEffectsPrefab, turningTarget.transform);
        currentTurnTargetEffectsInstance.transform.localPosition = Vector3.zero;
    }

    public void ClearTurningTargetEffects()
    {
        if (currentTurnTargetEffectsInstance == null) return;

        Destroy(currentTurnTargetEffectsInstance);
        currentTurnTargetEffectsInstance = null;
    }
}
