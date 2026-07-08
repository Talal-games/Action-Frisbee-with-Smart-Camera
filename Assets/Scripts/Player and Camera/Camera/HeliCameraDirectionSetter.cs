using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class HeliCameraDirectionSetter : MonoBehaviour
{
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool keepGenerating = true;
    [SerializeField] private Vector2 changeInterval = new Vector2(2f, 4f);
    [SerializeField] private Vector2 rotationSpeed = new Vector2(90f, 180f);
    [SerializeField, Range(0f, 120f)] private float minimumAngleSeparation = 70f;
    [SerializeField] private float[] shootingAngles = new float[3];
    [SerializeField] private float selectedShootingAngle;
    [SerializeField] private float selectedShotOccupiedPercent;

    [Header("Target Direction Visualization")]
    [SerializeField] private bool visualizeTargetDirection = true;
    [SerializeField] private float targetDirectionLength = 6f;
    [SerializeField] private float targetDirectionHeadLength = 1f;
    [SerializeField] private float targetDirectionHeadWidth = 0.5f;
    [SerializeField] private Color targetDirectionColor = Color.green;

    [Header("Camera Distance")]
    [FormerlySerializedAs("maxAgainstVelocityZMultiplier")]
    [SerializeField] private float maxAgainstVelocityCameraDistanceMultiplier = 1.5f;
    [FormerlySerializedAs("speedForMaxZMultiplier")]
    [SerializeField] private float speedForMaxCameraDistanceMultiplier = 25f;
    [FormerlySerializedAs("maxSpeedZMultiplier")]
    [SerializeField] private float maxSpeedCameraDistanceMultiplier = 1.5f;
    [FormerlySerializedAs("followOffsetZChangeSpeed")]
    [SerializeField] private float cameraDistanceChangeSpeed = 10f;
    [FormerlySerializedAs("currentZMultiplier")]
    [SerializeField] private float currentCameraDistanceMultiplier = 1f;

    [Header("Shot Evaluation")]
    [SerializeField] private CinemachineVirtualCamera evaluatedCamera;
    [SerializeField] private Rigidbody frisbeeRigidbody;
    [SerializeField] private LayerMask shotBlockLayers = ~0;
    [SerializeField] private float shotBoxThickness = 3f;
    [SerializeField] private float shotBoxSecondsAhead = 1f;
    [SerializeField] private Vector3Int occupancySamples = new Vector3Int(3, 3, 8);
    [SerializeField] private bool drawShotBoxGizmo = true;

    private const int AngleCount = 3;
    private const int MaxRandomAttempts = 100;
    private Coroutine rotationRoutine;
    private Coroutine cameraDistanceRoutine;

    private Vector3 lastShotBoxCenter;
    private Vector3 lastShotBoxHalfExtents;
    private Quaternion lastShotBoxRotation = Quaternion.identity;
    private Vector3 baseFollowOffset;
    private bool hasBaseFollowOffset;

    private void Start()
    {
        CacheBaseFollowOffset();

        if (generateOnStart)
        {
            GenerateAndApplyShootingAngle();
        }

        if (keepGenerating)
        {
            StartCoroutine(GenerateAnglesRoutine());
        }
    }

    private IEnumerator GenerateAnglesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(RandomFromRange(changeInterval));
            GenerateAndApplyShootingAngle();
        }
    }

    public void GenerateAndApplyShootingAngle()
    {
        GenerateShootingAngles();
        SelectClearestShootingAngle();
        ApplyCameraDistance(selectedShootingAngle);
        ChangeToShootingAngle(selectedShootingAngle);
    }

    private void GenerateShootingAngles()
    {
        if (shootingAngles == null || shootingAngles.Length != AngleCount)
        {
            shootingAngles = new float[AngleCount];
        }

        for (int attempt = 0; attempt < MaxRandomAttempts; attempt++)
        {
            for (int i = 0; i < AngleCount; i++)
            {
                shootingAngles[i] = Random.Range(-180f, 180f);
            }

            if (AnglesAreFarEnoughApart())
            {
                ShuffleAngles();
                return;
            }
        }

        GenerateEvenFallbackAngles();
        ShuffleAngles();
    }

    private bool AnglesAreFarEnoughApart()
    {
        for (int i = 0; i < shootingAngles.Length; i++)
        {
            for (int j = i + 1; j < shootingAngles.Length; j++)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(shootingAngles[i], shootingAngles[j])) < minimumAngleSeparation)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void GenerateEvenFallbackAngles()
    {
        float startAngle = Random.Range(-180f, 180f);

        for (int i = 0; i < AngleCount; i++)
        {
            shootingAngles[i] = NormalizeAngle(startAngle + i * 120f);
        }
    }

    private void ShuffleAngles()
    {
        for (int i = 0; i < shootingAngles.Length - 1; i++)
        {
            int swapIndex = Random.Range(i, shootingAngles.Length);
            float angle = shootingAngles[i];
            shootingAngles[i] = shootingAngles[swapIndex];
            shootingAngles[swapIndex] = angle;
        }
    }

    private void SelectClearestShootingAngle()
    {
        selectedShootingAngle = shootingAngles[0];
        selectedShotOccupiedPercent = GetShotOccupiedPercent(selectedShootingAngle);

        for (int i = 1; i < shootingAngles.Length; i++)
        {
            float occupiedPercent = GetShotOccupiedPercent(shootingAngles[i]);
            if (occupiedPercent < selectedShotOccupiedPercent)
            {
                selectedShootingAngle = shootingAngles[i];
                selectedShotOccupiedPercent = occupiedPercent;
            }
        }
    }

    public bool IsShotClear(float yAngle)
    {
        if (!TryGetShotBox(yAngle, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation))
        {
            return false;
        }

        Collider[] hits = Physics.OverlapBox(center, halfExtents, rotation, shotBlockLayers, QueryTriggerInteraction.Ignore);
        return hits.Length == 0;
    }

    public float GetShotOccupiedPercent(float yAngle)
    {
        if (!TryGetShotBox(yAngle, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation))
        {
            return 100f;
        }

        int samplesX = Mathf.Max(1, occupancySamples.x);
        int samplesY = Mathf.Max(1, occupancySamples.y);
        int samplesZ = Mathf.Max(1, occupancySamples.z);
        int totalSamples = samplesX * samplesY * samplesZ;
        int occupiedSamples = 0;

        Vector3 cellSize = new Vector3(halfExtents.x * 2f / samplesX, halfExtents.y * 2f / samplesY, halfExtents.z * 2f / samplesZ);
        Vector3 cellHalfExtents = cellSize * 0.45f;

        for (int x = 0; x < samplesX; x++)
        {
            for (int y = 0; y < samplesY; y++)
            {
                for (int z = 0; z < samplesZ; z++)
                {
                    Vector3 localPoint = new Vector3(
                        -halfExtents.x + cellSize.x * (x + 0.5f),
                        -halfExtents.y + cellSize.y * (y + 0.5f),
                        -halfExtents.z + cellSize.z * (z + 0.5f));
                    Vector3 worldPoint = center + rotation * localPoint;

                    if (Physics.CheckBox(worldPoint, cellHalfExtents, rotation, shotBlockLayers, QueryTriggerInteraction.Ignore))
                    {
                        occupiedSamples++;
                    }
                }
            }
        }

        return (float)occupiedSamples / totalSamples * 100f;
    }

    private bool TryGetShotBox(float yAngle, out Vector3 center, out Vector3 halfExtents, out Quaternion rotation)
    {
        center = transform.position;
        halfExtents = Vector3.zero;
        rotation = Quaternion.identity;

        Vector3 cameraPosition = GetCandidateCameraPosition(yAngle);
        Vector3 cameraToTarget = transform.position - cameraPosition;
        float cameraToTargetDistance = cameraToTarget.magnitude;
        if (cameraToTargetDistance <= Mathf.Epsilon)
        {
            return false;
        }

        rotation = Quaternion.LookRotation(cameraToTarget.normalized, Vector3.up);

        Vector3 velocity = frisbeeRigidbody != null ? frisbeeRigidbody.velocity : Vector3.zero;
        Vector3 velocityExtension = velocity * shotBoxSecondsAhead;
        Vector3 localVelocityExtension = Quaternion.Inverse(rotation) * velocityExtension;

        center = (cameraPosition + transform.position) * 0.5f + velocityExtension * 0.5f;
        halfExtents = new Vector3(
            shotBoxThickness * 0.5f + Mathf.Abs(localVelocityExtension.x) * 0.5f,
            shotBoxThickness * 0.5f + Mathf.Abs(localVelocityExtension.y) * 0.5f,
            cameraToTargetDistance * 0.5f + Mathf.Abs(localVelocityExtension.z) * 0.5f);

        lastShotBoxCenter = center;
        lastShotBoxHalfExtents = halfExtents;
        lastShotBoxRotation = rotation;
        return true;
    }

    private Vector3 GetCandidateCameraPosition(float yAngle)
    {
        Quaternion targetRotation = GetTargetRotationWithYAngle(yAngle);
        return transform.position + targetRotation * GetAdjustedFollowOffset(yAngle);
    }

    private Vector3 GetFollowOffset()
    {
        EnsureBaseFollowOffset();
        return baseFollowOffset;
    }

    private Vector3 GetAdjustedFollowOffset(float yAngle)
    {
        Vector3 followOffset = GetFollowOffset();
        followOffset.z = baseFollowOffset.z * GetCameraDistanceMultiplier(yAngle);
        return followOffset;
    }

    private void ApplyCameraDistance(float yAngle)
    {
        EnsureBaseFollowOffset();

        CinemachineTransposer transposer = GetTransposer();
        if (transposer == null)
        {
            return;
        }

        float targetZ = baseFollowOffset.z * GetCameraDistanceMultiplier(yAngle);
        if (cameraDistanceRoutine != null)
        {
            StopCoroutine(cameraDistanceRoutine);
        }

        cameraDistanceRoutine = StartCoroutine(ChangeCameraDistance(transposer, targetZ, cameraDistanceChangeSpeed));
    }

    private IEnumerator ChangeCameraDistance(CinemachineTransposer transposer, float targetZ, float unitsPerSecond)
    {
        Vector3 followOffset = transposer.m_FollowOffset;
        float startZ = followOffset.z;
        float duration = Mathf.Abs(targetZ - startZ) / Mathf.Max(Mathf.Epsilon, unitsPerSecond);

        if (duration <= Mathf.Epsilon)
        {
            followOffset.z = targetZ;
            transposer.m_FollowOffset = followOffset;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            followOffset = transposer.m_FollowOffset;
            followOffset.z = Mathf.Lerp(startZ, targetZ, Mathf.Clamp01(elapsed / duration));
            transposer.m_FollowOffset = followOffset;
            yield return null;
        }

        followOffset = transposer.m_FollowOffset;
        followOffset.z = targetZ;
        transposer.m_FollowOffset = followOffset;
    }

    private float GetCameraDistanceMultiplier(float yAngle)
    {
        Vector3 velocity = frisbeeRigidbody != null ? frisbeeRigidbody.velocity : Vector3.zero;
        if (velocity.sqrMagnitude <= Mathf.Epsilon)
        {
            currentCameraDistanceMultiplier = 1f;
            return currentCameraDistanceMultiplier;
        }

        Quaternion targetRotation = GetTargetRotationWithYAngle(yAngle);
        Vector3 cameraPosition = transform.position + targetRotation * GetFollowOffset();
        Vector3 cameraToTarget = transform.position - cameraPosition;
        if (cameraToTarget.sqrMagnitude <= Mathf.Epsilon)
        {
            currentCameraDistanceMultiplier = 1f;
            return currentCameraDistanceMultiplier;
        }

        float cameraAgainstVelocity = Mathf.InverseLerp(0f, -1f, Vector3.Dot(cameraToTarget.normalized, velocity.normalized));
        float speedAmount = Mathf.InverseLerp(0f, Mathf.Max(Mathf.Epsilon, speedForMaxCameraDistanceMultiplier), velocity.magnitude);

        float angleMultiplier = Mathf.Lerp(1f, Mathf.Max(1f, maxAgainstVelocityCameraDistanceMultiplier), cameraAgainstVelocity);
        float speedMultiplier = Mathf.Lerp(1f, Mathf.Max(1f, maxSpeedCameraDistanceMultiplier), speedAmount);
        currentCameraDistanceMultiplier = angleMultiplier * speedMultiplier;
        return currentCameraDistanceMultiplier;
    }

    private void CacheBaseFollowOffset()
    {
        CinemachineTransposer transposer = GetTransposer();
        if (transposer == null)
        {
            return;
        }

        baseFollowOffset = transposer.m_FollowOffset;
        hasBaseFollowOffset = true;
    }

    private void EnsureBaseFollowOffset()
    {
        if (!hasBaseFollowOffset)
        {
            CacheBaseFollowOffset();
        }
    }

    private CinemachineTransposer GetTransposer()
    {
        if (evaluatedCamera == null)
        {
            return null;
        }

        return evaluatedCamera.GetCinemachineComponent<CinemachineTransposer>();
    }

    private void ChangeToShootingAngle(float yAngle)
    {
        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
        }

        rotationRoutine = StartCoroutine(RotateToShootingAngle(yAngle, RandomFromRange(rotationSpeed)));
    }

    private IEnumerator RotateToShootingAngle(float yAngle, float degreesPerSecond)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = GetTargetRotationWithYAngle(yAngle);
        float angle = Quaternion.Angle(startRotation, endRotation);
        float duration = angle / Mathf.Max(Mathf.Epsilon, degreesPerSecond);

        if (duration <= Mathf.Epsilon)
        {
            transform.rotation = endRotation;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        transform.rotation = endRotation;
    }

    private Quaternion GetTargetRotationWithYAngle(float yAngle)
    {
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = yAngle;
        return Quaternion.Euler(eulerAngles);
    }

    private float RandomFromRange(Vector2 range)
    {
        return Random.Range(Mathf.Min(range.x, range.y), Mathf.Max(range.x, range.y));
    }

    private float NormalizeAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }

    private void OnDrawGizmos()
    {
        if (!visualizeTargetDirection)
        {
            return;
        }

        DrawTargetDirectionGizmo();
    }

    private void DrawTargetDirectionGizmo()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float length = Mathf.Max(0.01f, targetDirectionLength);
        float headLength = Mathf.Clamp(targetDirectionHeadLength, 0.01f, length);
        float headWidth = Mathf.Max(0.01f, targetDirectionHeadWidth);
        Vector3 tip = origin + direction * length;
        Vector3 headBase = tip - direction * headLength;
        Vector3 right = transform.right * headWidth;
        Vector3 up = transform.up * headWidth;

        Color previousColor = Gizmos.color;
        Gizmos.color = targetDirectionColor;

        Gizmos.DrawWireSphere(origin, headWidth * 0.35f);
        Gizmos.DrawLine(origin, tip);
        Gizmos.DrawLine(tip, headBase + right);
        Gizmos.DrawLine(tip, headBase - right);
        Gizmos.DrawLine(tip, headBase + up);
        Gizmos.DrawLine(tip, headBase - up);
        Gizmos.DrawLine(headBase + right, headBase + up);
        Gizmos.DrawLine(headBase + up, headBase - right);
        Gizmos.DrawLine(headBase - right, headBase - up);
        Gizmos.DrawLine(headBase - up, headBase + right);
        Gizmos.DrawWireSphere(tip, headWidth * 0.2f);

        Gizmos.color = previousColor;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawShotBoxGizmo || shootingAngles == null || shootingAngles.Length == 0)
        {
            return;
        }

        if (!TryGetShotBox(shootingAngles[0], out _, out _, out _))
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Matrix4x4 previousMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(lastShotBoxCenter, lastShotBoxRotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, lastShotBoxHalfExtents * 2f);
        Gizmos.matrix = previousMatrix;
    }
}
