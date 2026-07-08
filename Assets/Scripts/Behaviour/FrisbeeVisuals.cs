using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrisbeeVisuals : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 1f;
    [SerializeField] private float throwSpinSpeed = 1f;
    [SerializeField] private float maxTilt = 30f;
    [SerializeField] private float tiltSmoothing = 5f;
    [SerializeField] private float maxSpeedForTilt = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform tiltTransform; // this is the object we tilt which is a parent of the object we spin so that the rotation are independant


    private float currThrowPower = 0f;

    void Update()
    {
        
    }

    public void SetThrowPower(float power)
    {
        currThrowPower = power;
    }

    public void SpinFrisbee()
    {
        Vector3 velocity = rb.velocity;
        Vector3 forward = rb.transform.forward;

        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);

        if (flatVelocity.sqrMagnitude < 0.01f && currThrowPower <= 0f)
            return;

        float direction = Vector3.Cross(flatForward, flatVelocity).y > 0 ? 1f : -1f;

        float spinFromSpeed = direction * spinSpeed * flatVelocity.magnitude * Time.deltaTime;
        float spinFromThrow = direction * throwSpinSpeed * currThrowPower * Time.deltaTime;

        transform.Rotate(Vector3.up, spinFromSpeed + spinFromThrow, Space.Self);
    }

    public void TiltFrisbee()
    {
        Vector3 velocity = rb.velocity;
        Vector3 forward = rb.transform.forward;

        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);

        if (flatVelocity.sqrMagnitude < 0.01f)
            return;

        float sideSign = Mathf.Sign(Vector3.Cross(flatVelocity, flatForward).y);
        float speedFactor = Mathf.Clamp01(flatVelocity.magnitude / maxSpeedForTilt);
        float targetTiltAngle = sideSign * maxTilt * speedFactor;

        Quaternion current = tiltTransform.localRotation;
        Quaternion target = Quaternion.Euler(0f, 0f, targetTiltAngle);
        tiltTransform.localRotation = Quaternion.Lerp(current, target, Time.deltaTime * tiltSmoothing);
        //Debug.DrawRay(tiltTransform.position, tiltTransform.right * 0.5f, Color.cyan);

    }
}
