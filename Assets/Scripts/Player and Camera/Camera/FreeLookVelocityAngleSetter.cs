using Cinemachine;
using UnityEngine;

public class FreeLookVelocityAngleSetter : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private Rigidbody targetRigidbody;
    [SerializeField] private float minVelocityForDirection = 0.5f;
    [SerializeField] private float maxPitchAngle = 60f;

    public void FaceVelocityOrActiveCamera(CinemachineFreeLook cameraToAdjust, Rigidbody velocitySource)
    {
        freeLookCamera = cameraToAdjust;
        targetRigidbody = velocitySource;
        FaceVelocityOrActiveCamera();
    }

    public void FaceVelocityOrActiveCamera()
    {
        if (freeLookCamera == null || targetRigidbody == null) return;
        if (IsBlendingAwayFromFreeLook()) return;

        Vector3 direction = targetRigidbody.velocity;
        if (direction.sqrMagnitude < minVelocityForDirection * minVelocityForDirection)
        {
            if (brain == null || brain.OutputCamera == null) return;
            direction = brain.OutputCamera.transform.forward;
        }

        if (direction.sqrMagnitude <= Mathf.Epsilon) return;

        direction.Normalize();
        freeLookCamera.m_XAxis.Value = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        float pitch = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
        freeLookCamera.m_YAxis.Value = Mathf.InverseLerp(-maxPitchAngle, maxPitchAngle, pitch);
    }

    private bool IsBlendingAwayFromFreeLook()
    {
        if (brain == null || !brain.IsBlending || brain.ActiveBlend == null) return false;

        return brain.ActiveBlend.CamA == freeLookCamera && brain.ActiveBlend.CamB != freeLookCamera;
    }
}
