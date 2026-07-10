using UnityEngine;

public class TargetDestructionEffects : MonoBehaviour
{
    [Header("Effects (Optional)")]
    [SerializeField] private GameObject particleEffectPrefab;
    [SerializeField] private Light targetLight;
    [SerializeField] private bool playBreakSound = true;
    [SerializeField] private bool releaseChildRigidbodies = true;

    private TargetCollisionHandler target;

    private void Awake()
    {
        target = GetComponent<TargetCollisionHandler>();
    }

    private void OnEnable()
    {
        TargetCollisionHandler.TargetBroken += HandleTargetBroken;
    }

    private void OnDisable()
    {
        TargetCollisionHandler.TargetBroken -= HandleTargetBroken;
    }

    private void HandleTargetBroken(TargetCollisionHandler brokenTarget)
    {
        if (brokenTarget == null || brokenTarget != target) return;

        Vector3 breakPosition = brokenTarget.LastBreakPosition;
        SpawnParticles(breakPosition);
        PlayBreakSound(breakPosition);
        DisableLight();
        ReleaseRigidbodies();
    }

    private void SpawnParticles(Vector3 breakPosition)
    {
        if (particleEffectPrefab == null) return;

        Instantiate(particleEffectPrefab, breakPosition, Quaternion.identity);
    }

    private void PlayBreakSound(Vector3 breakPosition)
    {
        if (!playBreakSound) return;
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(FrisbeeFmodSoundDirectory.Instance.TargetBreak, breakPosition);
    }

    private void DisableLight()
    {
        if (targetLight == null) return;

        targetLight.enabled = false;
    }

    private void ReleaseRigidbodies()
    {
        if (!releaseChildRigidbodies) return;

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}
