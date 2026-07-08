using Cinemachine;
using UnityEngine;

public enum PlayerCameraMode
{
    ThirdPerson,
    Heli
}

public class PlayerCameraController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineFreeLook thirdPersonCamera;
    [SerializeField] private CinemachineVirtualCamera heliCamera;
    [SerializeField] private bool switchByPriority = false;
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;
    [SerializeField] private FreeLookVelocityAngleSetter freeLookVelocityAngleSetter;

    private CinemachineVirtualCameraBase[] cameras;
    private float pauseFreeLookPreparationUntilTime;

    private void Awake()
    {
        if (freeLookVelocityAngleSetter == null)
        {
            freeLookVelocityAngleSetter = GetComponent<FreeLookVelocityAngleSetter>();
        }

        cameras = new CinemachineVirtualCameraBase[] { thirdPersonCamera, heliCamera };
    }

    public void SetCameraMode(PlayerCameraMode mode)
    {
        switch (mode)
        {
            case PlayerCameraMode.ThirdPerson:
                ShowThirdPersonCamera();
                break;
            case PlayerCameraMode.Heli:
                ShowHeliCamera();
                break;
        }
    }

    public void ShowThirdPersonCamera()
    {
        if (thirdPersonCamera == null || heliCamera == null)
        {
            return;
        }

        thirdPersonCamera.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        SetActiveCamera(thirdPersonCamera, "third person");
    }

    public void ShowHeliCamera()
    {
        if (thirdPersonCamera == null || heliCamera == null)
        {
            return;
        }

        PauseFreeLookPreparation();
        SetActiveCamera(heliCamera, "heli");
    }

    public void PrepareThirdPersonCamera(Rigidbody velocitySource)
    {
        if (thirdPersonCamera == null || thirdPersonCamera.enabled || Time.time < pauseFreeLookPreparationUntilTime)
        {
            return;
        }

        if (freeLookVelocityAngleSetter != null)
        {
            freeLookVelocityAngleSetter.FaceVelocityOrActiveCamera(thirdPersonCamera, velocitySource);
        }
    }

    private void SetActiveCamera(CinemachineVirtualCameraBase activeCamera, string cameraName)
    {
        if (switchByPriority)
        {
            if (activeCamera.Priority > GetHighestInactivePriority(activeCamera))
            {
                return;
            }

            foreach (var camera in cameras)
            {
                if (camera == null) continue;

                camera.enabled = true;
                camera.Priority = camera == activeCamera ? activePriority : inactivePriority;
            }

            Debug.Log("Switched camera to " + cameraName);
            return;
        }

        if (activeCamera.enabled)
        {
            return;
        }

        foreach (var camera in cameras)
        {
            if (camera == null) continue;

            camera.enabled = false;
        }

        activeCamera.enabled = true;
        Debug.Log("Switched camera to " + cameraName);
    }

    private int GetHighestInactivePriority(CinemachineVirtualCameraBase activeCamera)
    {
        int highestPriority = int.MinValue;
        foreach (var camera in cameras)
        {
            if (camera == null || camera == activeCamera) continue;

            highestPriority = Mathf.Max(highestPriority, camera.Priority);
        }

        return highestPriority;
    }

    private void PauseFreeLookPreparation()
    {
        pauseFreeLookPreparationUntilTime = Time.time + Time.deltaTime;
    }
}
