using UnityEngine;

public class PlayerFeedback : MonoBehaviour
{
    [SerializeField] private Light chargeLight;
    [SerializeField] private float minChargeLight = 0f;
    [SerializeField] private float maxChargeLight = 4f;
    [SerializeField] private float maxFlySpeedForVolume = 30f;

    private FMOD.Studio.EventInstance chargeThrowPowerAudioInstance;
    private FMOD.Studio.EventInstance flyingAudioInstance;

    public void PlayThrowSound(Vector3 position)
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(FrisbeeFmodSoundDirectory.Instance.ThrowFrisbee, position);
        StopThrowChargeSound();
    }

    public void UpdateThrowChargeFeedback(bool isCharging, float currentThrowPower, float maxThrowPower, Transform followTarget, Rigidbody velocitySource)
    {
        UpdateThrowChargeLight(currentThrowPower, maxThrowPower);
        UpdateThrowChargeSound(isCharging, currentThrowPower, followTarget, velocitySource);
    }

    public void StopThrowChargeSound()
    {
        if (FrisbeeFmodAudioManager.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.StopAndRelease(chargeThrowPowerAudioInstance);
        chargeThrowPowerAudioInstance = default;
    }

    public void UpdateFlyingSound(Rigidbody velocitySource)
    {
        EnsureFlyingSoundPlaying(velocitySource);
        if (!flyingAudioInstance.isValid() || FrisbeeFmodAudioManager.Instance == null || velocitySource == null) return;

        float flySpeedRatio = maxFlySpeedForVolume > 0f ? Mathf.Clamp01(velocitySource.velocity.magnitude / maxFlySpeedForVolume) : 0f;
        FrisbeeFmodAudioManager.Instance.SetEventInstanceParameter(flyingAudioInstance, "flyspeed", flySpeedRatio);
    }

    private void UpdateThrowChargeLight(float currentThrowPower, float maxThrowPower)
    {
        if (chargeLight == null) return;

        float chargeRatio = maxThrowPower > 0f ? Mathf.Clamp01(currentThrowPower / maxThrowPower) : 0f;
        chargeLight.intensity = Mathf.Lerp(minChargeLight, maxChargeLight, chargeRatio);
    }

    private void UpdateThrowChargeSound(bool isCharging, float currentThrowPower, Transform followTarget, Rigidbody velocitySource)
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        if (!isCharging)
        {
            StopThrowChargeSound();
            return;
        }

        if (!FrisbeeFmodAudioManager.Instance.IsPlaying(chargeThrowPowerAudioInstance))
        {
            chargeThrowPowerAudioInstance = FrisbeeFmodAudioManager.Instance.CreateEventInstance(
                FrisbeeFmodSoundDirectory.Instance.ChargeThrowPower,
                followTarget,
                velocitySource);

            if (chargeThrowPowerAudioInstance.isValid())
            {
                chargeThrowPowerAudioInstance.start();
            }
        }

        FrisbeeFmodAudioManager.Instance.SetEventInstanceParameter(chargeThrowPowerAudioInstance, "chargeup", currentThrowPower);
    }

    private void EnsureFlyingSoundPlaying(Rigidbody velocitySource)
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null || velocitySource == null) return;
        if (FrisbeeFmodAudioManager.Instance.IsPlaying(flyingAudioInstance)) return;

        flyingAudioInstance = FrisbeeFmodAudioManager.Instance.CreateEventInstance(
            FrisbeeFmodSoundDirectory.Instance.Flying,
            velocitySource.transform,
            velocitySource);

        if (flyingAudioInstance.isValid())
        {
            flyingAudioInstance.start();
        }
    }
}
