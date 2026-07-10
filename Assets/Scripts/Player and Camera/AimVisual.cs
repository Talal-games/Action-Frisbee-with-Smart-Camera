using UnityEngine;

public class AimVisual : MonoBehaviour
{
    [Header("Effects (Optional)")]
    [SerializeField] bool useArrow = true;
    [SerializeField] GameObject arrow;
    [SerializeField] Transform arrowModel;
    [SerializeField] Material arrowMaterial;
    [SerializeField] ParticleSystem aimParticleSystem;
    [SerializeField] ParticleSystem throwPowerParticleSystem;
    [SerializeField] float maxArrowSize = 2f;
    [SerializeField] Color maxArrowColor = Color.red;
    Vector3 arrowStartScale;
    float aimParticleFullSpeed;
    ParticleSystem.Particle[] reusableParticles;

    private PlayerController player;
    void Start()
    {
        player = GetComponent<PlayerController>();
        if (arrow != null)
        {
            arrow.SetActive(useArrow);
        }

        if (arrowModel != null)
        {
            arrowStartScale = arrowModel.localScale;
        }

        if (aimParticleSystem != null)
        {
            aimParticleFullSpeed = aimParticleSystem.main.startSpeed.constant;
        }

        SetThrowPowerParticleVisibilityAndAlpha(0f);
    }

    public void UpdateAimVisuals(Vector3 throwDirection, Vector3 referenceDirection, float maxArrowAngle, float curThrowPower, float maxThrowPower)
    {
        if (!useArrow)
        {
            SetArrowActive(false);
        }

        if (useArrow && arrow != null && arrowModel != null)
        {
            if (throwDirection != referenceDirection)
            {
                arrow.SetActive(true);
            }
            float arrowSizeMul = Mathf.Lerp(0, maxArrowSize, curThrowPower / maxThrowPower);
            arrowModel.localScale = arrowStartScale * arrowSizeMul;
          //  Debug.Log("arrowSizeMul = " + arrowSizeMul);

            if (arrowMaterial != null)
            {
                Color arrowColor = arrowMaterial.color;
                arrowMaterial.color = Color.Lerp(arrowColor, maxArrowColor, Mathf.Abs(throwDirection.y / maxArrowAngle));
            }
        }
        RotateAimVisuals(throwDirection, referenceDirection);
        UpdateAimParticles(throwDirection, referenceDirection, maxArrowAngle);
        SetThrowPowerParticleVisibilityAndAlpha(maxThrowPower > 0f ? Mathf.Clamp01(curThrowPower / maxThrowPower) : 0f);
    }

    private void UpdateAimParticles(Vector3 throwDirection, Vector3 referenceDirection, float maxArrowAngle)
    {
        if (aimParticleSystem == null || maxArrowAngle <= 0f) return;

        Transform rbTransform = player.GetFrisbeeRigidbodyTransform();
        float aimMultiplier = Mathf.Clamp(
            Vector3.SignedAngle(referenceDirection, throwDirection, rbTransform.up) / maxArrowAngle,
            -1f,
            1f);

        ParticleSystem.MainModule main = aimParticleSystem.main;
        float targetSpeed = aimParticleFullSpeed * aimMultiplier;
        main.startSpeed = targetSpeed;
        ApplySpeedToLiveParticles(aimParticleSystem, targetSpeed);
    }

    private void SetThrowPowerParticleVisibilityAndAlpha(float throwPower)
    {
        if (throwPowerParticleSystem == null) return;

        bool hasThrowPower = throwPower > 0f;
        if (throwPowerParticleSystem.gameObject.activeSelf != hasThrowPower)
        {
            throwPowerParticleSystem.gameObject.SetActive(hasThrowPower);
        }

        ParticleSystem.MainModule main = throwPowerParticleSystem.main;
        Color startColor = main.startColor.color;
        startColor.a = throwPower;
        main.startColor = startColor;
        ApplyAlphaToLiveParticles(throwPowerParticleSystem, throwPower);
    }

    private void ApplyAlphaToLiveParticles(ParticleSystem particleSystem, float alpha)
    {
        int particleCount = GetLiveParticles(particleSystem);
        for (int i = 0; i < particleCount; i++)
        {
            Color32 startColor = reusableParticles[i].startColor;
            startColor.a = (byte)Mathf.RoundToInt(alpha * 255f);
            reusableParticles[i].startColor = startColor;
        }

        particleSystem.SetParticles(reusableParticles, particleCount);
    }

    private void ApplySpeedToLiveParticles(ParticleSystem particleSystem, float speed)
    {
        int particleCount = GetLiveParticles(particleSystem);
        float speedMagnitude = Mathf.Abs(speed);
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 direction = reusableParticles[i].velocity.normalized;
            if (direction.sqrMagnitude > 0f)
            {
                reusableParticles[i].velocity = direction * speedMagnitude;
            }
        }

        particleSystem.SetParticles(reusableParticles, particleCount);
    }

    private int GetLiveParticles(ParticleSystem particleSystem)
    {
        int maxParticles = particleSystem.main.maxParticles;
        if (reusableParticles == null || reusableParticles.Length < maxParticles)
        {
            reusableParticles = new ParticleSystem.Particle[maxParticles];
        }

        return particleSystem.GetParticles(reusableParticles);
    }

    private void RotateAimVisuals(Vector3 throwDirection, Vector3 referenceDirection)
    {
        if (arrow != null && throwDirection.sqrMagnitude > 0.001f)
        {
            arrow.transform.rotation = Quaternion.LookRotation(throwDirection);
        }

        if (aimParticleSystem != null && referenceDirection.sqrMagnitude > 0.001f)
        {
            aimParticleSystem.transform.rotation = Quaternion.LookRotation(referenceDirection);
        }
    }

    public void HideAimVisuals()
    {
        SetArrowActive(false);
        SetThrowPowerParticleVisibilityAndAlpha(0f);
    }

    private void SetArrowActive(bool isActive)
    {
        if (arrow != null)
        {
            arrow.SetActive(isActive);
        }
    }
}


