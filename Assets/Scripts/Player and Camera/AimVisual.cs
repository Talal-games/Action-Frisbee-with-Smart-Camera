using UnityEngine;

public class AimVisual : MonoBehaviour
{
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

    private PlayerStateController player;
    void Start()
    {
        player = GetComponent<PlayerStateController>();
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

        SetThrowPowerParticles(0f);
    }

    public void VisualizeArrow(Vector3 throwDirection, Vector3 referenceDirection, float maxArrowAngle, float curThrowPower, float maxThrowPower)
    {
        if (!useArrow)
        {
            DisableArrow();
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

            Color arrowColor = arrowMaterial.color;
            Color.Lerp(arrowColor, maxArrowColor, Mathf.Abs(throwDirection.y/ maxArrowAngle));

            AngleArrow(throwDirection);
        }
        UpdateAimParticles(throwDirection, referenceDirection, maxArrowAngle);
        SetThrowPowerParticles(maxThrowPower > 0f ? Mathf.Clamp01(curThrowPower / maxThrowPower) : 0f);
    }

    private void UpdateAimParticles(Vector3 throwDirection, Vector3 referenceDirection, float maxArrowAngle)
    {
        if (aimParticleSystem == null || maxArrowAngle <= 0f) return;

        Transform rbTransform = player.GetRBTransform();
        float aimMultiplier = Mathf.Clamp(
            Vector3.SignedAngle(referenceDirection, throwDirection, rbTransform.up) / maxArrowAngle,
            -1f,
            1f);

        ParticleSystem.MainModule main = aimParticleSystem.main;
        main.startSpeed = aimParticleFullSpeed * aimMultiplier;
    }

    private void SetThrowPowerParticles(float throwPower)
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
    }

    private void AngleArrow(Vector3 throwDirection)
    {
        arrow.transform.rotation = Quaternion.LookRotation(throwDirection);
    }

    public void DisableArrow()
    {
        if (arrow != null)
        {
            arrow.SetActive(false);
        }

        SetThrowPowerParticles(0f);
    }
}
